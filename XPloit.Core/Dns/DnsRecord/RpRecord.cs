using System;
using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>Responsible person record</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc1183">RFC 1183</see>
	///   </para>
	/// </summary>
	public class RpRecord : DnsRecordBase
	{
		/// <summary>
		///   Mail address of responsable person, the @ should be replaced by a dot
		/// </summary>
		public string MailBox { get; protected set; }

		/// <summary>
		///   Domain name of a <see cref="TxtRecord" /> with additional information
		/// </summary>
		public string TxtDomainName { get; protected set; }

		internal RpRecord() {}

		/// <summary>
		///   Creates a new instance of the RpRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="mailBox"> Mail address of responsable person, the @ should be replaced by a dot </param>
		/// <param name="txtDomainName">
		///   Domain name of a <see cref="TxtRecord" /> with additional information
		/// </param>
		public RpRecord(string name, int timeToLive, string mailBox, string txtDomainName)
			: base(name, RecordType.Rp, RecordClass.INet, timeToLive)
		{
			MailBox = mailBox ?? String.Empty;
			TxtDomainName = txtDomainName ?? String.Empty;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			MailBox = DnsMessageBase.ParseDomainName(resultData, ref startPosition);
			TxtDomainName = DnsMessageBase.ParseDomainName(resultData, ref startPosition);
		}

		internal override string RecordDataToString()
		{
			return MailBox
			       + " " + TxtDomainName;
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 4 + MailBox.Length + TxtDomainName.Length; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeDomainName(messageData, offset, ref currentPosition, MailBox, false, domainNames);
			DnsMessageBase.EncodeDomainName(messageData, offset, ref currentPosition, TxtDomainName, false, domainNames);
		}
	}
}