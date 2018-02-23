using System;
using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>Server selector</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc2782">RFC 2782</see>
	///   </para>
	/// </summary>
	public class SrvRecord : DnsRecordBase
	{
		/// <summary>
		///   Priority of the record
		/// </summary>
		public ushort Priority { get; private set; }

		/// <summary>
		///   Relative weight for records with the same priority
		/// </summary>
		public ushort Weight { get; private set; }

		/// <summary>
		///   The port of the service on the target
		/// </summary>
		public ushort Port { get; private set; }

		/// <summary>
		///   Domain name of the target host
		/// </summary>
		public string Target { get; private set; }

		internal SrvRecord() {}

		/// <summary>
		///   Creates a new instance of the SrvRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="priority"> Priority of the record </param>
		/// <param name="weight"> Relative weight for records with the same priority </param>
		/// <param name="port"> The port of the service on the target </param>
		/// <param name="target"> Domain name of the target host </param>
		public SrvRecord(string name, int timeToLive, ushort priority, ushort weight, ushort port, string target)
			: base(name, RecordType.Srv, RecordClass.INet, timeToLive)
		{
			Priority = priority;
			Weight = weight;
			Port = port;
			Target = target ?? String.Empty;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			Priority = DnsMessageBase.ParseUShort(resultData, ref startPosition);
			Weight = DnsMessageBase.ParseUShort(resultData, ref startPosition);
			Port = DnsMessageBase.ParseUShort(resultData, ref startPosition);
			Target = DnsMessageBase.ParseDomainName(resultData, ref startPosition);
		}

		internal override string RecordDataToString()
		{
			return Priority
			       + " " + Weight
			       + " " + Port
			       + " " + Target;
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return Target.Length + 8; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, Priority);
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, Weight);
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, Port);
			DnsMessageBase.EncodeDomainName(messageData, offset, ref currentPosition, Target, false, domainNames);
		}
	}
}