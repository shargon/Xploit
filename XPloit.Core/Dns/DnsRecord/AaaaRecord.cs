using System.Collections.Generic;
using System.Net;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>IPv6 address</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc3596">RFC 3596</see>
	///   </para>
	/// </summary>
	public class AaaaRecord : DnsRecordBase, IAddressRecord
	{
		/// <summary>
		///   IP address of the host
		/// </summary>
		public IPAddress Address { get; private set; }

		internal AaaaRecord() {}

		/// <summary>
		///   Creates a new instance of the AaaaRecord class
		/// </summary>
		/// <param name="name"> Domain name of the host </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="address"> IP address of the host </param>
		public AaaaRecord(string name, int timeToLive, IPAddress address)
			: base(name, RecordType.Aaaa, RecordClass.INet, timeToLive)
		{
			Address = address ?? IPAddress.IPv6None;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			Address = new IPAddress(DnsMessageBase.ParseByteData(resultData, ref startPosition, 16));
		}

		internal override string RecordDataToString()
		{
			return Address.ToString();
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 16; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, Address.GetAddressBytes());
		}
	}
}