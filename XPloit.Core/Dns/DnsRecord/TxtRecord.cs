using System;
using System.Collections.Generic;
using System.Linq;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>Text strings</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc1035">RFC 1035</see>
	///   </para>
	/// </summary>
	public class TxtRecord : DnsRecordBase, ITextRecord
	{
		/// <summary>
		///   Text data
		/// </summary>
		public string TextData { get; protected set; }

		/// <summary>
		///   The single parts of the text data
		/// </summary>
		public IEnumerable<string> TextParts { get; protected set; }

		internal TxtRecord() {}

		/// <summary>
		///   Creates a new instance of the TxtRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="textData"> Text data </param>
		public TxtRecord(string name, int timeToLive, string textData)
			: base(name, RecordType.Txt, RecordClass.INet, timeToLive)
		{
			TextData = textData ?? String.Empty;
			TextParts = new List<string> { TextData };
		}

		/// <summary>
		///   Creates a new instance of the TxtRecord class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="textParts"> All parts of the text data </param>
		public TxtRecord(string name, int timeToLive, IEnumerable<string> textParts)
			: base(name, RecordType.Txt, RecordClass.INet, timeToLive)
		{
			TextParts = new List<string>(textParts);
			TextData = String.Join(String.Empty, TextParts.ToArray());
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			int endPosition = startPosition + length;

			List<string> textParts = new List<string>();
			while (startPosition < endPosition)
			{
				textParts.Add(DnsMessageBase.ParseText(resultData, ref startPosition));
			}

			TextParts = textParts;
			TextData = String.Join(String.Empty, textParts.ToArray());
		}

		internal override string RecordDataToString()
		{
			return " \"" + TextData + "\"";
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return TextData.Length + TextParts.Sum(p => (p.Length / 255) + (p.Length % 255 == 0 ? 0 : 1)); }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			foreach (var part in TextParts)
			{
				DnsMessageBase.EncodeTextBlock(messageData, ref currentPosition, part);
			}
		}
	}
}