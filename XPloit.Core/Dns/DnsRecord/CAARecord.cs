using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>Mail exchange</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc6844">RFC 6844</see>
	///   </para>
	/// </summary>
	public class CAARecord : DnsRecordBase
	{
		/// <summary>
		///   The flags
		/// </summary>
		public byte Flags { get; private set; }

		/// <summary>
		///   The name of the tag
		/// </summary>
		public string Tag { get; private set; }

		/// <summary>
		///   The value of the tag
		/// </summary>
		public string Value { get; private set; }

		internal CAARecord() {}

		/// <summary>
		///   Creates a new instance of the CAARecord class
		/// </summary>
		/// <param name="name"> Name of the zone </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="flags">The flags</param>
		/// <param name="tag">The name of the tag</param>
		/// <param name="value">The value of the tag</param>
		public CAARecord(string name, int timeToLive, byte flags, string tag, string value)
			: base(name, RecordType.CAA, RecordClass.INet, timeToLive)
		{
			Flags = flags;
			Tag = tag;
			Value = value;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			Flags = resultData[startPosition++];
			Tag = DnsMessageBase.ParseText(resultData, ref startPosition);
			Value = DnsMessageBase.ParseText(resultData, ref startPosition, length - (2 + Tag.Length));
		}

		internal override string RecordDataToString()
		{
			return Flags + " " + Tag + " " + Value;
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 2 + Tag.Length + Value.Length; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			messageData[currentPosition++] = Flags;
			DnsMessageBase.EncodeTextBlock(messageData, ref currentPosition, Tag);
			DnsMessageBase.EncodeTextWithoutLength(messageData, ref currentPosition, Value);
		}
	}
}