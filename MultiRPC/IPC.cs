using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TinyUpdate.Core.Logging;

namespace MultiRPC
{
    //Works on top of this example
    //https://github.com/davidfowl/TcpEcho
    public class IPC
    {
        private Socket? _socket;
        private NetworkStream? _networkStream;
        private readonly Encoding _ipcEncoding = Encoding.Unicode;
        private readonly ILogging _logger = LoggingCreator.CreateLogger(nameof(IPC));
        public IPC(string name)
        {
            Name = name;
        }
        
        /// <summary>
        /// The name of the IPC connection
        /// </summary>
        private string Name { get; }

        /// <summary>
        /// What this IPC is acting as (Server/Client)
        /// </summary>
        public IPCType Type { get; private set; }

        public event EventHandler<string>? NewMessage; 

        /// <summary>
        /// Gets or makes a connection to a IPC server based on if one yet exists
        /// </summary>
        public static IPC GetOrMakeConnection()
        {
            //We want to run IPC logic in it's own thread
            var ipcCon = new IPC("MultiRPC")
            {
                Type = IPCType.Client
            };
            var thread = new Thread(() =>
            {
                if (!ipcCon.ConnectToServer())
                {
                    ipcCon.Type = IPCType.Server;
                    ipcCon.StartServer();
                }
            });
            thread.Name = "IPC Thread";
            thread.IsBackground = true;
            thread.Start();
            
            return ipcCon;
        }

        public void StartServer()
        {
            if (Type != IPCType.Server)
            {
                throw new Exception("This IPC is not a server!");
            }
            
            StopServer();
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(_endPoint);
            _socket.Listen(120);
            _ = WaitForConnection();
        }
        
        public void StopServer()
        {
            _socket?.Dispose();
            _socket = null;
        }

        private readonly IPEndPoint _endPoint = new IPEndPoint(IPAddress.Loopback, 8087);
        public bool ConnectToServer()
        {
            if (Type != IPCType.Client)
            {
                throw new Exception("This IPC is not a client!");
            }

            DisconnectFromServer();

            try
            {
                _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                _logger.Debug("Connecting to port 8087");
                _socket.Connect(_endPoint);
                _networkStream = new NetworkStream(_socket);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            return false;
        }

        public void DisconnectFromServer()
        {
            if (Type != IPCType.Client)
            {
                throw new Exception("This IPC is not a client!");
            }
            
            _networkStream?.Dispose();
            _networkStream = null;
            _socket?.Dispose();
            _socket = null;
        }

        public bool SendMessage(string message)
        {
            if (Type != IPCType.Client)
            {
                throw new Exception("This IPC is not a client!");
            }

            var b = _ipcEncoding.GetBytes(message + "\n");
            _networkStream?.Write(b);
            return true;
        }

        private async Task WaitForConnection()
        {
            var socket = await _socket!.AcceptAsync();
            _ = ProcessLinesAsync(socket);

            if (_socket != null)
            {
                _ = WaitForConnection();
            }
        }

        private async Task ProcessLinesAsync(Socket socket)
        {
            Console.WriteLine($"[{socket.RemoteEndPoint}]: connected");

            // Create a PipeReader over the network stream
            var stream = new NetworkStream(socket);
            var reader = PipeReader.Create(stream);

            while (true)
            {
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;

                while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
                {
                    // Process the line.
                    ProcessLine(line);
                }

                // Tell the PipeReader how much of the buffer has been consumed.
                reader.AdvanceTo(buffer.Start, buffer.End);

                // Stop reading if there's no more data coming.
                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Mark the PipeReader as complete.
            await reader.CompleteAsync();

            Console.WriteLine($"[{socket.RemoteEndPoint}]: disconnected");
        }
        
        private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            // Look for a EOL in the buffer.
            SequencePosition? position = buffer.PositionOf((byte)'\n');

            if (position == null)
            {
                line = default;
                return false;
            }

            // Skip the line + the \n.
            line = buffer.Slice(0, position.Value);
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
            return true;
        }

        private void ProcessLine(in ReadOnlySequence<byte> buffer)
        {
            foreach (var segment in buffer)
            {
                NewMessage?.Invoke(this, _ipcEncoding.GetString(segment.Span));
            }
        }
    }

    public enum IPCType
    {
        Server,
        Client
    }
}