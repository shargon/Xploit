using System.Collections.Generic;
using XPloit.Core.Dns.DnsRecord;

namespace XPloit.Core.Dns.DynamicUpdate
{
	/// <summary>
	///   Delete record action
	/// </summary>
	public class DeleteRecordUpdate : UpdateBase
	{
		/// <summary>
		///   Record that should be deleted
		/// </summary>
		public DnsRecordBase Record { get; private set; }

		internal DeleteRecordUpdate() {}

		/// <summary>
		///   Creates a new instance of the DeleteRecordUpdate class
		/// </summary>
		/// <param name="name"> Name of the record that should be deleted </param>
		/// <param name="recordType"> Type of the record that should be deleted </param>
		public DeleteRecordUpdate(string name, RecordType recordType)
			: base(name, recordType, RecordClass.Any, 0) {}

		/// <summary>
		///   Creates a new instance of the DeleteRecordUpdate class
		/// </summary>
		/// <param name="record"> Record that should be deleted </param>
		public DeleteRecordUpdate(DnsRecordBase record)
			: base(record.Name, record.RecordType, RecordClass.None, 0)
		{
			Record = record;
		}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length) {}

		internal override string RecordDataToString()
		{
			return (Record == null) ? null : Record.RecordDataToString();
		}

		protected internal override int MaximumRecordDataLength
		{
			get { return (Record == null) ? 0 : Record.MaximumRecordDataLength; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
		{
			if (Record != null)
				Record.EncodeRecordData(messageData, offset, ref currentPosition, domainNames);
		}
	}
}