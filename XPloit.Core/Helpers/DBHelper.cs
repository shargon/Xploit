using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using XPloit.Core.Enums;

namespace XPloit.Core.Helpers
{
    public class DBHelper
    {
        public class DbCommandEx : IDbCommand
        {
            EDbType _Type;
            IDbCommand _Command;

            public EDbType Type { get { return _Type; } }

            public IDbConnection Connection { get { return _Command.Connection; } set { _Command.Connection = value; } }
            public IDbTransaction Transaction { get { return _Command.Transaction; } set { _Command.Transaction = value; } }
            public string CommandText { get { return _Command.CommandText; } set { _Command.CommandText = value; } }
            public int CommandTimeout { get { return _Command.CommandTimeout; } set { _Command.CommandTimeout = value; } }
            public CommandType CommandType { get { return _Command.CommandType; } set { _Command.CommandType = value; } }
            public IDataParameterCollection Parameters { get { return _Command.Parameters; } }
            public UpdateRowSource UpdatedRowSource { get { return _Command.UpdatedRowSource; } set { _Command.UpdatedRowSource = value; } }

            public void Cancel() { _Command.Cancel(); }
            public IDbDataParameter CreateParameter() { return _Command.CreateParameter(); }

            public void Dispose() { _Command.Dispose(); }
            public int ExecuteNonQuery() { return _Command.ExecuteNonQuery(); }
            public IDataReader ExecuteReader() { return _Command.ExecuteReader(); }
            public IDataReader ExecuteReader(CommandBehavior behavior) { return _Command.ExecuteReader(behavior); }
            public object ExecuteScalar() { return _Command.ExecuteScalar(); }
            public void Prepare() { _Command.Prepare(); }

            public DbCommandEx(DbConnection con) : this(con.CreateCommand()) { }
            public DbCommandEx(IDbCommand command)
            {
                _Command = command;

                if (command is MySqlCommand) _Type = EDbType.MySql;
                else if (command is SqlCommand) _Type = EDbType.SqlServer;
                else if (command is OleDbCommand) _Type = EDbType.OleDb;
                else if (command is OdbcCommand) _Type = EDbType.Odbc;
                //else if (command is OracleCommand) _Type = EDbType.Oracle;
            }

            public void AddParameter(string name, object value)
            {
                switch (Type)
                {
                    case EDbType.Oracle: ((OracleCommand)_Command).Parameters.Add(name, value); break;
                    case EDbType.SqlServer: ((SqlCommand)_Command).Parameters.AddWithValue(name, value); break;
                    case EDbType.OleDb: ((OleDbCommand)_Command).Parameters.AddWithValue(name, value); break;
                    case EDbType.Odbc: ((OdbcCommand)_Command).Parameters.AddWithValue(name, value); break;
                    case EDbType.MySql: ((MySqlCommand)_Command).Parameters.AddWithValue(name, value); break;
                }

                //IDbDataParameter par = _Command.CreateParameter();
                //par.Value = value;
                //par.ParameterName = name;

                //_Command.Parameters.Add(par);
            }

            public bool TableExists(string table)
            {
                Parameters.Clear();

                switch (Type)
                {
                    case EDbType.Oracle:
                        {
                            break;
                        }
                    case EDbType.SqlServer:
                        {
                            break;
                        }
                    case EDbType.OleDb:
                        {
                            break;
                        }
                    case EDbType.Odbc:
                        {
                            break;
                        }
                    case EDbType.MySql:
                        {
                            AddParameter("t", table);
                            CommandText = "show tables like @t";
                            return (string)ExecuteScalar() == table;
                        }
                }

                throw (new NotImplementedException());
            }
        }

        public static DbConnection GetDB(EDbType type, string connectionString)
        {
            switch (type)
            {
                case EDbType.Oracle: return new OracleConnection(connectionString);
                case EDbType.SqlServer: return new SqlConnection(connectionString);
                case EDbType.OleDb: return new OleDbConnection(connectionString);
                case EDbType.Odbc: return new OdbcConnection(connectionString);
                case EDbType.MySql: return new MySqlConnection(connectionString);
            }
            return null;
        }
    }
}
