using System;
using System.Collections.Generic;
using System.Threading;

namespace XPloit.Core.Dns
{
	internal class DnsClientParallelAsyncState<TMessage> : IAsyncResult
		where TMessage : DnsMessageBase
	{
		internal int ResponsesToReceive;
		internal List<TMessage> Responses;

		internal AsyncCallback UserCallback;
		public object AsyncState { get; internal set; }
		public bool IsCompleted { get; private set; }

		public bool CompletedSynchronously
		{
			get { return false; }
		}

		private ManualResetEvent _waitHandle;

		public WaitHandle AsyncWaitHandle
		{
			get { return _waitHandle ?? (_waitHandle = new ManualResetEvent(IsCompleted)); }
		}

		internal void SetCompleted()
		{
			IsCompleted = true;

			if (_waitHandle != null)
				_waitHandle.Set();

			if (UserCallback != null)
				UserCallback(this);
		}
	}
}