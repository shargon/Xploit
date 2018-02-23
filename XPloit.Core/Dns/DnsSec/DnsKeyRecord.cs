﻿using System.Collections.Generic;
using XPloit.Core.Dns.DnsRecord;
using XPloit.Helpers;

namespace XPloit.Core.Dns.DnsSec
{
	/// <summary>
	///   <para>DNS Key record</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc4034">RFC 4034</see>
	///     and
	///     <see cref="!:http://tools.ietf.org/html/rfc3755">RFC 3755</see>
	///   </para>
	/// </summary>
	public class DnsKeyRecord : DnsRecordBase
	{
		/// <summary>
		///   Flags of the key
		/// </summary>
		public ushort Flags { get; private set; }

		/// <summary>
		///   Protocol field
		/// </summary>
		public byte Protocol { get; private set; }

		/// <summary>
		///   Algorithm of the key
		/// </summary>
		public DnsSecAlgorithm Algorithm { get; private set; }

		/// <summary>
		///   Binary data of the public key
		/// </summary>
		public byte[] PublicKey { get; private set; }

		/// <summary>
		///   Record holds a DNS zone key
		/// </summary>
		public bool IsZoneKey
		{
			get { return (Flags & 0x100) != 0; }
			set
			{
				if (value)
				{
					Flags |= 0x100;
				}
				else
				{
					Flags &= 0xfeff;
				}
			}
		}

		/// <summary>
		///   <para>Key is intended for use as a secure entry point</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc4034">RFC 4034</see>
		///     and
		///     <see cref="!:http://tools.ietf.org/html/rfc3757">RFC 3757</see>
		///   </para>
		/// </summary>
		public bool IsSecureEntryPoint
		{
			get { return (Flags & 0x01) != 0; }
			set
			{
				if (value)
				{
					Flags |= 0x01;
				}
				else
				{
					Flags &= 0xfffe;
				}
			}
		}

		internal DnsKeyRecord() {}

		/// <summary>
		///   Creates a new instance of the DnsKeyRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="recordClass"> Class of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="flags"> Flags of the key </param>
		/// <param name="protocol"> Protocol field </param>
		/// <param name="algorithm"> Algorithm of the key </param>
		/// <param name="publicKey"> Binary data of the public key </param>
		public DnsKeyRecord(string name, RecordClass recordClass, int timeToLive, ushort flags, byte protocol, DnsSecAlgorithm algorithm, byte[] publicKey)
			: base(name, RecordType.DnsKey, recordClass, timeToLive)
		{
			Flags = flags;
			Protocol = protocol;
			Algorithm = algorithm;
			PublicKey = publicKey ?? new byte[] { };
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			Flags = DnsMessageBase.ParseUShort(resultData, ref startPosition);
			Protocol = resultData[startPosition++];
			Algorithm = (DnsSecAlgorithm) resultData[startPosition++];
			PublicKey = DnsMessageBase.ParseByteData(resultData, ref startPosition, length - 4);
		}

		internal override string RecordDataToString()
		{
			return Flags
			       + " " + Protocol
			       + " " + (byte) Algorithm
                   + " " + BaseEncodingHelper.ToBase64String(PublicKey);
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 4 + PublicKey.Length; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, Flags);
			messageData[currentPosition++] = Protocol;
			messageData[currentPosition++] = (byte) Algorithm;
			DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, PublicKey);
		}
	}
}