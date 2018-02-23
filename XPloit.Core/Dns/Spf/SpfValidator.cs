using System.Linq;
using XPloit.Core.Dns.DnsRecord;

namespace XPloit.Core.Dns.Spf
{
	/// <summary>
	///   Validator for SPF records
	/// </summary>
	public class SpfValidator : ValidatorBase<SpfRecord>
	{
		protected override bool TryLoadRecords(string domain, out SpfRecord record, out SpfQualifier errorResult)
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

		private bool TryLoadRecords(string domain, RecordType recordType, out SpfRecord record, out SpfQualifier errorResult)
		{
			DnsMessage dnsMessage = ResolveDns(domain, recordType);
			if ((dnsMessage == null) || ((dnsMessage.ReturnCode != ReturnCode.NoError) && (dnsMessage.ReturnCode != ReturnCode.NxDomain)))
			{
				record = default(SpfRecord);
				errorResult = SpfQualifier.TempError;
				return false;
			}

			var spfTextRecords =
				dnsMessage.AnswerRecords
				          .Where(r => r.RecordType == recordType)
				          .Cast<ITextRecord>()
				          .Select(r => r.TextData)
				          .Where(SpfRecord.IsSpfRecord).ToList();

			if (spfTextRecords.Count == 0)
			{
				record = default(SpfRecord);
				errorResult = SpfQualifier.None;
				return false;
			}
			else if ((spfTextRecords.Count > 1) || !SpfRecord.TryParse(spfTextRecords[0], out record))
			{
				record = default(SpfRecord);
				errorResult = SpfQualifier.PermError;
				return false;
			}
			else
			{
				errorResult = default(SpfQualifier);
				return true;
			}
		}
	}
}