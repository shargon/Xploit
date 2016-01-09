using System.Collections.Generic;
using XPloit.Core.Helpers;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>NSAP address, NSAP style A record</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc1706">RFC 1706</see>
	///     and
	///     <see cref="!:http://tools.ietf.org/html/rfc1348">RFC 1348</see>
	///   </para>
	/// </summary>
	public class NsapRecord : DnsRecordBase
	{
		/// <summary>
		///   Binary encoded NSAP data
		/// </summary>
		public byte[] RecordData { get; private set; }

		internal NsapRecord() {}

		/// <summary>
		///   Creates a new instance of the NsapRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="recordData"> Binary encoded NSAP data </param>
		public NsapRecord(string name, int timeToLive, byte[] recordData)
			: base(name, RecordType.Nsap, RecordClass.INet, timeToLive)
		{
			RecordData = recordData ?? new byte[] { };
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			RecordData = DnsMessageBase.ParseByteData(resultData, ref startPosition, length);
		}

		internal override string RecordDataToString()
		{
            return "0x" + BaseEncodingHelper.ToBase16String(RecordData);
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