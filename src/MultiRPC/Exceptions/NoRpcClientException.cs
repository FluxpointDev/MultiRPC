namespace MultiRPC.Exceptions;

public class NoRpcClientException : Exception
{
    public override string Message => "We didn't get RPC client";
}