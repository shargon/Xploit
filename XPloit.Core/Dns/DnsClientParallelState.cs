using System;

namespace XPloit.Core.Dns
{
	internal class DnsClientParallelState<TMessage>
		where TMessage : DnsMessageBase
	{
		internal object Lock = new object();
		internal IAsyncResult SingleMessageAsyncResult;
		internal DnsClientParallelAsyncState<TMessage> ParallelMessageAsyncState;
	}
}