using System;

namespace XPloit.Core.Dns
{
	/// <summary>
	///   Event arguments of <see cref="DnsServer.InvalidSignedMessageReceived" /> event.
	/// </summary>
	public class InvalidSignedMessageEventArgs : EventArgs
	{
		/// <summary>
		///   Original message, which the client provided
		/// </summary>
		public DnsMessageBase Message { get; set; }

		internal InvalidSignedMessageEventArgs(DnsMessageBase message)
		{
			Message = message;
		}
	}
}