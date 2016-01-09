using System.Collections.Generic;
using XPloit.Core.Helpers;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>Dynamic Host Configuration Protocol (DHCP) Information record</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc4701">RFC 4701</see>
	///   </para>
	/// </summary>
	public class DhcidRecord : DnsRecordBase
	{
		/// <summary>
		///   Record data
		/// </summary>
		public byte[] RecordData { get; private set; }

		internal DhcidRecord() {}

		/// <summary>
		///   Creates a new instance of the DhcidRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="recordData"> Record data </param>
		public DhcidRecord(string name, int timeToLive, byte[] recordData)
			: base(name, RecordType.Dhcid, RecordClass.INet, timeToLive)
		{
			RecordData = recordData ?? new byte[] { };
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			RecordData = DnsMessageBase.ParseByteData(resultData, ref startPosition, length);
		}

		internal override string RecordDataToString()
		{
            return BaseEncodingHelper.ToBase64String(RecordData);
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return RecordData.Length; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, RecordData);
		}
	}
}