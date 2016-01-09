using XPloit.Core.Dns.DnsRecord;

namespace XPloit.Core.Dns.DynamicUpdate
{
	/// <summary>
	///   Base class for prequisites of dynamic dns updates
	/// </summary>
	public abstract class PrequisiteBase : DnsRecordBase
	{
		internal PrequisiteBase() {}

		protected PrequisiteBase(string name, RecordType recordType, RecordClass recordClass, int timeToLive)
			: base(name, recordType, recordClass, timeToLive) {}

		internal override string RecordDataToString()
		{
			return null;
		}
	}
}