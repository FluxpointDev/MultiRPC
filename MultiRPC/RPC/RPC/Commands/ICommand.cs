using DiscordRPC.RPC.Payload;

namespace DiscordRPC.RPC.Commands
{
    interface ICommand
	{
		IPayload PreparePayload(long nonce);
	}
}
