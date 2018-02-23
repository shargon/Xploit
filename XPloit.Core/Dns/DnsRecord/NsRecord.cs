using System;
using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>Authoritatitve name server record</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc1035">RFC 1035</see>
	///   </para>
	/// </summary>
	public class NsRecord : DnsRecordBase
	{
		/// <summary>
		///   Name of the authoritatitve nameserver for the zone
		/// </summary>
		public string NameServer { get; private set; }

		internal NsRecord() {}

		/// <summary>
		///   Creates a new instance of the NsRecord class
		/// </summary>
		/// <param name="name"> Domain name of the zone </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="nameServer"> Name of the authoritative name server </param>
		public NsRecord(string name, int timeToLive, string nameServer)
			: base(name, RecordType.Ns, RecordClass.INet, timeToLive)
		{
			NameServer = nameServer ?? String.Empty;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			NameServer = DnsMessageBase.ParseDomainName(resultData, ref startPosition);
		}

		internal override string RecordDataToString()
		{
			return NameServer;
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return NameServer.Length + 2; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeDomainName(messageData, offset, ref currentPosition, NameServer, true, domainNames);
		}
	}
}