﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace XPloit.Core.Dns
{
	/// <summary>
	///   Provides a one/shot client for querying Multicast DNS as defined in
	///   <see
	///     cref="!:http://www.ietf.org/id/draft-cheshire-dnsext-multicastdns-15.txt">
	///     draft-cheshire-dnsext-multicastdns-15
	///   </see>
	///   .
	/// </summary>
	public class MulticastDnsOneShotClient : DnsClientBase
	{
		private static readonly List<IPAddress> _addresses = new List<IPAddress> { IPAddress.Parse("FF02::FB"), IPAddress.Parse("224.0.0.251") };

		/// <summary>
		///   Provides a new instance with a timeout of 2.5 seconds
		/// </summary>
		public MulticastDnsOneShotClient()
			: this(2500) {}

		/// <summary>
		///   Provides a new instance with a custom timeout
		/// </summary>
		/// <param name="queryTimeout"> Query timeout in milliseconds </param>
		public MulticastDnsOneShotClient(int queryTimeout)
			: base(_addresses, queryTimeout, 5353)
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
		///   Queries for specified name and all records (RecordType.Any).
		/// </summary>
		/// <param name="name"> Domain, that should be queried </param>
		/// <returns> All available responses on the local network </returns>
		public List<MulticastDnsMessage> Resolve(string name)
		{
			return Resolve(name, RecordType.Any);
		}

		/// <summary>
		///   Queries for specified records.
		/// </summary>
		/// <param name="name"> Name, that should be queried </param>
		/// <param name="recordType"> Type the should be queried </param>
		/// <returns> All available responses on the local network </returns>
		public List<MulticastDnsMessage> Resolve(string name, RecordType recordType)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentException("Name must be provided", "name");
			}

			MulticastDnsMessage message = new MulticastDnsMessage { IsQuery = true, OperationCode = OperationCode.Query };
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
			return BeginResolve(name, RecordType.Any, requestCallback, state);
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

			MulticastDnsMessage message = new MulticastDnsMessage { IsQuery = true, OperationCode = OperationCode.Query };
			message.Questions.Add(new DnsQuestion(name, recordType, RecordClass.INet));

			return BeginSendMessageParallel(message, requestCallback, state);
		}

		/// <summary>
		///   Ends a pending asynchronous operation.
		/// </summary>
		/// <param name="ar">
		///   An <see cref="System.IAsyncResult" /> object returned by a call to
		///   <see
		///     cref="MulticastDnsOneShotClient.BeginResolve" />
		///   .
		/// </param>
		/// <returns> All available responses on the local network </returns>
		public List<MulticastDnsMessage> EndResolve(IAsyncResult ar)
		{
			return EndSendMessageParallel<MulticastDnsMessage>(ar);
		}
	}
}