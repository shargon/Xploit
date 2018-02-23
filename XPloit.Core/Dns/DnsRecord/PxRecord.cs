﻿using System;
using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>X.400 mail mapping information record</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc2163">RFC 2163</see>
	///   </para>
	/// </summary>
	public class PxRecord : DnsRecordBase
	{
		/// <summary>
		///   Preference of the record
		/// </summary>
		public ushort Preference { get; private set; }

		/// <summary>
		///   Domain name containing the RFC822 domain
		/// </summary>
		public string Map822 { get; private set; }

		/// <summary>
		///   Domain name containing the X.400 part
		/// </summary>
		public string MapX400 { get; private set; }

		internal PxRecord() {}

		/// <summary>
		///   Creates a new instance of the PxRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="preference"> Preference of the record </param>
		/// <param name="map822"> Domain name containing the RFC822 domain </param>
		/// <param name="mapX400"> Domain name containing the X.400 part </param>
		public PxRecord(string name, int timeToLive, ushort preference, string map822, string mapX400)
			: base(name, RecordType.Px, RecordClass.INet, timeToLive)
		{
			Preference = preference;
			Map822 = map822 ?? String.Empty;
			MapX400 = mapX400 ?? String.Empty;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			Preference = DnsMessageBase.ParseUShort(resultData, ref startPosition);
			Map822 = DnsMessageBase.ParseDomainName(resultData, ref startPosition);
			MapX400 = DnsMessageBase.ParseDomainName(resultData, ref startPosition);
		}

		internal override string RecordDataToString()
		{
			return Preference
			       + " " + Map822
			       + " " + MapX400;
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 6 + Map822.Length + MapX400.Length; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, Preference);
			DnsMessageBase.EncodeDomainName(messageData, offset, ref currentPosition, Map822, false, domainNames);
			DnsMessageBase.EncodeDomainName(messageData, offset, ref currentPosition, MapX400, false, domainNames);
		}
	}
}