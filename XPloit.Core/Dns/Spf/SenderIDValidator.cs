using System.Collections.Generic;
using System.Linq;
using XPloit.Core.Dns.DnsRecord;

namespace XPloit.Core.Dns.Spf
{
	/// <summary>
	///   Validator for SenderID records
	/// </summary>
	public class SenderIDValidator : ValidatorBase<SenderIDRecord>
	{
		/// <summary>
		///   Scope to examin
		/// </summary>
		public SenderIDScope Scope { get; set; }

		/// <summary>
		///   Initializes a new instance of the SenderIDValidator class.
		/// </summary>
		public SenderIDValidator()
		{
			Scope = SenderIDScope.MFrom;
		}

		protected override bool TryLoadRecords(string domain, out SenderIDRecord record, out SpfQualifier errorResult)
		{
			if (!TryLoadRecords(domain, RecordType.Spf, out record, out errorResult))
			{
				return (errorResult == SpfQualifier.None) && TryLoadRecords(domain, RecordType.Txt, out record, out errorResult);
			}
			else
			{
				return true;
			}
		}

		private bool TryLoadRecords(string domain, RecordType recordType, out SenderIDRecord record, out SpfQualifier errorResult)
		{
			DnsMessage dnsMessage = ResolveDns(domain, recordType);
			if ((dnsMessage == null) || ((dnsMessage.ReturnCode != ReturnCode.NoError) && (dnsMessage.ReturnCode != ReturnCode.NxDomain)))
			{
				record = default(SenderIDRecord);
				errorResult = SpfQualifier.TempError;
				return false;
			}
			else if ((Scope == SenderIDScope.Pra) && (dnsMessage.ReturnCode == ReturnCode.NxDomain))
			{
				record = default(SenderIDRecord);
				errorResult = SpfQualifier.Fail;
				return false;
			}

			var senderIDTextRecords =
				dnsMessage.AnswerRecords
				          .Where(r => r.RecordType == recordType)
				          .Cast<ITextRecord>()
				          .Select(r => r.TextData)
				          .Where(t => SenderIDRecord.IsSenderIDRecord(t, Scope)).ToList();

			if (senderIDTextRecords.Count >= 1)
			{
				var potentialRecords = new List<SenderIDRecord>();
				foreach (var senderIDTextRecord in senderIDTextRecords)
				{
					SenderIDRecord tmpRecord;
					if (SenderIDRecord.TryParse(senderIDTextRecord, out tmpRecord))
					{
						potentialRecords.Add(tmpRecord);
					}
					else
					{
						record = default(SenderIDRecord);
						errorResult = SpfQualifier.PermError;
						return false;
					}
				}

				if (potentialRecords.GroupBy(r => r.Version).Any(g => g.Count() > 1))
				{
					record = default(SenderIDRecord);
					errorResult = SpfQualifier.PermError;
					return false;
				}
				else
				{
					record = potentialRecords.OrderByDescending(r => r.Version).First();
					errorResult = default(SpfQualifier);
					return true;
				}
			}
			else
			{
				record = default(SenderIDRecord);
				errorResult = SpfQualifier.None;
				return false;
			}
		}
	}
}