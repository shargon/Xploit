using System;

namespace XPloit.Core.Dns.EDns
{
	/// <summary>
	///   <para>Update lease option</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://files.dns-sd.org/draft-sekar-dns-ul.txt">draft-sekar-dns-ul</see>
	///   </para>
	/// </summary>
	public class UpdateLeaseOption : EDnsOptionBase
	{
		/// <summary>
		///   Desired lease (request) or granted lease (response)
		/// </summary>
		public TimeSpan LeaseTime { get; private set; }

		internal UpdateLeaseOption()
			: base(EDnsOptionType.UpdateLease) {}

		/// <summary>
		///   Creates a new instance of the UpdateLeaseOption class
		/// </summary>
		public UpdateLeaseOption(TimeSpan leaseTime)
			: this()
		{
			LeaseTime = leaseTime;
		}

		internal override void ParseData(byte[] resultData, int startPosition, int length)
		{
			LeaseTime = TimeSpan.FromSeconds(DnsMessageBase.ParseInt(resultData, ref startPosition));
		}

		internal override ushort DataLength
		{
			get { return 4; }
		}

		internal override void EncodeData(byte[] messageData, ref int currentPosition)
		{
			DnsMessageBase.EncodeInt(messageData, ref currentPosition, (int) LeaseTime.TotalSeconds);
		}
	}
}