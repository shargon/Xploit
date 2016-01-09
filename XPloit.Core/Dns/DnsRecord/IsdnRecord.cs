using System;
using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>ISDN address</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc1183">RFC 1183</see>
	///   </para>
	/// </summary>
	public class IsdnRecord : DnsRecordBase
	{
		/// <summary>
		///   ISDN number
		/// </summary>
		public string IsdnAddress { get; private set; }

		/// <summary>
		///   Sub address
		/// </summary>
		public string SubAddress { get; private set; }

		internal IsdnRecord() {}

		/// <summary>
		///   Creates a new instance of the IsdnRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="isdnAddress"> ISDN number </param>
		public IsdnRecord(string name, int timeToLive, string isdnAddress)
			: this(name, timeToLive, isdnAddress, String.Empty) {}

		/// <summary>
		///   Creates a new instance of the IsdnRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="isdnAddress"> ISDN number </param>
		/// <param name="subAddress"> Sub address </param>
		public IsdnRecord(string name, int timeToLive, string isdnAddress, string subAddress)
			: base(name, RecordType.Isdn, RecordClass.INet, timeToLive)
		{
			IsdnAddress = isdnAddress ?? String.Empty;
			SubAddress = subAddress ?? String.Empty;
		}

		internal override void ParseRecordData(byte[] resultData, int currentPosition, int length)
		{
			int endPosition = currentPosition + length;

			IsdnAddress = DnsMessageBase.ParseText(resultData, ref currentPosition);
			SubAddress = (currentPosition < endPosition) ? DnsMessageBase.ParseText(resultData, ref currentPosition) : String.Empty;
		}

		internal override string RecordDataToString()
		{
			return IsdnAddress
			       + (String.IsNullOrEmpty(SubAddress) ? String.Empty : " " + SubAddress);
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 2 + IsdnAddress.Length + SubAddress.Length; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeTextBlock(messageData, ref currentPosition, IsdnAddress);
			DnsMessageBase.EncodeTextBlock(messageData, ref currentPosition, SubAddress);
		}
	}
}