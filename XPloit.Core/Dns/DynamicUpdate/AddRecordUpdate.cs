using System.Collections.Generic;
using XPloit.Core.Dns.DnsRecord;

namespace XPloit.Core.Dns.DynamicUpdate
{
    /// <summary>
    ///   Add record action
    /// </summary>
    public class AddRecordUpdate : UpdateBase
    {
        /// <summary>
        ///   Record which should be added
        /// </summary>
        public DnsRecordBase Record { get; private set; }

        internal AddRecordUpdate() { }

        /// <summary>
        ///   Creates a new instance of the AddRecordUpdate
        /// </summary>
        /// <param name="record"> Record which should be added </param>
        public AddRecordUpdate(DnsRecordBase record)
            : base(record.Name, record.RecordType, record.RecordClass, record.TimeToLive)
        {
            Record = record;
        }

        internal override void ParseRecordData(byte[] resultData, int startPosition, int length) { }

        internal override string RecordDataToString()
        {
            return (Record == null) ? null : Record.RecordDataToString();
        }

        protected internal override int MaximumRecordDataLength
        {
            get { return Record.MaximumRecordDataLength; }
        }

        protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames)
        {
            Record.EncodeRecordData(messageData, offset, ref currentPosition, domainNames);
        }
    }
}