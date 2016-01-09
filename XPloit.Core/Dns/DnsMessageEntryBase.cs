namespace XPloit.Core.Dns
{
	/// <summary>
	///   Base class for a dns name identity
	/// </summary>
	public abstract class DnsMessageEntryBase
	{
		/// <summary>
		///   Domain name
		/// </summary>
		public string Name { get; internal set; }

		/// <summary>
		///   Type of the record
		/// </summary>
		public RecordType RecordType { get; internal set; }

		/// <summary>
		///   Class of the record
		/// </summary>
		public RecordClass RecordClass { get; internal set; }

		internal abstract int MaximumLength { get; }

		/// <summary>
		///   Returns the textual representation
		/// </summary>
		/// <returns> Textual representation </returns>
		public override string ToString()
		{
			return Name + " " + RecordType + " " + RecordClass;
		}
	}
}