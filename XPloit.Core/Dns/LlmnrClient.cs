using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace XPloit.Core.Dns
{
	/// <summary>
	///   Provides a client for querying LLMNR (link-local multicast name resolution) as defined in
	///   <see
	///     cref="!:http://tools.ietf.org/html/rfc4795">
	///     RFC 4795
	///   </see>
	///   .
	/// </summary>
	public class LlmnrClient : DnsClientBase
	{
		private static readonly List<IPAddress> _addresses = new List<IPAddress> { IPAddress.Parse("FF02::1:3"), IPAddress.Parse("224.0.0.252") };

		/// <summary>
		///   Provides a new instance with a timeout of 1 second
		/// </summary>
		public LlmnrClient()
			: this(1000) {}

		/// <summary>
		///   Provides a new instance with a custom timeout
		/// </summary>
		/// <param name="queryTimeout"> Query timeout in milliseconds </param>
		public LlmnrClient(int queryTimeout)
			: base(_addresses, queryTimeout, 5355)
		{
			int maximumMessageSize = 0;

			try
			{
				maximumMessageSize = NetworkInterface.GetAllNetworkInterfaces()
				                                     .Where(n => n.SupportsMulticast && (n.NetworkInterfaceType != NetworkInterfaceType.Loopback) && (n.OperationalStatus == OperationalStatus.Up) && (n.Supports(NetworkInterfaceComponent.IPv4)))
				                                     .Select(n => n.GetIPProperties())
				                                     .Min(p => Math.Min(p.GetIPv4Properties().Mtu, p.GetIPv6Properties().Mtu));
			}
			catch {}

			_maximumMessageSize = Math.Max(512, maximumMessageSize);
		}

		private readonly int _maximumMessageSize;

		protected override int MaximumQueryMessageSize
		{
			get { return _maximumMessageSize; }
		}

		protected override bool AreMultipleResponsesAllowedInParallelMode
		{
			get { return true; }
		}

		/// <summary>
		///   Queries for specified records.
		/// </summary>
		/// <param name="name"> Domain, that should be queried </param>
		/// <returns> All available responses on the local network </returns>
		public List<LlmnrMessage> Resolve(string name)
		{
			return Resolve(name, RecordType.A);
		}

		/// <summary>
		///   Queries for specified records.
		/// </summary>
		/// <param name="name"> Name, that should be queried </param>
		/// <param name="recordType"> Type the should be queried </param>
		/// <returns> All available responses on the local network </returns>
		public List<LlmnrMessage> Resolve(string name, RecordType recordType)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentException("Name must be provided", "name");
			}

			LlmnrMessage message = new LlmnrMessage { IsQuery = true, OperationCode = OperationCode.Query };
			message.Questions.Add(new DnsQuestion(name, recordType, RecordClass.INet));

			return SendMessageParallel(message);
		}

		/// <summary>
		///   Queries for specified records asynchronously.
		/// </summary>
		/// <param name="name"> Name, that should be queried </param>
		/// <param name="requestCallback">
		///   An <see cref="System.AsyncCallback" /> delegate that references the method to invoked then the operation is complete.
		/// </param>
		/// <param name="state">
		///   A user-defined object that contains information about the receive operation. This object is passed to the
		///   <paramref
		///     name="requestCallback" />
		///   delegate when the operation is complete.
		/// </param>
		/// <returns>
		///   An <see cref="System.IAsyncResult" /> IAsyncResult object that references the asynchronous receive.
		/// </returns>
		public IAsyncResult BeginResolve(string name, AsyncCallback requestCallback, object state)
		{
			return BeginResolve(name, RecordType.A, requestCallback, state);
		}

		/// <summary>
		///   Queries for specified records asynchronously.
		/// </summary>
		/// <param name="name"> Name, that should be queried </param>
		/// <param name="recordType"> Type the should be queried </param>
		/// <param name="requestCallback">
		///   An <see cref="System.AsyncCallback" /> delegate that references the method to invoked then the operation is complete.
		/// </param>
		/// <param name="state">
		///   A user-defined object that contains information about the receive operation. This object is passed to the
		///   <paramref
		///     name="requestCallback" />
		///   delegate when the operation is complete.
		/// </param>
		/// <returns>
		///   An <see cref="System.IAsyncResult" /> IAsyncResult object that references the asynchronous receive.
		/// </returns>
		public IAsyncResult BeginResolve(string name, RecordType recordType, AsyncCallback requestCallback, object state)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentException("Name must be provided", "name");
			}

			LlmnrMessage message = new LlmnrMessage { IsQuery = true, OperationCode = OperationCode.Query };
			message.Questions.Add(new DnsQuestion(name, recordType, RecordClass.INet));

			return BeginSendMessageParallel(message, requestCallback, state);
		}

		/// <summary>
		///   Ends a pending asynchronous operation.
		/// </summary>
		/// <param name="ar">
		///   An <see cref="System.IAsyncResult" /> object returned by a call to
		///   <see
		///     cref="LlmnrClient.BeginResolve" />
		///   .
		/// </param>
		/// <returns> All available responses on the local network </returns>
		public List<LlmnrMessage> EndResolve(IAsyncResult ar)
		{
			return EndSendMessageParallel<LlmnrMessage>(ar);
		}
	}
}