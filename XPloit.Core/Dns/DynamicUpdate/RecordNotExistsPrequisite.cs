using System.Collections.Generic;

namespace XPloit.Core.Dns.DynamicUpdate
{
	/// <summary>
	///   Prequisite, that a record does not exist
	/// </summary>
	public class RecordNotExistsPrequisite : PrequisiteBase
	{
		internal RecordNotExistsPrequisite() {}

		/// <summary>
		///   Creates a new instance of the RecordNotExistsPrequisite class
		/// </summary>
		/// <param name="name"> Name of record that should be checked </param>
		/// <param name="recordType"> Type of record that should be checked </param>
		public RecordNotExistsPrequisite(string name, RecordType recordType)
			: base(name, recordType, RecordClass.None, 0) {}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length) {}

		protected internal override int MaximumRecordDataLength
		{
			get { return 0; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames) {}
	}
}