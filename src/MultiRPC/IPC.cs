using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MultiRPC.Exceptions;
using TinyUpdate.Core.Logging;

namespace MultiRPC;

//TODO: Use Name to check if this is a valid IPC
//Works on top of this example
//https://github.com/davidfowl/TcpEcho
public class IPC
{
    private Socket? _socket;
    private NetworkStream? _networkStream;
    private readonly Encoding _ipcEncoding = Encoding.Unicode;
    private readonly ILogging _logger = LoggingCreator.CreateLogger(nameof(IPC));
    private readonly IPEndPoint _endPoint = new IPEndPoint(IPAddress.Loopback, 8087);
    private IPC(string name)
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

    /// <summary>
    /// When we get a new message from a Client
    /// </summary>
    public event EventHandler<string>? NewMessage;

    private static readonly object GetLock = new object();
    /// <summary>
    /// Gets or makes a connection to a IPC server based on if one yet exists
    /// </summary>
    public static IPC GetOrMakeConnection(string name = "MultiRPC")
    {
        //We want to run IPC logic in it's own thread
        var ipcCon = new IPC(name)
        {
            Type = IPCType.Client
        };
        var thread = new Thread(() =>
        {
            lock (GetLock)
            {
                if (!ipcCon.ConnectToServer())
                {
                    ipcCon.DisconnectFromServer();
                    ipcCon.Type = IPCType.Server;
                    ipcCon.StartServer();
                }
            }
        });
        thread.Name = "IPC Thread";
        thread.IsBackground = true;
        thread.Start();

        Thread.Sleep(100);
        lock (GetLock)
        {
            return ipcCon;
        }
    }
    
    public bool ConnectToServer()
    {
        if (Type != IPCType.Client)
        {
            throw new NotClientIPCException();
        }

        DisconnectFromServer();
        try
        {
            _socket = MakeSocket();
            _logger.Information("Connecting to IPC Server ({0}:{1})", _endPoint.Address, _endPoint.Port);
            _socket.Connect(_endPoint);
            _networkStream = new NetworkStream(_socket);
            _logger.Information("Connected to IPC Server!");
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
        if (_socket == null)
        {
            return;
        }
            
        _logger.Information("Disconnecting from IPC Server!");
        _networkStream?.Dispose();
        _networkStream = null;
        _socket?.Dispose();
        _socket = null;
    }

    public bool SendMessage(string message)
    {
        if (Type != IPCType.Client)
        {
            throw new NotClientIPCException();
        }

        var b = _ipcEncoding.GetBytes(message + '\n');
        _networkStream?.Write(b);
        return true;
    }

    public void StartServer()
    {
        if (Type != IPCType.Server)
        {
            throw new NotServerIPCException();
        }
            
        StopServer();
        _logger.Information("Starting IPC Server");
        _socket = MakeSocket();
        _socket.Bind(_endPoint);
        _socket.Listen(120);
        _ = WaitForConnection();
        _logger.Information("IPC Server started!");
    }
        
    public void StopServer()
    {
        if (_socket == null)
        {
            return;
        }

        _logger.Information("Stopping IPC Server");
        _socket.Dispose();
        _socket = null;
    }

    private static Socket MakeSocket() => new Socket(SocketType.Stream, ProtocolType.Tcp);

    private async Task WaitForConnection()
    {
        if (_socket == null)
        {
            return;
        }

        var socket = await _socket.AcceptAsync();
        _ = ProcessLinesAsync(socket);

        if (_socket != null)
        {
            _ = WaitForConnection();
        }
    }

    private async Task ProcessLinesAsync(Socket socket)
    {
        _logger.Debug("[{0}]: connected", socket.RemoteEndPoint);

        // Create a PipeReader over the network stream
        var stream = new NetworkStream(socket);
        var reader = PipeReader.Create(stream);

        while (true)
        {
            ReadResult result;
            ReadOnlySequence<byte> buffer;
            try
            {
                result = await reader.ReadAsync();
                buffer = result.Buffer;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                break;
            }

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
                _logger.Debug("We finished reading all the data from the stream!");
                break;
            }
        }

        // Mark the PipeReader as complete.
        await reader.CompleteAsync();
        _logger.Debug("[{0}]: disconnected", socket.RemoteEndPoint);
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