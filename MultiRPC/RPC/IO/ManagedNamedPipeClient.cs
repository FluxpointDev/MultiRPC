using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;
using System.IO;

namespace DiscordRPC.IO
{
    /// <summary>
    /// A named pipe client using the .NET framework <see cref="NamedPipeClientStream"/>
    /// </summary>
    public class ManagedNamedPipeClient : INamedPipeClient
	{
		const string PIPE_NAME = @"discord-ipc-{0}";
        
		/// <summary>
		/// Checks if the client is connected
		/// </summary>
		public bool IsConnected
		{
			get
			{
				//This will trigger if the stream is disabled. This should prevent the lock check
				if (_isClosed) return false;
				lock (l_stream)
				{
					//We cannot be sure its still connected, so lets double check
					return _stream != null && _stream.IsConnected;
				}
			}
		}

		/// <summary>
		/// The pipe we are currently connected too.
		/// </summary>
		public int ConnectedPipe {  get { return _connectedPipe; } }

		private int _connectedPipe;
		private NamedPipeClientStream _stream;

		private byte[] _buffer = new byte[PipeFrame.MAX_SIZE];

		private Queue<PipeFrame> _framequeue = new Queue<PipeFrame>();
		private readonly object _framequeuelock = new object();

		private volatile bool _isDisposed = false;
		private volatile bool _isClosed = true;

		private readonly object l_stream = new object();

		/// <summary>
		/// Creates a new instance of a Managed NamedPipe client. Doesn't connect to anything yet, just setups the values.
		/// </summary>
		public ManagedNamedPipeClient()
		{
			_buffer = new byte[PipeFrame.MAX_SIZE];
			_stream = null;
		}

		/// <summary>
		/// Connects to the pipe
		/// </summary>
		/// <param name="pipe"></param>
		/// <returns></returns>
		public bool Connect(int pipe)
		{
			if (_isDisposed)
				throw new ObjectDisposedException("NamedPipe");

			if (pipe > 9)
				throw new ArgumentOutOfRangeException("pipe", "Argument cannot be greater than 9");

			//Attempt to connect to the specific pipe
			if (pipe >= 0 && AttemptConnection(pipe))
			{
				TBeginRead();
				return true;
			}

			//Iterate until we connect to a pipe
			for (int i = 0; i < 10; i++)
			{
				if (AttemptConnection(i))
				{
					TBeginRead();
					return true;
				}
			}

			//We failed to connect
			return false;
		}
		private bool AttemptConnection(int pipe)
		{
			if (_isDisposed)
				throw new ObjectDisposedException("_stream");

			//Prepare the pipe name
			string pipename = string.Format(PIPE_NAME, pipe);
			Console.WriteLine("Attempting to connect to " + pipename);

			try
			{
				//Create the client
				lock (l_stream)
				{
					_stream = new NamedPipeClientStream(".", pipename, PipeDirection.InOut, PipeOptions.Asynchronous);
					_stream.Connect(1000);

					//Spin for a bit while we wait for it to finish connecting
					Console.WriteLine("Waiting for connection...");
					do { Thread.Sleep(10); } while (!_stream.IsConnected);
				}

				//Store the value
				Console.WriteLine("Connected to " + pipename);
				_connectedPipe = pipe;
				_isClosed = false;
			}
			catch (Exception e)
			{
				//Something happened, try again
				//TODO: Log the failure condition
				Console.WriteLine($"Failed connection to {pipename}. {e.Message}");
				Close();
			}

			return !_isClosed;
		}

		/// <summary>
		/// Starts a read. Can be executed in another thread.
		/// </summary>
		private void TBeginRead()
		{
			if (_isClosed) return;
			try
			{
				lock (l_stream)
				{
					//Make sure the stream is valid
					if (_stream == null || !_stream.IsConnected) return;

					Console.WriteLine($"Begining Read of {_buffer.Length} bytes");
					_stream.BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(TEndRead), _stream.IsConnected);
				}
			}
			catch(ObjectDisposedException)
			{
				Console.WriteLine("Attempted to start reading from a disposed pipe");
				return;
			}
			catch (InvalidOperationException)
			{
				//The pipe has been closed
				Console.WriteLine("Attempted to start reading from a closed pipe");
				return;
			}
			catch (Exception e)
			{
				Console.WriteLine($"An exception occured while starting to read a stream: {e.Message}");
				Console.WriteLine(e.StackTrace);
			}
		}

