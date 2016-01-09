using System.Collections.Generic;

namespace XPloit.Core.Dns.DnsRecord
{
	/// <summary>
	///   <para>NID</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://tools.ietf.org/html/rfc6742">RFC 6742</see>
	///   </para>
	/// </summary>
	public class NIdRecord : DnsRecordBase
	{
		/// <summary>
		///   The preference
		/// </summary>
		public ushort Preference { get; private set; }

		/// <summary>
		///   The Node ID
		/// </summary>
		public ulong NodeID { get; private set; }

		internal NIdRecord() {}

		/// <summary>
		///   Creates a new instance of the NIdRecord class
		/// </summary>
		/// <param name="name"> Domain name of the host </param>
		/// <param name="timeToLive"> Seconds the record should be cached at most </param>
		/// <param name="preference"> The preference </param>
		/// <param name="nodeID"> The Node ID </param>
		public NIdRecord(string name, int timeToLive, ushort preference, ulong nodeID)
			: base(name, RecordType.NId, RecordClass.INet, timeToLive)
		{
			Preference = preference;
			NodeID = nodeID;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length)
		{
			Preference = DnsMessageBase.ParseUShort(resultData, ref startPosition);
			NodeID = DnsMessageBase.ParseULong(resultData, ref startPosition);
		}

		internal override string RecordDataToString()
		{
			string nodeID = NodeID.ToString("x16");
			return Preference + " " + nodeID.Substring(0, 4) + ":" + nodeID.Substring(4, 4) + ":" + nodeID.Substring(8, 4) + ":" + nodeID.Substring(12);
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return 10; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			DnsMessageBase.EncodeUShort(messageData, ref currentPosition, Preference);
			DnsMessageBase.EncodeULong(messageData, ref currentPosition, NodeID);
		}
	}
}