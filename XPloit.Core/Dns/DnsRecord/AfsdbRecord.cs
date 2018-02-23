using System;
using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>AFS data base location</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc1183">RFC 1183</see>
	///     and
	///     <see cref="!:http://tools.ietf.org/html/rfc5864">RFC 5864</see>
	///   </para>
	/// </summary>
	public class AfsdbRecord : DnsRecordBase
	{
		/// <summary>
		///   AFS database subtype
		/// </summary>
		public enum AfsSubType : ushort
		{
			/// <summary>
			///   <para>Andrews File Service v3.0 Location service</para>
			///   <para>
			///     Defined in
			///     <see cref="!:http://tools.ietf.org/html/rfc1183">RFC 1183</see>
			///   </para>
			/// </summary>
			Afs = 1,

			/// <summary>
			///   <para>DCE/NCA root cell directory node</para>
			///   <para>
			///     Defined in
			///     <see cref="!:http://tools.ietf.org/html/rfc1183">RFC 1183</see>
			///   </para>
			/// </summary>
			Dce = 2,
		}

		/// <summary>
		///   Subtype of the record
		/// </summary>
		public AfsSubType SubType { get; private set; }

		/// <summary>
		///   Hostname of the AFS database
		/// </summary>
		public string Hostname { get; private set; }

		internal AfsdbRecord() {}

		/// <summary>
		///   Creates a new instance of the AfsdbRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="subType"> Subtype of the record </param>
		/// <param name="hostname"> Hostname of the AFS database </param>
		public AfsdbRecord(string name, int timeToLive, AfsSubType subType, string hostname)
			: base(name, RecordType.Afsdb, RecordClass.INet, timeToLive)
		{
			SubType = subType;
			Hostname = hostname ?? String.Empty;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			SubType = (AfsSubType) DnsMessageBase.ParseUShort(resultData, ref startPosition);
			Hostname = DnsMessageBase.ParseDomainName(resultData, ref startPosition);
		}

		internal override string RecordDataToString()
		{
			return (byte) SubType
			       + " " + Hostname;
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return Hostname.Length + 4; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, (ushort) SubType);
			DnsMessageBase.EncodeDomainName(messageData, offset, ref currentPosition, Hostname, false, domainNames);
		}
	}
}