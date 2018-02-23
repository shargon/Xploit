using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace XPloit.Core.Dns
{
	internal class DnsClientAsyncState<TMessage> : IAsyncResult
		where TMessage : DnsMessageBase
	{
		internal List<DnsClientEndpointInfo> EndpointInfos;
		internal int EndpointInfoIndex;

		internal TMessage Query;
		internal byte[] QueryData;
		internal int QueryLength;

		internal DnsServer.SelectTsigKey TSigKeySelector;
		internal byte[] TSigOriginalMac;

		internal TMessage PartialMessage;
		internal List<TMessage> Responses;

		internal Timer Timer;
		internal bool TimedOut;

		private long _timeOutUtcTicks;

		internal long TimeRemaining
		{
			get
			{
				long res = (_timeOutUtcTicks - DateTime.UtcNow.Ticks) / TimeSpan.TicksPerMillisecond;
				return res > 0 ? res : 0;
			}
			set { _timeOutUtcTicks = DateTime.UtcNow.Ticks + value * TimeSpan.TicksPerMillisecond; }
		}

		internal System.Net.Sockets.Socket UdpClient;
		internal EndPoint UdpEndpoint;

		internal byte[] Buffer;

		internal TcpClient TcpClient;
		internal NetworkStream TcpStream;
		internal int TcpBytesToReceive;

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
			QueryData = null;

			if (Timer != null)
			{
				Timer.Dispose();
				Timer = null;
			}

			IsCompleted = true;
			if (_waitHandle != null)
				_waitHandle.Set();

			if (UserCallback != null)
				UserCallback(this);
		}

		public DnsClientAsyncState<TMessage> CreateTcpCloneWithoutCallback()
		{
			return
				new DnsClientAsyncState<TMessage>
				{
					EndpointInfos = EndpointInfos,
					EndpointInfoIndex = EndpointInfoIndex,
					Query = Query,
					QueryData = QueryData,
					QueryLength = QueryLength,
					TSigKeySelector = TSigKeySelector,
					TSigOriginalMac = TSigOriginalMac,
					Responses = Responses,
					_timeOutUtcTicks = _timeOutUtcTicks
				};
		}
	}
}