using MongoDB.Driver;
using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Mongo;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;
using XPloit.Helpers.Geolocate;
using XPloit.Sniffer.Extractors;

namespace Auxiliary.ETL
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
                    // -----
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS  `hosts` (
  `ID` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `IP` varchar(40) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `CONTINENT` char(2) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `COUNTRY` char(2) NOT NULL DEFAULT '',
  `DATE_INS` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `IP_UNIQUE` (`IP`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
";

                    cmd.ExecuteNonQuery();
                    ok1 = ExistTable(cmd, "hosts");
                    WriteInfo("Creating hosts table", ok1 ? "OK" : "ERROR", ok1 ? ConsoleColor.Green : ConsoleColor.Red);
                    // -----
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS  `credentials_http` (
  `HOST` bigint(20) NOT NULL,
  `PORT` smallint(5) unsigned NOT NULL,
  `TYPE` varchar(20) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `USER` varchar(100) NOT NULL DEFAULT '',
  `PASS` varchar(100) NOT NULL DEFAULT '',
  `VALID` tinyint(4) NOT NULL DEFAULT '0',
  `DATE` date NOT NULL,
  `HOUR` char(5) CHARACTER SET latin1 NOT NULL DEFAULT '00:00',
  `HTTP_CRC` char(32) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `HTTP_HOST` varchar(1000) NOT NULL DEFAULT '',
  `HTTP_URL` varchar(1000) NOT NULL DEFAULT '',
  PRIMARY KEY (`HTTP_CRC`,`HOUR`,`DATE`,`VALID`,`PASS`,`USER`,`TYPE`,`PORT`,`HOST`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
";
                    cmd.ExecuteNonQuery();

                    ok2 = ExistTable(cmd, "credentials_http");
                    WriteInfo("Creating credentials_http table", ok2 ? "OK" : "ERROR", ok2 ? ConsoleColor.Green : ConsoleColor.Red);// -----
                    // -----
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS  `credentials_telnet` (
  `HOST` bigint(20) NOT NULL,
  `PORT` smallint(5) unsigned NOT NULL,
  `TYPE` varchar(20) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `USER` varchar(100) NOT NULL DEFAULT '',
  `PASS` varchar(100) NOT NULL DEFAULT '',
  `VALID` tinyint(4) NOT NULL DEFAULT '0',
  `DATE` date NOT NULL,
  `HOUR` char(5) CHARACTER SET latin1 NOT NULL DEFAULT '00:00',
  PRIMARY KEY (`HOST`,`PORT`,`TYPE`,`USER`,`PASS`,`VALID`,`DATE`,`HOUR`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
";
                    cmd.ExecuteNonQuery();

                    ok2 = ExistTable(cmd, "credentials_telnet");
                    WriteInfo("Creating credentials_telnet table", ok2 ? "OK" : "ERROR", ok2 ? ConsoleColor.Green : ConsoleColor.Red);
                    // -----
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS  `credentials_ftp` (
  `HOST` bigint(20) NOT NULL,
  `PORT` smallint(5) unsigned NOT NULL,
  `TYPE` varchar(20) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `USER` varchar(100) NOT NULL DEFAULT '',
  `PASS` varchar(100) NOT NULL DEFAULT '',
  `VALID` tinyint(4) NOT NULL DEFAULT '0',
  `DATE` date NOT NULL,
  `HOUR` char(5) CHARACTER SET latin1 NOT NULL DEFAULT '00:00',
  PRIMARY KEY (`HOST`,`PORT`,`TYPE`,`USER`,`PASS`,`VALID`,`DATE`,`HOUR`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
";
                    cmd.ExecuteNonQuery();

                    ok2 = ExistTable(cmd, "credentials_ftp");
                    WriteInfo("Creating credentials_ftp table", ok2 ? "OK" : "ERROR", ok2 ? ConsoleColor.Green : ConsoleColor.Red);
                    // -----
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS  `credentials_pop3` (
  `HOST` bigint(20) NOT NULL,
  `PORT` smallint(5) unsigned NOT NULL,
  `TYPE` varchar(20) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `AUTH_TYPE` varchar(20) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `USER` varchar(100) NOT NULL DEFAULT '',
  `PASS` varchar(100) NOT NULL DEFAULT '',
  `VALID` tinyint(4) NOT NULL DEFAULT '0',
  `DATE` date NOT NULL,
  `HOUR` char(5) CHARACTER SET latin1 NOT NULL DEFAULT '00:00',
  PRIMARY KEY (`DATE`,`HOUR`,`VALID`,`USER`,`PASS`,`AUTH_TYPE`,`TYPE`,`PORT`,`HOST`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
;";
                    cmd.ExecuteNonQuery();

                    ok2 = ExistTable(cmd, "credentials_pop3");
                    WriteInfo("Creating credentials_pop3 table", ok2 ? "OK" : "ERROR", ok2 ? ConsoleColor.Green : ConsoleColor.Red);
                    // -----

                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS `attacks_http` (
  `HOST` bigint(20) NOT NULL,
  `PORT` smallint(5) unsigned NOT NULL,
  `TYPE` varchar(20) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `DATE` date NOT NULL,
  `HOUR` char(5) CHARACTER SET latin1 NOT NULL DEFAULT '00:00',
  `HTTP_CRC` char(32) CHARACTER SET latin1 NOT NULL,
  `HTTP_HOST` varchar(200) NOT NULL DEFAULT '',
  `HTTP_URL` varchar(1000) NOT NULL DEFAULT '',
  `HTTP_QUERY` json NOT NULL,
  PRIMARY KEY (`HOST`,`PORT`,`TYPE`,`DATE`,`HOUR`,`HTTP_CRC`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
                    ";
                    cmd.ExecuteNonQuery();
                    ok3 = ExistTable(cmd, "attacks_http");
                    WriteInfo("Creating attacks_http table", ok3 ? "OK" : "ERROR", ok3 ? ConsoleColor.Green : ConsoleColor.Red);
                    // -----
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

        static string NotNull(string input)
        {
            if (input == null) return "";
            return input;
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

            if (item is HttpAttack) return InsertHttpAttack(cmd, id, (HttpAttack)item);
            else if (item is HttpCredential) return InsertHttpCredential(cmd, id, (HttpCredential)item);

            else if (item is TelnetCredential) return InsertTelnetCredential(cmd, id, (TelnetCredential)item);
            else if (item is Pop3Credential) return InsertPop3Credential(cmd, id, (Pop3Credential)item);
            else if (item is FTPCredential) return InsertFTPCredential(cmd, id, (FTPCredential)item);

            // Delete item
            return false;
        }

        class HttpQuery
        {
            public string[] Post { get; set; }
            public string[] Get { get; set; }
        }

        static bool InsertFTPCredential(MySqlCommand cmd, ulong countryId, FTPCredential item)
        {
            lock (cmd)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("HOST", countryId);
                cmd.Parameters.AddWithValue("PORT", item.Port);
                cmd.Parameters.AddWithValue("TYPE", item.Type.ToString());
                cmd.Parameters.AddWithValue("DATE", item.Date.Substring(0, 10));
                cmd.Parameters.AddWithValue("HOUR", item.Date.Substring(11, 5));
                cmd.Parameters.AddWithValue("USER", NotNull(item.User));
                cmd.Parameters.AddWithValue("PASS", NotNull(item.Password));
                cmd.Parameters.AddWithValue("VALID", item.IsValid ? 1 : 0);
                cmd.CommandText = "INSERT IGNORE INTO credentials_ftp(HOST,PORT,TYPE,DATE,HOUR,USER,PASS,VALID)VALUES(@HOST,@PORT,@TYPE,@DATE,@HOUR,@USER,@PASS,@VALID);";
                cmd.ExecuteNonQuery();
                return true;
            }
        }
        static bool InsertPop3Credential(MySqlCommand cmd, ulong countryId, Pop3Credential item)
        {
            lock (cmd)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("HOST", countryId);
                cmd.Parameters.AddWithValue("PORT", item.Port);
                cmd.Parameters.AddWithValue("TYPE", item.Type.ToString());
                cmd.Parameters.AddWithValue("AUTH_TYPE", NotNull(item.AuthType));
                cmd.Parameters.AddWithValue("DATE", item.Date.Substring(0, 10));
                cmd.Parameters.AddWithValue("HOUR", item.Date.Substring(11, 5));
                cmd.Parameters.AddWithValue("USER", NotNull(item.User));
                cmd.Parameters.AddWithValue("PASS", NotNull(item.Password));
                cmd.Parameters.AddWithValue("VALID", item.IsValid ? 1 : 0);
                cmd.CommandText = "INSERT IGNORE INTO credentials_pop3(HOST,PORT,TYPE,AUTH_TYPE,DATE,HOUR,USER,PASS,VALID)VALUES(@HOST,@PORT,@TYPE,@AUTH_TYPE,@DATE,@HOUR,@USER,@PASS,@VALID);";
                cmd.ExecuteNonQuery();
                return true;
            }
        }
        static bool InsertTelnetCredential(MySqlCommand cmd, ulong countryId, TelnetCredential item)
        {
            lock (cmd)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("HOST", countryId);
                cmd.Parameters.AddWithValue("PORT", item.Port);
                cmd.Parameters.AddWithValue("TYPE", item.Type.ToString());
                cmd.Parameters.AddWithValue("DATE", item.Date.Substring(0, 10));
                cmd.Parameters.AddWithValue("HOUR", item.Date.Substring(11, 5));
                cmd.Parameters.AddWithValue("USER", NotNull(item.User));
                cmd.Parameters.AddWithValue("PASS", NotNull(item.Password));
                cmd.Parameters.AddWithValue("VALID", item.IsValid ? 1 : 0);
                cmd.CommandText = "INSERT IGNORE INTO credentials_telnet(HOST,PORT,TYPE,DATE,HOUR,USER,PASS,VALID)VALUES(@HOST,@PORT,@TYPE,@DATE,@HOUR,@USER,@PASS,@VALID);";
                cmd.ExecuteNonQuery();
                return true;
            }
        }
        static bool InsertHttpCredential(MySqlCommand cmd, ulong countryId, HttpCredential item)
        {
            lock (cmd)
            {
                string url = NotNull(item.HttpUrl);
                string host = NotNull(item.HttpHost);
                string crc = HashHelper.HashHex(HashHelper.EHashType.Md5, host + "\n" + url);

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("HOST", countryId);
                cmd.Parameters.AddWithValue("PORT", item.Port);
                cmd.Parameters.AddWithValue("TYPE", item.Type.ToString());
                cmd.Parameters.AddWithValue("DATE", item.Date.Substring(0, 10));
                cmd.Parameters.AddWithValue("HOUR", item.Date.Substring(11, 5));
                cmd.Parameters.AddWithValue("HTTP_CRC", crc);
                cmd.Parameters.AddWithValue("HTTP_URL", url);
                cmd.Parameters.AddWithValue("HTTP_HOST", host);
                cmd.Parameters.AddWithValue("USER", NotNull(item.User));
                cmd.Parameters.AddWithValue("PASS", NotNull(item.Password));
                cmd.Parameters.AddWithValue("VALID", item.IsValid ? 1 : 0);
                cmd.CommandText = "INSERT IGNORE INTO credentials_http(HOST,PORT,TYPE,HTTP_HOST,HTTP_URL,HTTP_CRC,DATE,HOUR,USER,PASS,VALID)VALUES(@HOST,@PORT,@TYPE,@HTTP_HOST,@HTTP_URL,@HTTP_CRC,@DATE,@HOUR,@USER,@PASS,@VALID);";
                cmd.ExecuteNonQuery();
                return true;
            }
        }
        static bool InsertHttpAttack(MySqlCommand cmd, ulong countryId, HttpAttack item)
        {
            lock (cmd)
            {
                HttpQuery d = new HttpQuery()
                {
                    Post = item.Post,
                    Get = item.Get
                };

                string query = JsonHelper.Serialize(d);
                string url = NotNull(item.HttpUrl);
                string host = NotNull(item.HttpHost);
                string crc = HashHelper.HashHex(HashHelper.EHashType.Md5, host + "\n" + url + "\n" + query);

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("HOST", countryId);
                cmd.Parameters.AddWithValue("PORT", item.Port);
                cmd.Parameters.AddWithValue("TYPE", item.Type.ToString());
                cmd.Parameters.AddWithValue("DATE", item.Date.Substring(0, 10));
                cmd.Parameters.AddWithValue("HOUR", item.Date.Substring(11, 5));
                cmd.Parameters.AddWithValue("HTTP_QUERY", query);
                cmd.Parameters.AddWithValue("HTTP_URL", url);
                cmd.Parameters.AddWithValue("HTTP_HOST", host);
                cmd.Parameters.AddWithValue("HTTP_CRC", crc);
                cmd.CommandText = "INSERT IGNORE INTO attacks_http(HOST,PORT,TYPE,DATE,HOUR,HTTP_HOST,HTTP_URL,HTTP_QUERY,HTTP_CRC)VALUES(@HOST,@PORT,@TYPE,@DATE,@HOUR,@HTTP_HOST,@HTTP_URL,@HTTP_QUERY,@HTTP_CRC);";
                cmd.ExecuteNonQuery();
                return true;
            }
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

                // Exists
                if (o != null) return Convert.ToUInt64(o);

                // Not exists
                it.RecallCounty(GeoLite2LocationProvider.Current);

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

            if (input.Length > length) return ifExcedEmpty ? "" : input.Substring(0, length);
            return input;
        }
    }
}