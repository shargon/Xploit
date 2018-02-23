using System.Collections.Generic;
using XPloit.Helpers;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>SSH key fingerprint record</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc4255">RFC 4255</see>
	///   </para>
	/// </summary>
	public class SshFpRecord : DnsRecordBase
	{
		/// <summary>
		///   Algorithm of the fingerprint
		/// </summary>
		public enum SshFpAlgorithm : byte
		{
			/// <summary>
			///   None
			/// </summary>
			None = 0,

			/// <summary>
			///   <para>RSA</para>
			///   <para>
			///     Defined in
			///     <see cref="!:http://tools.ietf.org/html/rfc4255">RFC 4255</see>
			///   </para>
			/// </summary>
			Rsa = 1,

			/// <summary>
			///   <para>DSA</para>
			///   <para>
			///     Defined in
			///     <see cref="!:http://tools.ietf.org/html/rfc4255">RFC 4255</see>
			///   </para>
			/// </summary>
			Dsa = 2,
		}

		/// <summary>
		///   Type of the fingerprint
		/// </summary>
		public enum SshFpFingerPrintType : byte
		{
			/// <summary>
			///   None
			/// </summary>
			None = 0,

			/// <summary>
			///   <para>SHA-1</para>
			///   <para>
			///     Defined in
			///     <see cref="!:http://tools.ietf.org/html/rfc4255">RFC 4255</see>
			///   </para>
			/// </summary>
			Sha1 = 1,
		}

		/// <summary>
		///   Algorithm of fingerprint
		/// </summary>
		public SshFpAlgorithm Algorithm { get; private set; }

		/// <summary>
		///   Type of fingerprint
		/// </summary>
		public SshFpFingerPrintType FingerPrintType { get; private set; }

		/// <summary>
		///   Binary data of the fingerprint
		/// </summary>
		public byte[] FingerPrint { get; private set; }

		internal SshFpRecord() {}

		/// <summary>
		///   Creates a new instance of the SshFpRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="algorithm"> Algorithm of fingerprint </param>
		/// <param name="fingerPrintType"> Type of fingerprint </param>
		/// <param name="fingerPrint"> Binary data of the fingerprint </param>
		public SshFpRecord(string name, int timeToLive, SshFpAlgorithm algorithm, SshFpFingerPrintType fingerPrintType, byte[] fingerPrint)
			: base(name, RecordType.SshFp, RecordClass.INet, timeToLive)
		{
			Algorithm = algorithm;
			FingerPrintType = fingerPrintType;
			FingerPrint = fingerPrint ?? new byte[] { };
		}

		internal override void ParseRecordData(byte[] resultData, int currentPosition, int length)
		{
			Algorithm = (SshFpAlgorithm) resultData[currentPosition++];
			FingerPrintType = (SshFpFingerPrintType) resultData[currentPosition++];
			FingerPrint = DnsMessageBase.ParseByteData(resultData, ref currentPosition, length - 2);
		}

		internal override string RecordDataToString()
		{
			return (byte) Algorithm
			       + " " + (byte) FingerPrintType
                   + " " + BaseEncodingHelper.ToBase16String(FingerPrint);
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 2 + FingerPrint.Length; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			messageData[currentPosition++] = (byte) Algorithm;
			messageData[currentPosition++] = (byte) FingerPrintType;
			DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, FingerPrint);
		}
	}
}