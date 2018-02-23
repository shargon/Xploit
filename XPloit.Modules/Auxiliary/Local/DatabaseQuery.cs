﻿using System;
using System.Data;
using System.Data.Common;
using System.IO;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Command;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
using XPloit.Helpers.Attributes;

namespace Auxiliary.Local
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Database query")]
    public class DatabaseQuery : Module
    {
        public enum EFormat
        {
            Console = 0,
            Xml = 1,
            Txt = 2
        }

        #region Configure
        public override Reference[] References { get { return new Reference[] { new Reference(EReferenceType.URL, "http://www.connectionstrings.com/") }; } }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "Connection String")]
        public string ConnectionString { get; set; }
        [ConfigurableProperty(Description = "Sql query")]
        public string Query { get; set; }
        [ConfigurableProperty(Description = "Server type")]
        public EDbType ServerType { get; set; }

        // Format
        [ConfigurableProperty(Optional = true, Description = "Out File for Query")]
        public FileInfo QueryOutFile { get; set; }
        [ConfigurableProperty(Description = "Out File format for querys")]
        public EFormat QueryOutFormat { get; set; }
        #endregion

        public DatabaseQuery()
        {
            ConnectionString = "Server=myServerAddress;Port=3306;Database=myDataBase;Uid=myUsername;Pwd = myPassword;";
            Query = "Select 'X-Polit' as Framework ";
            ServerType = EDbType.MySql;
            QueryOutFormat = EFormat.Console;
        }

        public override ECheck Check()
        {
            DbConnection db = null;
            try
            {
                db = Get();
                if (db == null) return ECheck.Error;

                db.Open();
            }
            catch (Exception e)
            {
                WriteError(e.Message.ToString());
                return ECheck.Error;
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                    db.Dispose();
                }
            }
            return ECheck.Ok;
        }

        public override bool Run()
        {
            DbConnection db = Get();
            if (db == null) return false;

            WriteInfo("Connecting ...");

            using (db)
            {
                try { db.Open(); }
                catch (Exception e)
                {
                    WriteError(e.Message);
                    return false;
                }

                using (DbCommand cmd = db.CreateCommand())
                {
                    cmd.CommandText = Query;

                    if (IsQuery(Query))
                    {
                        WriteInfo("Detecting Query Type");
                        // Query
                        cmd.CommandType = CommandType.Text;
                        using (DataTable dt = new DataTable()
                        {
                            TableName = Query
                        })
                        {
                            using (DbDataAdapter adapter = DbProviderFactories.GetFactory(db).CreateDataAdapter())
                            {
                                adapter.SelectCommand = cmd;
                                adapter.Fill(dt);
                            }

                            CommandTable table = new CommandTable(dt);

                            switch (QueryOutFormat)
                            {
                                case EFormat.Console:
                                    {
                                        WriteTable(table);
                                        break;
                                    }
                                case EFormat.Txt:
                                    {
                                        File.WriteAllText(QueryOutFile.FullName, table.Output());
                                        break;
                                    }
                                case EFormat.Xml:
                                    {
                                        dt.WriteXml(QueryOutFile.FullName, XmlWriteMode.WriteSchema, true);
                                        break;
                                    }
                            }

                            if (QueryOutFormat != EFormat.Console)
                            {
                                QueryOutFile.Refresh();
                                WriteInfo("OutFile Size: ", QueryOutFile.Length.ToString(), QueryOutFile.Length <= 0 ? ConsoleColor.Red : ConsoleColor.Green);
                            }

                            WriteInfo("Rows    : ", dt.Rows.Count.ToString(), dt.Rows.Count <= 0 ? ConsoleColor.Red : ConsoleColor.Green);
                            WriteInfo("Columns : ", dt.Columns.Count.ToString(), dt.Columns.Count <= 0 ? ConsoleColor.Red : ConsoleColor.Green);
                        }
                    }
                    else
                    {
                        // Non query
                        WriteInfo("Detecting Non-Query Type");

                        int x = cmd.ExecuteNonQuery();
                        WriteInfo("Affected rows", x.ToString(), x <= 0 ? ConsoleColor.Red : ConsoleColor.Green);
                    }

                    WriteInfo("Disconnecting");
                    db.Close();
                }
            }

            return true;
        }

        bool IsQuery(string query)
        {
            query = query.Trim(' ', '(');

            if (query.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase)) return true;
            if (query.StartsWith("SHOW", StringComparison.InvariantCultureIgnoreCase)) return true;
            if (query.StartsWith("DESC", StringComparison.InvariantCultureIgnoreCase)) return true;

            return false;
        }

        public DbConnection Get()
        {
            if (QueryOutFormat != EFormat.Console)
            {
                QueryOutFile.Refresh();

                if (QueryOutFile == null)
                    throw (new Exception(QueryOutFormat.ToString() + " Format require set 'QueryOutFile' Property"));
            }

            return DBHelper.GetDB(ServerType, ConnectionString);
        }
    }
}