using System;
using System.Collections.Generic;

namespace XPloit.Core.Dns
{
	/// <summary>
	///   A single entry of the Question section of a dns query
	/// </summary>
	public class DnsQuestion : DnsMessageEntryBase
	{
		/// <summary>
		///   Creates a new instance of the DnsQuestion class
		/// </summary>
		/// <param name="name"> Domain name </param>
		/// <param name="recordType"> Record type </param>
		/// <param name="recordClass"> Record class </param>
		public DnsQuestion(string name, RecordType recordType, RecordClass recordClass)
		{
			Name = name ?? String.Empty;
			RecordType = recordType;
			RecordClass = recordClass;
		}

		internal DnsQuestion() {}

		internal override int MaximumLength
		{
			get { return Name.Length + 6; }
		}

		internal void Encode(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeDomainName(messageData, offset, ref currentPosition, Name, true, domainNames);
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, (ushort) RecordType);
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, (ushort) RecordClass);
		}
	}
}