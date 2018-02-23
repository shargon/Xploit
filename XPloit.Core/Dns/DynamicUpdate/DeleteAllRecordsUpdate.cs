namespace XPloit.Core.Dns.DynamicUpdate
{
	/// <summary>
	///   Delete all records action
	/// </summary>
	public class DeleteAllRecordsUpdate : DeleteRecordUpdate
	{
		internal DeleteAllRecordsUpdate() {}

		/// <summary>
		///   Creates a new instance of the DeleteAllRecordsUpdate class
		/// </summary>
		/// <param name="name"> Name of records, that should be deleted </param>
		public DeleteAllRecordsUpdate(string name)
			: base(name, RecordType.Any) {}
	}
}