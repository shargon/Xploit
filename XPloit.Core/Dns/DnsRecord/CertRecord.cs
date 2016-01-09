using System.Collections.Generic;
using XPloit.Core.Dns.DnsSec;
using XPloit.Core.Helpers;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>Certificate storage record</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc4398">RFC 4398</see>
	///   </para>
	/// </summary>
	public class CertRecord : DnsRecordBase
	{
		/// <summary>
		///   Type of cert
		/// </summary>
		public enum CertType : ushort
		{
			/// <summary>
			///   None
			/// </summary>
			None = 0,

			/// <summary>
			///   <para>X.509 as per PKIX</para>
			///   <para>
			///     Defined in
			///     <see cref="!:http://tools.ietf.org/html/rfc4398">RFC 4398</see>
			///   </para>
			/// </summary>
			Pkix = 1,

			/// <summary>
			///   <para>SPKI certificate</para>
			///   <para>
			///     Defined in
			///     <see cref="!:http://tools.ietf.org/html/rfc4398">RFC 4398</see>
			///   </para>
			/// </summary>
			Spki = 2,

			/// <summary>
			///   <para>OpenPGP packet</para>
			///   <para>
			///     Defined in
			///     <see cref="!:http://tools.ietf.org/html/rfc4398">RFC 4398</see>
			///   </para>
			/// </summary>
			Pgp = 3,

			/// <summary>
			///   <para>The URL of an X.509 data object</para>
			///   <para>
			///     Defined in
			///     <see cref="!:http://tools.ietf.org/html/rfc4398">RFC 4398</see>
			///   </para>
			/// </summary>
			IPkix = 4,

			/// <summary>
			///   <para>The URL of an SPKI certificate</para>
			///   <para>
			///     Defined in
			///     <see cref="!:http://tools.ietf.org/html/rfc4398">RFC 4398</see>
			///   </para>
			/// </summary>
			ISpki = 5,

			/// <summary>
			///   <para>The fingerprint and URL of an OpenPGP packet</para>
			///   <para>
			///     Defined in
			///     <see cref="!:http://tools.ietf.org/html/rfc4398">RFC 4398</see>
			///   </para>
			/// </summary>
			IPgp = 6,

			/// <summary>
			///   <para>Attribute Certificate</para>
			///   <para>
			///     Defined in
			///     <see cref="!:http://tools.ietf.org/html/rfc4398">RFC 4398</see>
			///   </para>
			/// </summary>
			Acpkix = 7,

			/// <summary>
			///   <para>The URL of an Attribute Certificate</para>
			///   <para>
			///     Defined in
			///     <see cref="!:http://tools.ietf.org/html/rfc4398">RFC 4398</see>
			///   </para>
			/// </summary>
			IAcpkix = 8,

			/// <summary>
			///   <para>URI private</para>
			///   <para>
			///     Defined in
			///     <see cref="!:http://tools.ietf.org/html/rfc4398">RFC 4398</see>
			///   </para>
			/// </summary>
			Uri = 253,

			/// <summary>
			///   <para>OID private</para>
			///   <para>
			///     Defined in
			///     <see cref="!:http://tools.ietf.org/html/rfc4398">RFC 4398</see>
			///   </para>
			/// </summary>
			Oid = 254,
		}

		/// <summary>
		///   Type of the certificate data
		/// </summary>
		public CertType Type { get; private set; }

		/// <summary>
		///   Key tag
		/// </summary>
		public ushort KeyTag { get; private set; }

		/// <summary>
		///   Algorithm of the certificate
		/// </summary>
		public DnsSecAlgorithm Algorithm { get; private set; }

		/// <summary>
		///   Binary data of the certificate
		/// </summary>
		public byte[] Certificate { get; private set; }

		internal CertRecord() {}

		/// <summary>
		///   Creates a new instace of the CertRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="type"> Type of the certificate data </param>
		/// <param name="keyTag"> Key tag </param>
		/// <param name="algorithm"> Algorithm of the certificate </param>
		/// <param name="certificate"> Binary data of the certificate </param>
		public CertRecord(string name, int timeToLive, CertType type, ushort keyTag, DnsSecAlgorithm algorithm, byte[] certificate)
			: base(name, RecordType.Cert, RecordClass.INet, timeToLive)
		{
			Type = type;
			KeyTag = keyTag;
			Algorithm = algorithm;
			Certificate = certificate ?? new byte[] { };
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			Type = (CertType) DnsMessageBase.ParseUShort(resultData, ref startPosition);
			KeyTag = DnsMessageBase.ParseUShort(resultData, ref startPosition);
			Algorithm = (DnsSecAlgorithm) resultData[startPosition++];
			Certificate = DnsMessageBase.ParseByteData(resultData, ref startPosition, length - 5);
		}

		internal override string RecordDataToString()
		{
			return (ushort) Type
			       + " " + KeyTag
			       + " " + (byte) Algorithm
                   + " " + BaseEncodingHelper.ToBase64String(Certificate);
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 5 + Certificate.Length; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, (ushort) Type);
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, KeyTag);
			messageData[currentPosition++] = (byte) Algorithm;
			DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, Certificate);
		}
	}
}