using System.Collections.Generic;

namespace XPloit.Core.Dns.DynamicUpdate
{
	/// <summary>
	///   Prequisite, that a name exists
	/// </summary>
	public class NameIsInUsePrequisite : PrequisiteBase
	{
		internal NameIsInUsePrequisite() {}

		/// <summary>
		///   Creates a new instance of the NameIsInUsePrequisite class
		/// </summary>
		/// <param name="name"> Name that should be checked </param>
		public NameIsInUsePrequisite(string name)
			: base(name, RecordType.Any, RecordClass.Any, 0) {}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length) {}

		protected internal override int MaximumRecordDataLength
		{
			get { return 0; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames) {}
	}
}