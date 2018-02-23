using System;
using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>Key exchanger record</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc2230">RFC 2230</see>
	///   </para>
	/// </summary>
	public class KxRecord : DnsRecordBase
	{
		/// <summary>
		///   Preference of the record
		/// </summary>
		public ushort Preference { get; private set; }

		/// <summary>
		///   Domain name of the exchange host
		/// </summary>
		public string Exchanger { get; private set; }

		internal KxRecord() {}

		/// <summary>
		///   Creates a new instance of the KxRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="preference"> Preference of the record </param>
		/// <param name="exchanger"> Domain name of the exchange host </param>
		public KxRecord(string name, int timeToLive, ushort preference, string exchanger)
			: base(name, RecordType.Kx, RecordClass.INet, timeToLive)
		{
			Preference = preference;
			Exchanger = exchanger ?? String.Empty;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			Preference = DnsMessageBase.ParseUShort(resultData, ref startPosition);
			Exchanger = DnsMessageBase.ParseDomainName(resultData, ref startPosition);
		}

		internal override string RecordDataToString()
		{
			return Preference
			       + " " + Exchanger;
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return Exchanger.Length + 4; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, Preference);
			DnsMessageBase.EncodeDomainName(messageData, offset, ref currentPosition, Exchanger, true, domainNames);
		}
	}
}