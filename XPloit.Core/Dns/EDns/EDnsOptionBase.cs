namespace XPloit.Core.Dns.EDns
{
	/// <summary>
	///   Base class of EDNS options
	/// </summary>
	public abstract class EDnsOptionBase
	{
		/// <summary>
		///   Type of the option
		/// </summary>
		public EDnsOptionType Type { get; internal set; }

		internal EDnsOptionBase(EDnsOptionType optionType)
		{
			Type = optionType;
		}

		internal abstract void ParseData(byte[] resultData, int startPosition, int length);
		internal abstract ushort DataLength { get; }
		internal abstract void EncodeData(byte[] messageData, ref int currentPosition);
	}
}