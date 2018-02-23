using System.Collections.Generic;
using System.Net;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>L32</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc6742">RFC 6742</see>
	///   </para>
	/// </summary>
	public class L32Record : DnsRecordBase
	{
		/// <summary>
		///   The preference
		/// </summary>
		public ushort Preference { get; private set; }

		/// <summary>
		///   The Locator
		/// </summary>
		public uint Locator32 { get; private set; }

		internal L32Record() {}

		/// <summary>
		///   Creates a new instance of the L32Record class
		/// </summary>
		/// <param name="name"> Domain name of the host </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="preference"> The preference </param>
		/// <param name="locator32"> The Locator </param>
		public L32Record(string name, int timeToLive, ushort preference, uint locator32)
			: base(name, RecordType.L32, RecordClass.INet, timeToLive)
		{
			Preference = preference;
			Locator32 = locator32;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			Preference = DnsMessageBase.ParseUShort(resultData, ref startPosition);
			Locator32 = DnsMessageBase.ParseUInt(resultData, ref startPosition);
		}

		internal override string RecordDataToString()
		{
			return Preference + " " + new IPAddress(Locator32);
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 6; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, Preference);
			DnsMessageBase.EncodeUInt(messageData, ref currentPosition, Locator32);
		}
	}
}