		/// <summary>
		/// Ends a read. Can be executed in another thread.
		/// </summary>
		/// <param name="callback"></param>
		private void TEndRead(IAsyncResult callback)
		{
			Console.WriteLine("Ending Read");
			int bytes = 0;

			try
			{
				//Attempt to read the bytes, catching for IO exceptions or dispose exceptions
				lock (l_stream)
				{
					//Make sure the stream is still valid
					if (_stream == null || !_stream.IsConnected) return;

					//Read our btyes
					bytes = _stream.EndRead(callback);
				}
			}
			catch (IOException)
			{
				Console.WriteLine("Attempted to end reading from a closed pipe");
				return;
			}
			catch(NullReferenceException)
			{
				Console.WriteLine("Attempted to read from a null pipe");
				return;
			}
			catch(ObjectDisposedException)
			{
				Console.WriteLine("Attemped to end reading from a disposed pipe");
				return;
			}
			catch(Exception e)
			{
				Console.WriteLine($"An exception occured while ending a read of a stream: {e.Message}");
				Console.WriteLine(e.StackTrace);
				return;
			}

			//How much did we read?
			Console.WriteLine($"Read {bytes} bytes");

			//Did we read anything? If we did we should enqueue it.
			if (bytes > 0)
			{
				//Load it into a memory stream and read the frame
				using (MemoryStream memory = new MemoryStream(_buffer, 0, bytes))
				{
					try
					{
						PipeFrame frame = new PipeFrame();
						if (frame.ReadStream(memory))
						{
							Console.WriteLine($"Read a frame: {frame.Opcode}");

							//Enqueue the stream
							lock (_framequeuelock)
								_framequeue.Enqueue(frame);
						}
						else
						{
							//TODO: Enqueue a pipe close event here as we failed to read something.
							Console.WriteLine("Pipe failed to read from the data received by the stream.");
							Close();
						}
					}
					catch (Exception e)
					{
						Console.WriteLine("A exception has occured while trying to parse the pipe data: " + e.Message);
						Close();
					}
				}
			}

			//We are still connected, so continue to read
			if (!_isClosed && IsConnected)
			{
				Console.WriteLine("Starting another read");
				TBeginRead();
			}
		}

		/// <summary>
		/// Reads a frame, returning false if none are available
		/// </summary>
		/// <param name="frame"></param>
		/// <returns></returns>
		public bool ReadFrame(out PipeFrame frame)
		{
			if (_isDisposed)
				throw new ObjectDisposedException("_stream");

			//Check the queue, returning the pipe if we have anything available. Otherwise null.
			lock(_framequeuelock)
			{
				if (_framequeue.Count == 0)
				{
					//We found nothing, so just default and return null
					frame = default(PipeFrame);
					return false;
				}

				//Return the dequed frame
				frame = _framequeue.Dequeue();
				return true;
			}
		}

		/// <summary>
		/// Writes a frame to the pipe
		/// </summary>
		/// <param name="frame"></param>
		/// <returns></returns>
		public bool WriteFrame(PipeFrame frame)
		{
			if (_isDisposed)
				throw new ObjectDisposedException("_stream");

			//Write the frame. We are assuming proper duplex connection here
			if (_isClosed || !IsConnected)
			{
				Console.WriteLine("Failed to write frame because the stream is closed");
				return false;
			}

			try
			{
				//Write the pipe
				//This can only happen on the main thread so it should be fine.
				frame.WriteStream(_stream);
				return true;
			}
			catch(IOException io)
			{
				Console.WriteLine($"Failed to write frame because of a IO Exception: {io.Message}");
			}
			catch (ObjectDisposedException)
			{
				Console.WriteLine("Failed to write frame as the stream was already disposed");
			}
			catch (InvalidOperationException)
			{
				Console.WriteLine("Failed to write frame because of a invalid operation");
			}

			//We must have failed the try catch
			return false;
		}
		
		/// <summary>
		/// Closes the pipe
		/// </summary>
		public void Close()
		{
			//If we are already closed, jsut exit
			if (_isClosed)
			{
				Console.WriteLine("Tried to close a already closed pipe.");
				return;
			}

			//flush and dispose			
			try
			{
				//Wait for the stream object to become available.
				lock (l_stream)
				{
					if (_stream != null)
					{
						try
						{
                            //Stream isn't null, so flush it and then dispose of it.\
                            // We are doing a catch here because it may throw an error during this process and we dont care if it fails.
                            _stream.Flush();
							_stream.Dispose();
						}
						catch
						{
                            //We caught an error, but we dont care anyways because we are disposing of the stream.
						}

						//Make the stream null and set our flag.
						_stream = null;
						_isClosed = true;
					}
					else
					{
						//The stream is already null?
						Console.WriteLine("Stream was closed, but no stream was available to begin with!");
					}
				}
			}
			catch (ObjectDisposedException)
			{
				//ITs already been disposed
				Console.WriteLine("Tried to dispose already disposed stream");
			}
			finally
			{
				//For good measures, we will mark the pipe as closed anyways
				_isClosed = true;
				_connectedPipe = -1;
			}
		}

		/// <summary>
		/// Disposes of the stream
		/// </summary>
		public void Dispose()
		{
			//Prevent double disposing
			if (_isDisposed) return;

			//Close the stream (disposing of it too)
			if (!_isClosed) Close();

			//Dispose of the stream if it hasnt been destroyed already.
			lock(l_stream)
			{
				if (_stream != null)
				{
					_stream.Dispose();
					_stream = null;
				}
			}

			//Set our dispose flag
			_isDisposed = true;
		}		
	}
}
