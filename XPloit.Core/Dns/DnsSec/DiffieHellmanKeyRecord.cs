using System.Collections.Generic;
using XPloit.Helpers;

namespace XPloit.Core.Dns.DnsSec
{
	/// <summary>
	///   <para>Security Key record using Diffie Hellman algorithm</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc4034">RFC 4034</see>
	///     ,
	///     <see cref="!:http://tools.ietf.org/html/rfc3755">RFC 3755</see>
	///     ,
	///     <see cref="!:http://tools.ietf.org/html/rfc2535">RFC 2535</see>
	///     and
	///     <see cref="!:http://tools.ietf.org/html/rfc2930">RFC 2930</see>
	///   </para>
	/// </summary>
	public class DiffieHellmanKeyRecord : KeyRecordBase
	{
		/// <summary>
		///   Binary data of the prime of the key
		/// </summary>
		public byte[] Prime { get; private set; }

		/// <summary>
		///   Binary data of the generator of the key
		/// </summary>
		public byte[] Generator { get; private set; }

		/// <summary>
		///   Binary data of the public value
		/// </summary>
		public byte[] PublicValue { get; private set; }

		internal DiffieHellmanKeyRecord() {}

		/// <summary>
		///   Creates a new instance of the DiffieHellmanKeyRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="recordClass"> Class of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="flags"> Flags of the key </param>
		/// <param name="protocol"> Protocol for which the key is used </param>
		/// <param name="prime"> Binary data of the prime of the key </param>
		/// <param name="generator"> Binary data of the generator of the key </param>
		/// <param name="publicValue"> Binary data of the public value </param>
		public DiffieHellmanKeyRecord(string name, RecordClass recordClass, int timeToLive, ushort flags, ProtocolType protocol, byte[] prime, byte[] generator, byte[] publicValue)
			: base(name, recordClass, timeToLive, flags, protocol, DnsSecAlgorithm.DiffieHellman)
		{
			Prime = prime ?? new byte[] { };
			Generator = generator ?? new byte[] { };
			PublicValue = publicValue ?? new byte[] { };
		}

		protected override void ParsePublicKey(byte[] resultData, int startPosition, int length)
		{
			int primeLength = DnsMessageBase.ParseUShort(resultData, ref startPosition);
			Prime = DnsMessageBase.ParseByteData(resultData, ref startPosition, primeLength);
			int generatorLength = DnsMessageBase.ParseUShort(resultData, ref startPosition);
			Generator = DnsMessageBase.ParseByteData(resultData, ref startPosition, generatorLength);
			int publicValueLength = DnsMessageBase.ParseUShort(resultData, ref startPosition);
			PublicValue = DnsMessageBase.ParseByteData(resultData, ref startPosition, publicValueLength);
		}

		protected override string PublicKeyToString()
		{
			byte[] publicKey = new byte[MaximumPublicKeyLength];
			int currentPosition = 0;

			EncodePublicKey(publicKey, 0, ref currentPosition, null);

            return BaseEncodingHelper.ToBase64String(publicKey);
		}

		protected override int MaximumPublicKeyLength
		{
			get { return 3 + Prime.Length + Generator.Length + PublicValue.Length; }
		}

		protected override void EncodePublicKey(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, (ushort) Prime.Length);
			DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, Prime);
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, (ushort) Generator.Length);
			DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, Generator);
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, (ushort) PublicValue.Length);
			DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, PublicValue);
		}
	}
}