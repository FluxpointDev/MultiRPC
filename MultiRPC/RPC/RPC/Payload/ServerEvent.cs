using DiscordRPC.Converters;
using System;
using System.Runtime.Serialization;

namespace DiscordRPC.RPC.Payload
{
	/// <summary>
	/// See https://discordapp.com/developers/docs/topics/rpc#rpc-server-payloads-rpc-events for documentation
	/// </summary>
	public enum ServerEvent
	{
		/// <summary>
		/// Sent when the server is ready to accept messages
		/// </summary>
		[EnumValue("READY")]
		Ready,

		/// <summary>
		/// Sent when something bad has happened
		/// </summary>
		[EnumValue("ERROR")]
		Error,
        
	}
}
