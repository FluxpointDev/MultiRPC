using DiscordRPC.Helper;
using DiscordRPC.Message;
using DiscordRPC.IO;
using DiscordRPC.RPC.Commands;
using DiscordRPC.RPC.Payload;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DiscordRPC.RPC
{
    /// <summary>
    /// Communicates between the client and discord through RPC
    /// </summary>
    public class RpcConnection : IDisposable
	{
		/// <summary>
		/// Version of the RPC Protocol
		/// </summary>
		public static readonly int VERSION = 1;

		/// <summary>
		/// The rate of poll to the discord pipe.
		/// </summary>
		public static readonly int POLL_RATE = 1000;

		/// <summary>
		/// Should we send a null presence on the fairwells?
		/// </summary>
		private static readonly bool CLEAR_ON_SHUTDOWN = true;

		/// <summary>
		/// Should we work in a lock step manner? This option is semi-obsolete and may not work as expected.
		/// </summary>
		private static readonly bool LOCK_STEP = false;

		#region States

		/// <summary>
		/// The current state of the RPC connection
		/// </summary>
		public RpcState State { get { RpcState tmp = RpcState.Disconnected; lock (l_states) tmp = _state; return tmp; } }
		private RpcState _state;
		private readonly object l_states = new object();

		/// <summary>
		/// The configuration received by the Ready
		/// </summary>
		public Configuration Configuration { get { Configuration tmp = null; lock (l_config) tmp = _configuration; return tmp; } }
		private Configuration _configuration = null;
		private readonly object l_config = new object();

		private volatile bool aborting = false;
		private volatile bool shutdown = false;
		
		/// <summary>
		/// Indicates if the RPC connection is still running in the background
		/// </summary>
		public bool IsRunning { get { return thread != null; } }

		/// <summary>
		/// Forces the <see cref="Close"/> to call <see cref="Shutdown"/> instead, safely saying goodbye to Discord. 
		/// <para>This option helps prevents ghosting in applications where the Process ID is a host and the game is executed within the host (ie: the Unity3D editor). This will tell Discord that we have no presence and we are closing the connection manually, instead of waiting for the process to terminate.</para>
		/// </summary>
		public bool ShutdownOnly { get; set; }

		#endregion

		#region Privates

		private string applicationID;					//ID of the Discord APP
		private int processID;							//ID of the process to track

		private long nonce;								//Current command index

		private Thread thread;							//The current thread
		private INamedPipeClient namedPipe;

		private int targetPipe;							//The pipe to taget. Leave as -1 for any available pipe.

		private readonly object l_rtqueue = new object();		//Lock for the send queue
		private Queue<ICommand> _rtqueue;				//The send queue

		private readonly object l_rxqueue = new object();		//Lock for the receive queue
		private Queue<IMessage> _rxqueue;               //The receive queue

		private AutoResetEvent queueUpdatedEvent = new AutoResetEvent(false);
		private BackoffDelay delay;                     //The backoff delay before reconnecting.
		#endregion

		/// <summary>
		/// Creates a new instance of the RPC.
		/// </summary>
		/// <param name="applicationID">The ID of the Discord App</param>
		/// <param name="processID">The ID of the currently running process</param>
		/// <param name="targetPipe">The target pipe to connect too</param>
		/// <param name="client">The pipe client we shall use.</param>
		public RpcConnection(string applicationID, int processID, int targetPipe, INamedPipeClient client)
		{
			this.applicationID = applicationID;
			this.processID = processID;
			this.targetPipe = targetPipe;
			this.namedPipe = client;
			this.ShutdownOnly = true;

			delay = new BackoffDelay(500, 60 * 1000);
			_rtqueue = new Queue<ICommand>();
			_rxqueue = new Queue<IMessage>();
			
			nonce = 0;
		}
		
			
		private long GetNextNonce()
		{
			nonce += 1;
			return nonce;
		}

		#region Queues
		/// <summary>
		/// Enqueues a command
		/// </summary>
		/// <param name="command">The command to enqueue</param>
		internal void EnqueueCommand(ICommand command)
		{
			//We cannot add anything else if we are aborting or shutting down.
			if (aborting || shutdown) return;

			//Enqueue the set presence argument
			lock (l_rtqueue)
				_rtqueue.Enqueue(command);
		}

		/// <summary>
		/// Adds a message to the message queue. Does not copy the message, so besure to copy it yourself or dereference it.
		/// </summary>
		/// <param name="message">The message to add</param>
		private void EnqueueMessage(IMessage message)
		{
			lock (l_rxqueue)
				_rxqueue.Enqueue(message);
		}

		/// <summary>
		/// Dequeues a single message from the event stack. Returns null if none are available.
		/// </summary>
		/// <returns></returns>
		internal IMessage DequeueMessage()
		{
			lock (l_rxqueue)
			{
				//We have nothing, so just return null.
				if (_rxqueue.Count == 0) return null;

				//Get the value and remove it from the list at the same time
				return _rxqueue.Dequeue();
			}
		}

		/// <summary>
		/// Dequeues all messages from the event stack. 
		/// </summary>
		/// <returns></returns>
		internal IMessage[] DequeueMessages()
		{
			lock (l_rxqueue)
			{
				//Copy the messages into an array
				IMessage[] messages = _rxqueue.ToArray();

				//Clear the entire queue
				_rxqueue.Clear();

				//return the array
				return messages;
			}
		}
		#endregion
				
		/// <summary>
		/// Main thread loop
		/// </summary>
		private void MainLoop()
		{
			//initialize the pipe
			Console.WriteLine("Initializing Thread. Creating pipe object.");

			//Forever trying to connect unless the abort signal is sent
			//Keep Alive Loop
			while (!aborting && !shutdown)
			{
				try
				{
					//Wrap everything up in a try get
					//Dispose of the pipe if we have any (could be broken)
					if (namedPipe == null)
					{
                        Console.WriteLine("Something bad has happened with our pipe client!");
						aborting = true;
						return;
					}

                    //Connect to a new pipe
                    Console.WriteLine($"Connecting to the pipe through the {namedPipe.GetType().FullName}");
					if (namedPipe.Connect(targetPipe))
					{
						#region Connected
						//We connected to a pipe! Reset the delay
						Console.WriteLine("Connected to the pipe. Attempting to establish handshake...");
						EnqueueMessage(new ConnectionEstablishedMessage() { ConnectedPipe = namedPipe.ConnectedPipe });

						//Attempt to establish a handshake
						EstablishHandshake();
						Console.WriteLine("Connection Established. Starting reading loop...");

						//Continously iterate, waiting for the frame
						//We want to only stop reading if the inside tells us (mainloop), if we are aborting (abort) or the pipe disconnects
						// We dont want to exit on a shutdown, as we still have information
						PipeFrame frame;
						bool mainloop = true;
						while (mainloop && !aborting && !shutdown && namedPipe.IsConnected)
						{
							#region Read Loop

							//Iterate over every frame we have queued up, processing its contents
							if (namedPipe.ReadFrame(out frame))
							{
								#region Read Payload
								Console.WriteLine($"Read Payload: {frame.Opcode}");

								//Do some basic processing on the frame
								switch (frame.Opcode)
								{
									//We have been told by discord to close, so we will consider it an abort
									case Opcode.Close:

										ClosePayload close = frame.GetObject<ClosePayload>();
										Console.WriteLine($"We have been told to terminate by discord: ({close.Code}) {close.Reason}");
										EnqueueMessage(new CloseMessage() { Code = close.Code, Reason = close.Reason });
										mainloop = false;
										break;

									//We have pinged, so we will flip it and respond back with pong
									case Opcode.Ping:					
										Console.WriteLine("PING");
										frame.Opcode = Opcode.Pong;
										namedPipe.WriteFrame(frame);
										break;

									//We have ponged? I have no idea if Discord actually sends ping/pongs.
									case Opcode.Pong:															
										Console.WriteLine("PONG");
										break;

									//A frame has been sent, we should deal with that
									case Opcode.Frame:					
										if (shutdown)
										{
											//We are shutting down, so skip it
											Console.WriteLine("Skipping frame because we are shutting down.");
											break;
										}

										if (frame.Data == null)
										{
											//We have invalid data, thats not good.
											Console.WriteLine("We received no data from the frame so we cannot get the event payload!");
											break;
										}

										//We have a frame, so we are going to process the payload and add it to the stack
										EventPayload response = null;
										try { response = frame.GetObject<EventPayload>(); } catch (Exception e)
										{
											Console.WriteLine("Failed to parse event! " + e.Message);
											Console.WriteLine("Data: " + frame.Message);
										}

										if (response != null) ProcessFrame(response);
										break;
										

									default:
									case Opcode.Handshake:
										//We have a invalid opcode, better terminate to be safe
										Console.WriteLine($"Invalid opcode: {frame.Opcode}");
										mainloop = false;
										break;
								}

								#endregion
							}

							if (!aborting && namedPipe.IsConnected)
							{ 
								//Process the entire command queue we have left
								ProcessCommandQueue();

								//Wait for some time, or until a command has been queued up
								queueUpdatedEvent.WaitOne(POLL_RATE);
							}

							#endregion
						}
                        #endregion

                        Console.WriteLine($"Left main read loop for some reason. Aborting: {aborting}, Shutting Down: {shutdown}");
					}
					else
					{
						Console.WriteLine("Failed to connect for some reason.");
						EnqueueMessage(new ConnectionFailedMessage() { FailedPipe = targetPipe });
					}

					//If we are not aborting, we have to wait a bit before trying to connect again
					if (!aborting && !shutdown)
					{
						//We have disconnected for some reason, either a failed pipe or a bad reading,
						// so we are going to wait a bit before doing it again
						long sleep = delay.NextDelay();

						Console.WriteLine($"Waiting {sleep}ms before attempting to connect again");
						Thread.Sleep(delay.NextDelay());
					}
				}
				catch(InvalidPipeException e)
				{
					Console.WriteLine($"Invalid Pipe Exception: {e.Message}");
				}
				catch (Exception e)
				{
					Console.WriteLine($"Unhandled Exception: {e.GetType().FullName}");
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
				finally
				{
					//Disconnect from the pipe because something bad has happened. An exception has been thrown or the main read loop has terminated.
					if (namedPipe.IsConnected)
					{
						//Terminate the pipe
						Console.WriteLine("Closing the named pipe.");
						namedPipe.Close();
					}

					//Update our state
					SetConnectionState(RpcState.Disconnected);
				}
			}

			//We have disconnected, so dispose of the thread and the pipe.
			Console.WriteLine("Left Main Loop");
			if (namedPipe != null)
				namedPipe.Dispose();

			Console.WriteLine("Thread Terminated, no longer performing RPC connection.");
		}

		#region Reading

		/// <summary>Handles the response from the pipe and calls appropriate events and changes states.</summary>
		/// <param name="response">The response received by the server.</param>
		private void ProcessFrame(EventPayload response)
		{
			Console.WriteLine($"Handling Response. Cmd: {response.Command}, Event: {response.Event}");

			//Check if it is an error
			if (response.Event.HasValue && response.Event.Value == ServerEvent.Error)
			{
				//We have an error
				Console.WriteLine("Error received from the RPC");
				
				//Create the event objetc and push it to the queue
				ErrorMessage err = response.GetObject<ErrorMessage>();
				Console.WriteLine($"Server responded with an error message: ({err.Code.ToString()}) {err.Message}");

				//Enqueue the messsage and then end
				EnqueueMessage(err);
				return;
			}

			//Check if its a handshake
			if (State == RpcState.Connecting)
			{
				if (response.Command == Command.Dispatch && response.Event.HasValue && response.Event.Value == ServerEvent.Ready)
				{
					Console.WriteLine("Connection established with the RPC");
					SetConnectionState(RpcState.Connected);
					delay.Reset();

					//Prepare the object
					ReadyMessage ready = response.GetObject<ReadyMessage>();
					lock (l_config)
					{
						_configuration = ready.Configuration;
						ready.User.SetConfiguration(_configuration);
					}

					//Enqueue the message
					EnqueueMessage(ready);
					return;
				}
			}

			if (State == RpcState.Connected)
			{
				switch(response.Command)
				{
					//We were sent a Activity Update, better enqueue it
					case Command.SetActivity:
						if (response.Data == null)
						{
							EnqueueMessage(new PresenceMessage());
						}
						else
						{
							RichPresenceResponse rp = response.GetObject<RichPresenceResponse>();
							EnqueueMessage(new PresenceMessage(rp));
						}
						break;
						
						
					//we have no idea what we were sent
					default:
						Console.WriteLine($"Unkown frame was received! {response.Command}");
						return;
				}
				return;
			}

			Console.WriteLine($"Received a frame while we are disconnected. Ignoring. Cmd: {response.Command}, Event: {response.Event}");			
		}
		
		#endregion

		#region Writting
		
		private void ProcessCommandQueue()
		{
			//We are not ready yet, dont even try
			if (State != RpcState.Connected)
				return;

			//We are aborting, so we will just log a warning so we know this is probably only going to send the CLOSE
			if (aborting)
				Console.WriteLine("We have been told to write a queue but we have also been aborted.");

			//Prepare some variabels we will clone into with locks
			bool needsWriting = true;
			ICommand item = null;
			
			//Continue looping until we dont need anymore messages
			while (needsWriting && namedPipe.IsConnected)
			{
				lock (l_rtqueue)
				{
					//Pull the value and update our writing needs
					// If we have nothing to write, exit the loop
					needsWriting = _rtqueue.Count > 0;
					if (!needsWriting) break;	

					//Peek at the item
					item = _rtqueue.Peek();
				}

				//BReak out of the loop as soon as we send this item
				if (shutdown || (!aborting && LOCK_STEP))
					needsWriting = false;
				
				//Prepare the payload
				IPayload payload = item.PreparePayload(GetNextNonce());
				Console.WriteLine("Attempting to send payload: " + payload.Command);

				//Prepare the frame
				PipeFrame frame = new PipeFrame();
				if (item is CloseCommand)
				{
					//We have been sent a close frame. We better just send a handwave
					//Send it off to the server
					SendHandwave();

					//Queue the item
					Console.WriteLine("Handwave sent, ending queue processing.");
					lock (l_rtqueue) _rtqueue.Dequeue();

					//Stop sending any more messages
					return;
				}
				else
				{
					if (aborting)
					{
						//We are aborting, so just dequeue the message and dont bother sending it
						Console.WriteLine("- skipping frame because of abort.");
						lock (l_rtqueue) _rtqueue.Dequeue();
					}
					else
					{
						//Prepare the frame
						frame.SetObject(Opcode.Frame, item.PreparePayload(GetNextNonce()));

						//Write it and if it wrote perfectly fine, we will dequeue it
						Console.WriteLine("Sending payload: " + payload.Command);
						if (namedPipe.WriteFrame(frame))
						{
							//We sent it, so now dequeue it
							Console.WriteLine("Sent Successfully.");
							lock (l_rtqueue) _rtqueue.Dequeue();
						}
						else
						{
							//Something went wrong, so just giveup and wait for the next time around.
							Console.WriteLine("Something went wrong during writing!");
							return;
						}
					}
				}
			}
		}

		#endregion

		#region Connection

		/// <summary>
		/// Establishes the handshake with the server. 
		/// </summary>
		/// <returns></returns>
		private void EstablishHandshake()
		{
			Console.WriteLine("Attempting to establish a handshake...");

			//We are establishing a lock and not releasing it until we sent the handshake message.
			// We need to set the key, and it would not be nice if someone did things between us setting the key.
		
			//Check its state
			if (State != RpcState.Disconnected)
			{
				Console.WriteLine("State must be disconnected in order to start a handshake!");
				return;
			}

			//Send it off to the server
			Console.WriteLine("Sending Handshake...");				
			if (!namedPipe.WriteFrame(new PipeFrame(Opcode.Handshake, new Handshake() { Version = VERSION, ClientID = applicationID })))
			{
				Console.WriteLine("Failed to write a handshake.");
				return;
			}

			//This has to be done outside the lock
			SetConnectionState(RpcState.Connecting);
		}

		/// <summary>
		/// Establishes a fairwell with the server by sending a handwave.
		/// </summary>
		private void SendHandwave()
		{
			Console.WriteLine("Attempting to wave goodbye...");
    
			//Check its state
			if (State == RpcState.Disconnected)
			{
				Console.WriteLine("State must NOT be disconnected in order to send a handwave!");
				return;
			}
			
			//Send the handwave
			if (!namedPipe.WriteFrame(new PipeFrame(Opcode.Close, new Handshake() { Version = VERSION, ClientID = applicationID })))
			{
				Console.WriteLine("failed to write a handwave.");
				return;
            }

            Console.WriteLine("Goodbye sent.");
        }
		

		/// <summary>
		/// Attempts to connect to the pipe. Returns true on success
		/// </summary>
		/// <returns></returns>
		public bool AttemptConnection()
		{
			Console.WriteLine("Attempting a new connection");

			//The thread mustn't exist already
			if (thread != null)
			{
				Console.WriteLine("Cannot attempt a new connection as the previous connection thread is not null!");
				return false;
			}

			//We have to be in the disconnected state
			if (State != RpcState.Disconnected)
			{
				Console.WriteLine("Cannot attempt a new connection as the previous connection hasn't changed state yet.");
				return false;
			}

			if (aborting)
			{
				Console.WriteLine("Cannot attempt a new connection while aborting!");
				return false;
			}

            //Start the thread up
            thread = new Thread(MainLoop)
            {
                Name = "Discord IPC Thread",
                IsBackground = true
            };
            thread.Start();

			return true;
		}
		
		/// <summary>
		/// Sets the current state of the pipe, locking the l_states object for thread saftey.
		/// </summary>
		/// <param name="state">The state to set it too.</param>
		private void SetConnectionState(RpcState state)
		{
			Console.WriteLine($"Setting the connection state to {state.ToString().ToSnakeCase().ToUpperInvariant()}");
			lock (l_states)
			{
				_state = state;
			}
		}

		/// <summary>
		/// Closes the connection and disposes of resources. This will not force termination, but instead allow Discord disconnect us after we say goodbye. 
		/// <para>This option helps prevents ghosting in applications where the Process ID is a host and the game is executed within the host (ie: the Unity3D editor). This will tell Discord that we have no presence and we are closing the connection manually, instead of waiting for the process to terminate.</para>
		/// </summary>
		public void Shutdown()
		{
			//Enable the flag
			Console.WriteLine("Initiated shutdown procedure");
			shutdown = true;

			//Clear the commands and enqueue the close
			lock(l_rtqueue)
			{
				_rtqueue.Clear();
				if (CLEAR_ON_SHUTDOWN) _rtqueue.Enqueue(new PresenceCommand() { PID = processID, Presence = null });
				_rtqueue.Enqueue(new CloseCommand());
			}

			//Trigger the event
			queueUpdatedEvent.Set();
		}

		/// <summary>
		/// Closes the connection and disposes of resources.
		/// </summary>
		public void Close()
		{
			if (thread == null)
			{
				Console.WriteLine("Cannot close as it is not available!");
				return;
			}

			if (aborting)
			{
				Console.WriteLine("Cannot abort as it has already been aborted");
				return;
			}

			//Set the abort state
			if (ShutdownOnly)
			{
				Shutdown();
				return;
			}

			//Terminate
			Console.WriteLine("Updating Abort State...");
			aborting = true;
			queueUpdatedEvent.Set();
		}


		/// <summary>
		/// Closes the connection and disposes resources. Identical to <see cref="Close"/> but ignores the "ShutdownOnly" value.
		/// </summary>
		public void Dispose()
		{
			ShutdownOnly = false;
			Close();
		}
		#endregion

	}

	/// <summary>
	/// State of the RPC connection
	/// </summary>
	public enum RpcState
	{
		/// <summary>
		/// Disconnected from the discord client
		/// </summary>
		Disconnected,
		
		/// <summary>
		/// Connecting to the discord client. The handshake has been sent and we are awaiting the ready event
		/// </summary>
		Connecting,

		/// <summary>
		/// We are connect to the client and can send and receive messages.
		/// </summary>
		Connected
	}
}
