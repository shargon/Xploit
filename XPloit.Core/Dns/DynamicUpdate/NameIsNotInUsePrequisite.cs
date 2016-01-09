using System.Collections.Generic;

namespace XPloit.Core.Dns.DynamicUpdate
{
	/// <summary>
	///   Prequisite, that a name does not exist
	/// </summary>
	public class NameIsNotInUsePrequisite : PrequisiteBase
	{
		internal NameIsNotInUsePrequisite() {}

		/// <summary>
		///   Creates a new instance of the NameIsNotInUsePrequisite class
		/// </summary>
		/// <param name="name"> Name that should be checked </param>
		public NameIsNotInUsePrequisite(string name)
			: base(name, RecordType.Any, RecordClass.None, 0) {}

		internal override void ParseRecordData(byte[] resultData, int startPosition, int length) {}

		protected internal override int MaximumRecordDataLength
		{
			get { return 0; }
		}

		protected internal override void EncodeRecordData(byte[] messageData, int offset, ref int currentPosition, Dictionary<string, ushort> domainNames) {}
	}
}