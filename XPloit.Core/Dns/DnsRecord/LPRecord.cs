using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>LP</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc6742">RFC 6742</see>
	///   </para>
	/// </summary>
	public class LPRecord : DnsRecordBase
	{
		/// <summary>
		///   The preference
		/// </summary>
		public ushort Preference { get; private set; }

		/// <summary>
		///   The FQDN
		/// </summary>
		public string FQDN { get; private set; }

		internal LPRecord() {}

		/// <summary>
		///   Creates a new instance of the LpRecord class
		/// </summary>
		/// <param name="name"> Domain name of the host </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="preference"> The preference </param>
		/// <param name="fqdn"> The FQDN </param>
		public LPRecord(string name, int timeToLive, ushort preference, string fqdn)
			: base(name, RecordType.LP, RecordClass.INet, timeToLive)
		{
			Preference = preference;
			FQDN = fqdn;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			Preference = DnsMessageBase.ParseUShort(resultData, ref startPosition);
			FQDN = DnsMessageBase.ParseDomainName(resultData, ref startPosition);
		}

		internal override string RecordDataToString()
		{
			return Preference + " " + FQDN;
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 4 + FQDN.Length; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, Preference);
			DnsMessageBase.EncodeDomainName(messageData, offset, ref currentPosition, FQDN, false, domainNames);
		}
	}
}