namespace MultiRPC.Exceptions;

public class NotClientIPCException : Exception
{
    public override string Message => "This IPC is not a client!";
}