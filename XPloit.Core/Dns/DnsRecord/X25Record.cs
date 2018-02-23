using System;
using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>X.25 PSDN address record</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc1183">RFC 1183</see>
	///   </para>
	/// </summary>
	public class X25Record : DnsRecordBase
	{
		/// <summary>
		///   PSDN (Public Switched Data Network) address
		/// </summary>
		public string X25Address { get; protected set; }

		internal X25Record() {}

		/// <summary>
		///   Creates a new instance of the X25Record class
		/// </summary>
		/// <param name="name"> Name of the record </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="x25Address"> PSDN (Public Switched Data Network) address </param>
		public X25Record(string name, int timeToLive, string x25Address)
			: base(name, RecordType.X25, RecordClass.INet, timeToLive)
		{
			X25Address = x25Address ?? String.Empty;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			X25Address += DnsMessageBase.ParseText(resultData, ref startPosition);
		}

		internal override string RecordDataToString()
		{
			return X25Address;
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 1 + X25Address.Length; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeTextBlock(messageData, ref currentPosition, X25Address);
		}
	}
}