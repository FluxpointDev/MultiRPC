namespace MultiRPC.Exceptions;

public class DesignException : Exception
{
    public override string Message => "This constructor can only be used for the Designer!!";
}