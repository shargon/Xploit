using MongoDB.Driver;
using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Mongo;
using XPloit.Helpers.Attributes;
using XPloit.Helpers.Geolocate;
using XPloit.Sniffer.Extractors;
using static XPloit.Sniffer.Extractors.ExtractFtpPop3;
using static XPloit.Sniffer.Extractors.ExtractHttp;
using static XPloit.Sniffer.Extractors.ExtractTelnet;

namespace Auxiliary.Multi.ETL
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "SnifferMongo to Mysql")]
    public class SnifferMongoToMysql : Module
    {
        [ConfigurableProperty(Description = "Mongo repository url", Optional = false)]
        public MongoUrl Repository { get; set; }

        [ConfigurableProperty(Description = "Mysql connection string", Optional = false)]
        public string ConnectionString { get; set; }

        public override ECheck Check()
        {
            bool ok1 = false, ok2 = false, ok3 = false, ok4 = false;

            using (MySqlConnection con = new MySqlConnection(ConnectionString))
            {
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS `hosts` (
  `ID` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `IP` varchar(40) NOT NULL DEFAULT '',
  `CONTINENT` char(2) NOT NULL DEFAULT '',
  `COUNTRY` char(2) NOT NULL DEFAULT '',
  PRIMARY KEY (`ID`),
  UNIQUE KEY `IP_UNIQUE` (`IP`)
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;
";

                    cmd.ExecuteNonQuery();
                    ok1 = ExistTable(cmd, "hosts");
                    WriteInfo("Creating hosts table", ok1 ? "OK" : "ERROR", ok1 ? ConsoleColor.Green : ConsoleColor.Red);

                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS `credentials_http` (
  `HOST` bigint(20) NOT NULL,
  `PORT` smallint(5) unsigned NOT NULL,
  `PROTOCOL` enum('TCP','UDP') NOT NULL,
  `TYPE` varchar(100) NOT NULL DEFAULT '',
  `HTTP_HOST` varchar(1000) NOT NULL DEFAULT '',
  `HTTP_URL` varchar(1000) NOT NULL DEFAULT '',
  `USER` varchar(1000) NOT NULL DEFAULT '',
  `PASS` varchar(1000) NOT NULL DEFAULT '',
  PRIMARY KEY (`HOST`,`PORT`,`PROTOCOL`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
";
                    cmd.ExecuteNonQuery();
                    ok2 = ExistTable(cmd, "credentials_http");
                    WriteInfo("Creating credentials_http table", ok2 ? "OK" : "ERROR", ok2 ? ConsoleColor.Green : ConsoleColor.Red);

                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS `attacks_http` (
  `HOST` bigint(20) NOT NULL,
  `PORT` smallint(5) unsigned NOT NULL,
  `PROTOCOL` enum('TCP','UDP') NOT NULL,
  `TYPE` varchar(100) NOT NULL DEFAULT '',
  `HTTP_HOST` varchar(1000) NOT NULL DEFAULT '',
  `HTTP_URL` varchar(1000) NOT NULL DEFAULT '',
  `GET` json NOT NULL,
  `POST` json NOT NULL,
  PRIMARY KEY (`HOST`,`PORT`,`PROTOCOL`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
";
                    cmd.ExecuteNonQuery();
                    ok3 = ExistTable(cmd, "attacks_http");
                    WriteInfo("Creating attacks_http table", ok3 ? "OK" : "ERROR", ok3 ? ConsoleColor.Green : ConsoleColor.Red);
                }
            }

            try
            {
                XploitMongoRepository<HttpCredential> cHttpCredential = new XploitMongoRepository<HttpCredential>(Repository);
                WriteInfo("Creating mongo connection", "OK", ConsoleColor.Green);
                ok4 = true;
            }
            catch (Exception e)
            {
                WriteError(e.ToString());
            }

            return ok1 && ok2 && ok3 && ok4 ? ECheck.Ok : ECheck.Error;
        }

        bool ExistTable(MySqlCommand cmd, string table)
        {
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("t", table);
            cmd.CommandText = "show tables like @t";
            return (string)cmd.ExecuteScalar() == table;
        }

        public override bool Run()
        {
            // Check mongo
            XploitMongoRepository<HttpCredential> cHttpCredential = new XploitMongoRepository<HttpCredential>(Repository);

            CreateJob(new Task(DoJob));
            return true;
        }

        void DoJob()
        {
            XploitMongoRepository<TelnetCredential> cTelnetCredential = new XploitMongoRepository<TelnetCredential>(Repository);
            XploitMongoRepository<HttpAttack> cHttpAttack = new XploitMongoRepository<HttpAttack>(Repository);
            XploitMongoRepository<Pop3Credential> cPop3Credential = new XploitMongoRepository<Pop3Credential>(Repository);
            XploitMongoRepository<HttpCredential> cHttpCredential = new XploitMongoRepository<HttpCredential>(Repository);
            XploitMongoRepository<FTPCredential> cFTPCredential = new XploitMongoRepository<FTPCredential>(Repository);

            using (MySqlConnection con = new MySqlConnection(ConnectionString))
            {
                con.Open();

                using (MySqlCommand cmd = con.CreateCommand())
                {
                    // Watch collections
                    Task t1 = Task.Run(() => { foreach (RemovableValue<TelnetCredential> r in cTelnetCredential.Watch()) r.Remove = Run(r.Value, cmd); });
                    Task t2 = Task.Run(() => { foreach (RemovableValue<HttpAttack> r in cHttpAttack.Watch()) r.Remove = Run(r.Value, cmd); });
                    Task t3 = Task.Run(() => { foreach (RemovableValue<Pop3Credential> r in cPop3Credential.Watch()) r.Remove = Run(r.Value, cmd); });
                    Task t4 = Task.Run(() => { foreach (RemovableValue<HttpCredential> r in cHttpCredential.Watch()) r.Remove = Run(r.Value, cmd); });
                    Task t5 = Task.Run(() => { foreach (RemovableValue<FTPCredential> r in cFTPCredential.Watch()) r.Remove = Run(r.Value, cmd); });

                    Task.WaitAll(t1, t2, t3, t4, t5);
                }
            }
        }
        static internal bool Run(ExtractBase item, MySqlCommand cmd)
        {
            if (item == null) return false;

            // Raise insert
            ulong id = GetHostId(cmd, item);

            if (item is HttpAttack) InsertHttpAttack(cmd, id, (HttpAttack)item);
            else if (item is HttpCredential) InsertHttpCredential(cmd, id, (HttpCredential)item);

            else if (item is TelnetCredential) InsertTelnetCredential(cmd, id, (TelnetCredential)item);
            else if (item is Pop3Credential) InsertPop3Credential(cmd, id, (Pop3Credential)item);
            else if (item is FTPCredential) InsertFTPCredential(cmd, id, (FTPCredential)item);

            // Delete item
            return false;
        }
        static void InsertFTPCredential(MySqlCommand cmd, ulong id, FTPCredential item)
        {

        }
        static void InsertPop3Credential(MySqlCommand cmd, ulong id, Pop3Credential item)
        {

        }
        static void InsertTelnetCredential(MySqlCommand cmd, ulong id, TelnetCredential item)
        {

        }
        static void InsertHttpCredential(MySqlCommand cmd, ulong countryId, HttpCredential item)
        {

        }
        static void InsertHttpAttack(MySqlCommand cmd, ulong countryId, HttpAttack item)
        {

        }
        static ulong GetHostId(MySqlCommand cmd, ExtractBase it)
        {
            object o;
            lock (cmd)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("i", it.Address);
                cmd.CommandText = "SELECT ID FROM hosts WHERE IP= @i";
                o = cmd.ExecuteScalar();
            }
            // Exists
            if (o != null) return Convert.ToUInt64(o);

            // Not exists
            it.RecallCounty(GeoLite2LocationProvider.Current);

            lock (cmd)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("ip", it.Address);
                cmd.Parameters.AddWithValue("cont", EnsureLength(it.Continent, 2, true));
                cmd.Parameters.AddWithValue("count", EnsureLength(it.Country, 2, true));
                cmd.CommandText = "INSERT INTO hosts(IP,CONTINENT,COUNTRY)VALUES( @ip , @cont , @count )";
                cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();
                cmd.CommandText = "SELECT LAST_INSERT_ID()";

                o = cmd.ExecuteScalar();
            }

            return Convert.ToUInt64(o);
        }
        static object EnsureLength(string input, int length, bool ifExcedEmpty)
        {
            if (string.IsNullOrEmpty(input)) return "";

            if (input.Length > length)
                return ifExcedEmpty ? "" : input.Substring(0, length);
            return input;
        }
    }
}