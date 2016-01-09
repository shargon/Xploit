using System;
using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>Canonical name for an alias</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc1035">RFC 1035</see>
	///   </para>
	/// </summary>
	public class CNameRecord : DnsRecordBase
	{
		/// <summary>
		///   Canonical name
		/// </summary>
		public string CanonicalName { get; private set; }

		internal CNameRecord() {}

		/// <summary>
		///   Creates a new instance of the CNameRecord class
		/// </summary>
		/// <param name="name"> Domain name the host </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="canonicalName"> Canocical name for the alias of the host </param>
		public CNameRecord(string name, int timeToLive, string canonicalName)
			: base(name, RecordType.CName, RecordClass.INet, timeToLive)
		{
			CanonicalName = canonicalName ?? String.Empty;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			CanonicalName = DnsMessageBase.ParseDomainName(resultData, ref startPosition);
		}

		internal override string RecordDataToString()
		{
			return CanonicalName;
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return CanonicalName.Length + 2; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeDomainName(messageData, offset, ref currentPosition, CanonicalName, true, domainNames);
		}
	}
}