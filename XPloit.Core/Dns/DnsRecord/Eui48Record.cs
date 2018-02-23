using System;
using System.Collections.Generic;
using System.Linq;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>EUI48</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc7043">RFC 7043</see>
	///   </para>
	/// </summary>
	public class Eui48Record : DnsRecordBase
	{
		/// <summary>
		///   IP address of the host
		/// </summary>
		public byte[] Address { get; private set; }

		internal Eui48Record() {}

		/// <summary>
		///   Creates a new instance of the Eui48Record class
		/// </summary>
		/// <param name="name"> Domain name of the host </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="address"> The EUI48 address</param>
		public Eui48Record(string name, int timeToLive, byte[] address)
			: base(name, RecordType.Eui48, RecordClass.INet, timeToLive)
		{
			Address = address ?? new byte[6];
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			Address = DnsMessageBase.ParseByteData(resultData, ref startPosition, 6);
		}

		internal override string RecordDataToString()
		{
			return String.Join("-", Address.Select(x => x.ToString("x2")).ToArray());
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 6; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, Address);
		}
	}
}