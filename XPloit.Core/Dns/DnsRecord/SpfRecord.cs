using System;
using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>Sender Policy Framework</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc4408">RFC 4408</see>
	///   </para>
	/// </summary>
	public class SpfRecord : DnsRecordBase, ITextRecord
	{
		/// <summary>
		///   Text data of the record
		/// </summary>
		public string TextData { get; protected set; }

		internal SpfRecord() {}

		/// <summary>
		///   Creates a new instance of the SpfRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="textData"> Text data of the record </param>
		public SpfRecord(string name, int timeToLive, string textData)
			: base(name, RecordType.Spf, RecordClass.INet, timeToLive)
		{
			TextData = textData ?? String.Empty;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			int endPosition = startPosition + length;

			TextData = String.Empty;
			while (startPosition < endPosition)
			{
				TextData += DnsMessageBase.ParseText(resultData, ref startPosition);
			}
		}

		internal override string RecordDataToString()
		{
			return " \"" + TextData + "\"";
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return TextData.Length + (TextData.Length / 255) + (TextData.Length % 255 == 0 ? 0 : 1); }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeTextBlock(messageData, ref currentPosition, TextData);
		}
	}
}