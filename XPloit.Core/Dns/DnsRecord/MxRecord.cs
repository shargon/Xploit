using System;
using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>Mail exchange</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc1035">RFC 1035</see>
	///   </para>
	/// </summary>
	public class MxRecord : DnsRecordBase
	{
		/// <summary>
		///   Preference of the record
		/// </summary>
		public ushort Preference { get; private set; }

		/// <summary>
		///   Host name of the mail exchanger
		/// </summary>
		public string ExchangeDomainName { get; private set; }

		internal MxRecord() {}

		/// <summary>
		///   Creates a new instance of the MxRecord class
		/// </summary>
		/// <param name="name"> Name of the zone </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="preference"> Preference of the record </param>
		/// <param name="exchangeDomainName"> Host name of the mail exchanger </param>
		public MxRecord(string name, int timeToLive, ushort preference, string exchangeDomainName)
			: base(name, RecordType.Mx, RecordClass.INet, timeToLive)
		{
			Preference = preference;
			ExchangeDomainName = exchangeDomainName ?? String.Empty;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			Preference = DnsMessageBase.ParseUShort(resultData, ref startPosition);
			ExchangeDomainName = DnsMessageBase.ParseDomainName(resultData, ref startPosition);
		}

		internal override string RecordDataToString()
		{
			return Preference
			       + " " + ExchangeDomainName;
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return ExchangeDomainName.Length + 4; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, Preference);
			DnsMessageBase.EncodeDomainName(messageData, offset, ref currentPosition, ExchangeDomainName, true, domainNames);
		}
	}
}