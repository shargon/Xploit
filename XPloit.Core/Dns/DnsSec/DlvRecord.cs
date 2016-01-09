using System.Collections.Generic;
using XPloit.Core.Dns.DnsRecord;
using XPloit.Core.Helpers;

namespace XPloit.Core.Dns.DnsSec
{
	/// <summary>
	///   <para>DNSSEC lookaside validation</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc4431">RFC 4431</see>
	///   </para>
	/// </summary>
	public class DlvRecord : DnsRecordBase
	{
		/// <summary>
		///   Key tag
		/// </summary>
		public ushort KeyTag { get; private set; }

		/// <summary>
		///   Algorithm used
		/// </summary>
		public DnsSecAlgorithm Algorithm { get; private set; }

		/// <summary>
		///   Type of the digest
		/// </summary>
		public DnsSecDigestType DigestType { get; private set; }

		/// <summary>
		///   Binary data of the digest
		/// </summary>
		public byte[] Digest { get; private set; }

		internal DlvRecord() {}

		/// <summary>
		///   Creates a new instance of the DlvRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="recordClass"> Class of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="keyTag"> Key tag </param>
		/// <param name="algorithm"> Algorithm used </param>
		/// <param name="digestType"> Type of the digest </param>
		/// <param name="digest"> Binary data of the digest </param>
		public DlvRecord(string name, RecordClass recordClass, int timeToLive, ushort keyTag, DnsSecAlgorithm algorithm, DnsSecDigestType digestType, byte[] digest)
			: base(name, RecordType.Dlv, recordClass, timeToLive)
		{
			KeyTag = keyTag;
			Algorithm = algorithm;
			DigestType = digestType;
			Digest = digest ?? new byte[] { };
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			KeyTag = DnsMessageBase.ParseUShort(resultData, ref startPosition);
			Algorithm = (DnsSecAlgorithm) resultData[startPosition++];
			DigestType = (DnsSecDigestType) resultData[startPosition++];
			Digest = DnsMessageBase.ParseByteData(resultData, ref startPosition, length - 4);
		}

		internal override string RecordDataToString()
		{
			return KeyTag
			       + " " + (byte) Algorithm
			       + " " + (byte) DigestType
                   + " " + BaseEncodingHelper.ToBase64String(Digest);
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 4 + Digest.Length; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, KeyTag);
			messageData[currentPosition++] = (byte) Algorithm;
			messageData[currentPosition++] = (byte) DigestType;
			DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, Digest);
		}
	}
}