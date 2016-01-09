using System;
using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>DNS Name Redirection record</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc6672">RFC 6672</see>
	///   </para>
	/// </summary>
	public class DNameRecord : DnsRecordBase
	{
		/// <summary>
		///   Target of the redirection
		/// </summary>
		public string Target { get; private set; }

		internal DNameRecord() {}

		/// <summary>
		///   Creates a new instance of the DNameRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="target"> Target of the redirection </param>
		public DNameRecord(string name, int timeToLive, string target)
			: base(name, RecordType.DName, RecordClass.INet, timeToLive)
		{
			Target = target ?? String.Empty;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			Target = DnsMessageBase.ParseDomainName(resultData, ref startPosition);
		}

		internal override string RecordDataToString()
		{
			return Target;
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return Target.Length + 2; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeDomainName(messageData, offset, ref currentPosition, Target, false, domainNames);
		}
	}
}