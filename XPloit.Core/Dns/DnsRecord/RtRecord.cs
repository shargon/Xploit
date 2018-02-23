﻿using System;
using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>Route through record</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc1183">RFC 1183</see>
	///   </para>
	/// </summary>
	public class RtRecord : DnsRecordBase
	{
		/// <summary>
		///   Preference of the record
		/// </summary>
		public ushort Preference { get; private set; }

		/// <summary>
		///   Name of the intermediate host
		/// </summary>
		public string IntermediateHost { get; private set; }

		internal RtRecord() {}

		/// <summary>
		///   Creates a new instance of the RtRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="preference"> Preference of the record </param>
		/// <param name="intermediateHost"> Name of the intermediate host </param>
		public RtRecord(string name, int timeToLive, ushort preference, string intermediateHost)
			: base(name, RecordType.Rt, RecordClass.INet, timeToLive)
		{
			Preference = preference;
			IntermediateHost = intermediateHost ?? String.Empty;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			Preference = DnsMessageBase.ParseUShort(resultData, ref startPosition);
			IntermediateHost = DnsMessageBase.ParseDomainName(resultData, ref startPosition);
		}

		internal override string RecordDataToString()
		{
			return Preference
			       + " " + IntermediateHost;
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return IntermediateHost.Length + 4; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, Preference);
			DnsMessageBase.EncodeDomainName(messageData, offset, ref currentPosition, IntermediateHost, false, domainNames);
		}
	}
}