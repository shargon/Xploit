using XPloit.Core.Dns.DnsRecord;

namespace XPloit.Core.Dns.DynamicUpdate
{
	/// <summary>
	///   Base update action of dynamic dns update
	/// </summary>
	public abstract class UpdateBase : DnsRecordBase
	{
		internal UpdateBase() {}

		protected UpdateBase(string name, RecordType recordType, RecordClass recordClass, int timeToLive)
			: base(name, recordType, recordClass, timeToLive) {}
	}
}