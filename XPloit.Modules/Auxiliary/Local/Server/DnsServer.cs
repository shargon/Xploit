using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Dns;
using XPloit.Core.Dns.DnsRecord;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;

namespace Auxiliary.Local.Server
{
    public class DnsServer : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "DNS-Server"; } }
        public override Reference[] References
        {
            get
            {
                return new Reference[]
                {
                    new Reference(EReferenceType.URL, "https://es.wikipedia.org/wiki/Domain_Name_System") ,
                };
            }
        }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Address for binding")]
        public IPAddress LocalAddress { get; set; }
        [ConfigurableProperty(Description = "Directory for creating TcpStream files")]
        public DirectoryInfo DumpFolder { get; set; }

        [ConfigurableProperty(Description = "Replicate answer from default DnsServers")]
        public bool ReplicateAnswer { get; set; }
        [ConfigurableProperty(Description = "Replicate answer from default DnsServers (when log)")]
        public bool LogReplicate { get; set; }
        [ConfigurableProperty(Description = "Query for Log (*.midomain.com) this query was respond with the client IP")]
        public string LogPattern { get; set; }
        #endregion

        public DnsServer()
        {
            LocalAddress = IPAddress.Any;
            ReplicateAnswer = true;
            LogPattern = "*";
        }

        [NonJobable()]
        public override bool Run()
        {
            if (!DumpFolder.Exists) return false;

            XPloit.Core.Dns.DnsServer server = new XPloit.Core.Dns.DnsServer(LocalAddress, 10, 10, ProcessQuery);
            server.Start();

            CreateJob(server);
            return true;
        }

        public override ECheck Check()
        {
            try
            {
                if (!SystemHelper.IsAvailableTcpPort(XPloit.Core.Dns.DnsServer.DNS_PORT))
                {
                    WriteError("Cant open port");
                    return ECheck.Error;
                }

                if (!DumpFolder.Exists)
                {
                    WriteError("DumpFolder must exists");
                    return ECheck.Error;
                }

                return ECheck.Ok;
            }
            catch { return ECheck.Error; }
        }

        DnsMessageBase ProcessQuery(DnsMessageBase message, IPAddress clientAddress, ProtocolType protocol)
        {
            message.IsQuery = false;
            DnsMessage query = (DnsMessage)message;

            if (query != null)
            {
                bool has_answer = false;
                foreach (DnsQuestion question in query.Questions)
                {
                    bool replicate = ReplicateAnswer;
                    if (StringHelper.Like(LogPattern, question.Name))
                    {
                        string path = DumpFolder.FullName + System.IO.Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                        string[] lines = new string[]
                        {
                            DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\t"+question.Name+"\t"+clientAddress.ToString()
                        };

                        lock (this) { File.AppendAllLines(path, lines); }

                        if (!LogReplicate) replicate = false;
                    }

                    if (replicate)
                    {
                        DnsMessage answer = DnsClient.Default.Resolve(question.Name, question.RecordType, question.RecordClass);
                        // if got an answer, copy it to the message sent to the client
                        if (answer != null)
                        {
                            has_answer = true;
                            foreach (DnsRecordBase record in (answer.AnswerRecords)) query.AnswerRecords.Add(record);
                            foreach (DnsRecordBase record in (answer.AdditionalRecords)) query.AnswerRecords.Add(record);
                        }
                    }
                    else
                    {
                        switch (question.RecordType)
                        {
                            case RecordType.A:
                                {
                                    if (clientAddress.AddressFamily == AddressFamily.InterNetwork)
                                    {
                                        query.ReturnCode = ReturnCode.NoError;
                                        query.AnswerRecords.Add(new ARecord(question.Name, 1000, clientAddress));
                                        return query;
                                    }
                                    break;
                                }
                            case RecordType.Aaaa:
                                {
                                    if (clientAddress.AddressFamily == AddressFamily.InterNetworkV6)
                                    {
                                        query.ReturnCode = ReturnCode.NoError;
                                        query.AnswerRecords.Add(new AaaaRecord(question.Name, 1000, clientAddress));
                                        return query;
                                    }
                                    break;
                                }
                        }
                    }
                }

                if (has_answer)
                {
                    query.ReturnCode = ReturnCode.NoError;
                    return query;
                }
            }

            // Not a valid query or upstream server did not answer correct
            message.ReturnCode = ReturnCode.ServerFailure;
            return message;
        }
    }
}
