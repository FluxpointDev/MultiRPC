namespace MultiRPC.Exceptions;

public class NotServerIPCException : Exception
{
    public override string Message => "This IPC is not a server!";
}