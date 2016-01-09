using System.Collections.Generic;
using XPloit.Core.Dns.DnsRecord;

namespace XPloit.Core.Dns
{
	/// <summary>
	///   Message returned as result to a LLMNR query
	/// </summary>
	public class LlmnrMessage : DnsMessageBase
	{
		/// <summary>
		///   Parses a the contents of a byte array as LlmnrMessage
		/// </summary>
		/// <param name="data">Buffer, that contains the message data</param>
		/// <returns>A new instance of the LlmnrMessage class</returns>
		public static LlmnrMessage Parse(byte[] data)
		{
			return Parse<LlmnrMessage>(data);
		}

		#region Header
		/// <summary>
		///   <para>Gets or sets the conflict (C) flag</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc4795">RFC 4795</see>
		///   </para>
		/// </summary>
		public bool IsConflict
		{
			get { return (Flags & 0x0400) != 0; }
			set
			{
				if (value)
				{
					Flags |= 0x0400;
				}
				else
				{
					Flags &= 0xfbff;
				}
			}
		}

		/// <summary>
		///   <para>Gets or sets the truncated response (TC) flag</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc4795">RFC 4795</see>
		///   </para>
		/// </summary>
		public bool IsTruncated
		{
			get { return (Flags & 0x0200) != 0; }
			set
			{
				if (value)
				{
					Flags |= 0x0200;
				}
				else
				{
					Flags &= 0xfdff;
				}
			}
		}

		/// <summary>
		///   <para>Gets or sets the tentive (T) flag</para>
		///   <para>
		///     Defined in
		///     <see cref="!:http://tools.ietf.org/html/rfc4795">RFC 4795</see>
		///   </para>
		/// </summary>
		public bool IsTentive
		{
			get { return (Flags & 0x0100) != 0; }
			set
			{
				if (value)
				{
					Flags |= 0x0100;
				}
				else
				{
					Flags &= 0xfeff;
				}
			}
		}
		#endregion

		/// <summary>
		///   Gets or sets the entries in the question section
		/// </summary>
		public new List<DnsQuestion> Questions
		{
			get { return base.Questions; }
			set { base.Questions = (value ?? new List<DnsQuestion>()); }
		}

		/// <summary>
		///   Gets or sets the entries in the answer records section
		/// </summary>
		public new List<DnsRecordBase> AnswerRecords
		{
			get { return base.AnswerRecords; }
			set { base.AnswerRecords = (value ?? new List<DnsRecordBase>()); }
		}

		/// <summary>
		///   Gets or sets the entries in the authority records section
		/// </summary>
		public new List<DnsRecordBase> AuthorityRecords
		{
			get { return base.AuthorityRecords; }
			set { base.AuthorityRecords = (value ?? new List<DnsRecordBase>()); }
		}

		internal override bool IsTcpUsingRequested
		{
			get { return (Questions.Count > 0) && ((Questions[0].RecordType == RecordType.Axfr) || (Questions[0].RecordType == RecordType.Ixfr)); }
		}

		internal override bool IsTcpResendingRequested
		{
			get { return IsTruncated; }
		}

		internal override bool IsTcpNextMessageWaiting(bool isSubsequentResponseMessage)
		{
			return false;
		}
	}
}