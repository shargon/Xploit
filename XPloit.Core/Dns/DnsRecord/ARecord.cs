using System.Collections.Generic;
using System.Net;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>Host address record</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc1035">RFC 1035</see>
	///   </para>
	/// </summary>
	public class ARecord : DnsRecordBase, IAddressRecord
	{
		/// <summary>
		///   IP address of the host
		/// </summary>
		public IPAddress Address { get; private set; }

		internal ARecord() {}

		/// <summary>
		///   Creates a new instance of the ARecord class
		/// </summary>
		/// <param name="name"> Domain name of the host </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="address"> IP address of the host </param>
		public ARecord(string name, int timeToLive, IPAddress address)
			: base(name, RecordType.A, RecordClass.INet, timeToLive)
		{
			Address = address ?? IPAddress.None;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			Address = new IPAddress(DnsMessageBase.ParseByteData(resultData, ref startPosition, 4));
		}

		internal override string RecordDataToString()
		{
			return Address.ToString();
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 4; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, Address.GetAddressBytes());
		}
	}
}