﻿using System.Net.NetworkInformation;

namespace XPloit.Core.Dns.EDns
{
	/// <summary>
	///   <para>EDNS0 Owner Option</para>
	///   <para>
	///     Defined in
	///     <see cref="!:http://files.dns-sd.org/draft-sekar-dns-llq.txt">draft-cheshire-edns0-owner-option</see>
	///   </para>
	/// </summary>
	public class OwnerOption : EDnsOptionBase
	{
		/// <summary>
		///   The version
		/// </summary>
		public byte Version { get; private set; }

		/// <summary>
		///   The sequence number
		/// </summary>
		public byte Sequence { get; private set; }

		/// <summary>
		///   The primary MAC address
		/// </summary>
		public PhysicalAddress PrimaryMacAddress { get; private set; }

		/// <summary>
		///   The Wakeup MAC address
		/// </summary>
		public PhysicalAddress WakeupMacAddress { get; private set; }

		/// <summary>
		///   The password, should be empty, 4 bytes long or 6 bytes long
		/// </summary>
		public byte[] Password { get; private set; }

		internal OwnerOption()
			: base(EDnsOptionType.Owner) {}

		/// <summary>
		///   Creates a new instance of the OwnerOption class
		/// </summary>
		/// <param name="sequence"> The sequence number </param>
		/// <param name="primaryMacAddress"> The primary MAC address </param>
		public OwnerOption(byte sequence, PhysicalAddress primaryMacAddress)
			: this(0, sequence, primaryMacAddress, null) {}

		/// <summary>
		///   Creates a new instance of the OwnerOption class
		/// </summary>
		/// <param name="version"> The version </param>
		/// <param name="sequence"> The sequence number </param>
		/// <param name="primaryMacAddress"> The primary MAC address </param>
		public OwnerOption(byte version, byte sequence, PhysicalAddress primaryMacAddress)
			: this(version, sequence, primaryMacAddress, null) {}

		/// <summary>
		///   Creates a new instance of the OwnerOption class
		/// </summary>
		/// <param name="sequence"> The sequence number </param>
		/// <param name="primaryMacAddress"> The primary MAC address </param>
		/// <param name="wakeupMacAddress"> The wakeup MAC address </param>
		public OwnerOption(byte sequence, PhysicalAddress primaryMacAddress, PhysicalAddress wakeupMacAddress)
			: this(0, sequence, primaryMacAddress, wakeupMacAddress) {}

		/// <summary>
		///   Creates a new instance of the OwnerOption class
		/// </summary>
		/// <param name="version"> The version </param>
		/// <param name="sequence"> The sequence number </param>
		/// <param name="primaryMacAddress"> The primary MAC address </param>
		/// <param name="wakeupMacAddress"> The wakeup MAC address </param>
		public OwnerOption(byte version, byte sequence, PhysicalAddress primaryMacAddress, PhysicalAddress wakeupMacAddress)
			: this(version, sequence, primaryMacAddress, wakeupMacAddress, null) {}

		/// <summary>
		///   Creates a new instance of the OwnerOption class
		/// </summary>
		/// <param name="sequence"> The sequence number </param>
		/// <param name="primaryMacAddress"> The primary MAC address </param>
		/// <param name="wakeupMacAddress"> The wakeup MAC address </param>
		/// <param name="password"> The password, should be empty, 4 bytes long or 6 bytes long </param>
		public OwnerOption(byte sequence, PhysicalAddress primaryMacAddress, PhysicalAddress wakeupMacAddress, byte[] password)
			: this(0, sequence, primaryMacAddress, wakeupMacAddress, password) {}

		/// <summary>
		///   Creates a new instance of the OwnerOption class
		/// </summary>
		/// <param name="version"> The version </param>
		/// <param name="sequence"> The sequence number </param>
		/// <param name="primaryMacAddress"> The primary MAC address </param>
		/// <param name="wakeupMacAddress"> The wakeup MAC address </param>
		/// <param name="password"> The password, should be empty, 4 bytes long or 6 bytes long </param>
		public OwnerOption(byte version, byte sequence, PhysicalAddress primaryMacAddress, PhysicalAddress wakeupMacAddress, byte[] password)
			: this()
		{
			Version = version;
			Sequence = sequence;
			PrimaryMacAddress = primaryMacAddress;
			WakeupMacAddress = wakeupMacAddress;
			Password = password;
		}

		internal override void ParseData(byte[] resultData, int startPosition, int length)
		{
			Version = resultData[startPosition++];
			Sequence = resultData[startPosition++];
			PrimaryMacAddress = new PhysicalAddress(DnsMessageBase.ParseByteData(resultData, ref startPosition, 6));
			if (length > 8)
				WakeupMacAddress = new PhysicalAddress(DnsMessageBase.ParseByteData(resultData, ref startPosition, 6));
			if (length > 14)
				Password = DnsMessageBase.ParseByteData(resultData, ref startPosition, length - 14);
		}

		internal override ushort DataLength
		{
			get { return (ushort) (8 + (WakeupMacAddress != null ? 6 : 0) + (Password != null ? Password.Length : 0)); }
		}

		internal override void EncodeData(byte[] messageData, ref int currentPosition)
		{
			messageData[currentPosition++] = Version;
			messageData[currentPosition++] = Sequence;
			DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, PrimaryMacAddress.GetAddressBytes());
			if (WakeupMacAddress != null)
				DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, WakeupMacAddress.GetAddressBytes());
			DnsMessageBase.EncodeByteArray(messageData, ref currentPosition, Password);
		}
	}
}