using System;
using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>Domain name pointer</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc1035">RFC 1035</see>
	///   </para>
	/// </summary>
	public class PtrRecord : DnsRecordBase
	{
		/// <summary>
		///   Domain name the address points to
		/// </summary>
		public string PointerDomainName { get; private set; }

		internal PtrRecord() {}

		/// <summary>
		///   Creates a new instance of the PtrRecord class
		/// </summary>
		/// <param name="name"> Reverse name of the address </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="pointerDomainName"> Domain name the address points to </param>
		public PtrRecord(string name, int timeToLive, string pointerDomainName)
			: base(name, RecordType.Ptr, RecordClass.INet, timeToLive)
		{
			PointerDomainName = pointerDomainName ?? String.Empty;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			PointerDomainName = DnsMessageBase.ParseDomainName(resultData, ref startPosition);
		}

		internal override string RecordDataToString()
		{
			return PointerDomainName;
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return PointerDomainName.Length + 2; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeDomainName(messageData, offset, ref currentPosition, PointerDomainName, true, domainNames);
		}
	}
}