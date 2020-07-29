using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace commanet.Db
{
    public static class DbParam
    {
        public static KeyValuePair<string, object> New(string name, object value) => new KeyValuePair<string, object>(name, value);
    }
    public class SQLDBConnection : IDisposable
    {

        public DbConnection? SqlConnection { get; private set; }

        public string DbType { get; private set; } = "sqlite";
        public string DbUser { get => cinfo.DbUser; }
        public string DbPassword { get => cinfo.DbPassword; }
        public string DbConnection { get => cinfo.DbConnectionString; }
        public DbTransaction? CurrentTransaction { get; set; }

        public SQLDBConnection(string dbType, SqlDbConnectionInfo connectionInfo)
        {
            CreateSQLDBConnection(dbType, connectionInfo);
        }

        public SQLDBConnection(string fullConnectionString)
        {
            if (string.IsNullOrEmpty(fullConnectionString))
            {
                #pragma warning disable CA1303 // Do not pass literals as localized parameters
                throw new Exception("Connection string can not be empty");
                #pragma warning restore CA1303 // Do not pass literals as localized parameters
            }

            var connectionInfo = new SqlDbConnectionInfo();
            var dbType = "";

            var paramVars = fullConnectionString.Split(';');
            foreach (var v in paramVars)
            {
                if (string.IsNullOrEmpty(v)) continue;
                if (!v.Contains('=', StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new Exception($"SQLDBConnection: Incorrect connection string variable: {v}");
                }
                var kv = v.Split('=');

                var err = $"Wrong parameter {kv[0].Trim()} value format: {kv[1].Trim()}";
                switch (kv[0].Trim().ToUpperInvariant())
                {
                    case "TYPE": dbType = kv[1].Trim(); break;
                    case "USER": connectionInfo.DbUser = kv[1].Trim(); break;
                    case "PASSWORD": connectionInfo.DbPassword = kv[1].Trim(); break;
                    case "CONNECTION": connectionInfo.DbConnectionString = kv[1].Trim(); break;
                    case "POOLING":
                        var b = true;
                        if (bool.TryParse(kv[1].Trim(), out b)) connectionInfo.Pooling = b;
                        else throw new Exception(err);
                        break;
                    case "CONNECTIONTIMEOUT":
                        uint uiv = 0;
                        if (uint.TryParse(kv[1].Trim(), out uiv)) connectionInfo.ConnectionTimeout = uiv;
                        else throw new Exception(err);
                        break;
                    case "CONNECTIONLIFETIME":
                        uiv = 0;
                        if (uint.TryParse(kv[1].Trim(), out uiv)) connectionInfo.ConnectionLifetime = uiv;
                        else throw new Exception(err);
                        break;
                    case "DECRPOOLSIZE":
                        uiv = 0;
                        if (uint.TryParse(kv[1].Trim(), out uiv)) connectionInfo.IncrPoolSize = uiv;
                        else throw new Exception(err);
                        break;
                    case "INCRPOOLSIZE":
                        uiv = 0;
                        if (uint.TryParse(kv[1].Trim(), out uiv)) connectionInfo.DecrPoolSize = uiv;
                        else throw new Exception(err);
                        break;
                    case "LOADBALANCING":
                        b = false;
                        if (bool.TryParse(kv[1].Trim(), out b)) connectionInfo.LoadBalancing = b;
                        else throw new Exception(err);
                        break;
                    case "HAEVENTS":
                        b = false;
                        if (bool.TryParse(kv[1].Trim(), out b)) connectionInfo.HAEvents = b;
                        else throw new Exception(err);
                        break;
                    default:
                        throw new Exception($"SQLDBConnection: Unknown connection string variable: {kv[0].Trim()}");
                }

            }

            CreateSQLDBConnection(dbType, connectionInfo);
        }

        public SQLDBConnection(string dbType, string dbUser, string dbPassword, string dbConnection)
        {
            CreateSQLDBConnection(dbType, new SqlDbConnectionInfo()
            {
                DbUser = dbUser,
                DbPassword = dbPassword,
                DbConnectionString = dbConnection
            });
        }

        ~SQLDBConnection()
        {
            Dispose(false);
        }

        private void CreateSQLDBConnection(string dbType, SqlDbConnectionInfo connectionInfo)
        {
            DbType = dbType;

            cinfo = connectionInfo;

            info = SqlDbInfo.GetDbTypeInfo(DbType) ?? throw
                new Exception($"Database provider {dbType} is not supported");

            var conStr = SqlDbInfo.CreateConnectionString(info, cinfo);
            var cFactory = SqlDbInfo.GetDbProviderFactory(info) ??
                        throw new Exception($"Can not find DB provider factory for {dbType}");

            SqlConnection = cFactory.CreateConnection();
            SqlConnection.ConnectionString = conStr;
        }


        public void Open()
        {
            try
            {
                SqlConnection?.Open();
            }
            catch (Exception ex)
            {
                if (info.Type == SqlDbType.Odbc)
                {
                    throw new Exception(
$@"ODBC Exception happens. Check connection string and verify if provided ODBC driver is installed for current architecture. 
For example for MS Access x64 is required to install AccessDatabaseEngine_X64.exe (can be downloaded from Microsoft site).
Check driver name - it should be exactly same as showed ODBC Datsource Administrator. 
 Example: Driver={{Microsoft Access Driver (*.mdb, *.accdb)}}
ODBC Error:
{ex.Message}");
                }
            }
        }

        public void Close()
        {
            try
            {
                SqlConnection?.Close();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception) { }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        public bool IsConnected { get { return (SqlConnection != null) && SqlConnection.State == ConnectionState.Open; } }

        private bool isDisposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                CurrentTransaction?.Dispose();
                SqlConnection?.Dispose();
            }

            isDisposed = true;
        }

        #region Internal Auxiliary Methods 
        internal static IDbDataParameter Param(DbCommand cmd, string name, object value, DbType? dbtype = null)
        {
            var lvalue = value;
            if (value != null && value.GetType() == typeof(bool))
                if ((bool)value) lvalue = 1; else lvalue = 0;
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            if (dbtype != null) param.DbType = (DbType)dbtype;
            if (value != null && value.GetType().IsEnum)
            {
                param.Value = (int)lvalue;
            }
            else param.Value = lvalue;
            return param;
        }

        internal static Regex rxComments = new Regex(@"//.*\n|/\*(.|\n)*?\*/");
        internal static Regex rxParams = new Regex(@":(?=(?:(?:[^']*'){2})*[^']*$)");
        internal static Regex rxParamsFullName = new Regex(@":(?=(?:(?:[^']*'){2})*[^']*$)\w*");
        internal static Regex rxLeadingSpaces = new Regex(@"^(\s|\t)+", RegexOptions.Multiline);
        internal static Regex rxTrailingSpaces = new Regex(@"(\s|\t)+$", RegexOptions.Multiline);

        internal SqlDbTypeInfo info = new SqlDbTypeInfo();
        internal SqlDbConnectionInfo cinfo = new SqlDbConnectionInfo();

        internal string SubstParamSymb(string SQL)
        {
            var res = rxComments.Replace(SQL, ""); // Remove /* */ and -- comments
            if (info.Type == SqlDbType.Odbc)                   // ODBC supports only positional
                return rxParamsFullName.Replace(res, "?");     // parameters with ? as position placeholder  
            var ps = "" + info.PARAMSYMBOL;
            if (ps == ":") return res;
            return rxParams.Replace(res, ps); // Making substitution with ignoring string constants 
        }

        internal static string Pack(string SQL)
            => rxLeadingSpaces.Replace(rxTrailingSpaces.Replace(SQL, ""), " ");

        internal static void BindParameters(DbCommand cmd, KeyValuePair<string, object>[] Parameters)
        {
            cmd.Parameters.Clear();
            foreach (var p in Parameters)
            {
                cmd.Parameters.Add(Param(cmd, p.Key, p.Value));
            }
        }

        internal void CheckConnection()
        {
            if (SqlConnection == null || SqlConnection.State == ConnectionState.Closed || SqlConnection.State == ConnectionState.Broken)
            {
                Close();
                Open();
                if (SqlConnection == null || SqlConnection.State == ConnectionState.Closed)
                    throw new Exception(
$@"Can't connect to Database: 
DbType: {DbType}, DbUser: {DbUser}, DbPassword: {DbPassword}, DbConnection: {DbConnection}"
                    );
            }
        }
        #endregion

        #region Data Manipulation (DML)  Operations

        public void Transaction(Action<TransactionHelper> ac, IsolationLevel isolation = IsolationLevel.ReadCommitted, bool NoTransaction = false)
        {
            CheckConnection();
            var th = new TransactionHelper(this, isolation, NoTransaction);
            try
            {
                ac?.Invoke(th);
                th.Commit();
            }
            catch (Exception)
            {
                th.Rollback();
                throw;
            }
        }

        #endregion

        #region Data Read Methods

        public DataTable GetSchema(string SchemaName)
        {
            CheckConnection();
            // Suppress static analyser warning because all neccessative
            // checks performed in line above
            #pragma warning disable CS8602 // Dereference of a possibly null reference.
            return SqlConnection.GetSchema(SchemaName);
            #pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        public void ExecuteReader(string SQL, Func<DbDataReader, bool> ac, params KeyValuePair<string, object>[] Parameters)
        {
            CheckConnection();
            try
            {
                List<IDbDataParameter> SqlParams = new List<IDbDataParameter>();

                #pragma warning disable CS8602 // Dereference of a possibly null reference.
                using var cmd = SqlConnection.CreateCommand();
                #pragma warning restore CS8602 // Dereference of a possibly null reference.

                if (CurrentTransaction != null)
                    cmd.Transaction = CurrentTransaction;

                #pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                cmd.CommandText = SubstParamSymb(Pack(SQL));
                #pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

                BindParameters(cmd, Parameters);
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    if (!ac(rd)) break;
                }
                rd.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error in SQL:\n" + SQL + "\n" + ex.Message);
            }

        }

        public List<T> ReadVector<T>(string SQL, params KeyValuePair<string, object>[] Parameters)
        {
            var res = new List<T>();
            ExecuteReader(SQL, (rd) => {
                for (int i = 0; i < rd.FieldCount; i++)
                {
                    var typ = typeof(T);
                    typ = Nullable.GetUnderlyingType(typ) ?? typ;
                    var v = (T)Convert.ChangeType(rd.GetValue(i), typ,CultureInfo.InvariantCulture);
                    res.Add(v);
                }
                return false;
            }, Parameters);
            return res;
        }

        public T ReadOne<T>(string SQL, params KeyValuePair<string, object>[] Parameters)
        {
            T res = default;
            ExecuteReader(SQL, (rd) => {
                if (!rd.IsDBNull(0))
                {
                    var typ = typeof(T);
                    typ = Nullable.GetUnderlyingType(typ) ?? typ;
                    res = (T)Convert.ChangeType(rd.GetValue(0), typ, CultureInfo.InvariantCulture);
                }
                return false;
            }, Parameters);
            #pragma warning disable CS8603 // Possible null reference return.
            return res;
            #pragma warning restore CS8603 // Possible null reference return.
        }

        public int? ReadOneInt(string SQL, params KeyValuePair<string, object>[] Parameters)        
            => ReadOne<int?>(SQL,Parameters);        

        public long? ReadOneLong(string SQL, params KeyValuePair<string, object>[] Parameters)
            => ReadOne<long?>(SQL, Parameters);

        public double? ReadOneDouble(string SQL, params KeyValuePair<string, object>[] Parameters)
            => ReadOne<double?>(SQL, Parameters);

        public decimal? ReadOneDecimal(string SQL, params KeyValuePair<string, object>[] Parameters)
            => ReadOne<decimal?>(SQL, Parameters);

        public DateTime? ReadOneDateTime(string SQL, params KeyValuePair<string, object>[] Parameters)
            => ReadOne<DateTime?>(SQL, Parameters);

        public string? ReadOneString(string SQL, params KeyValuePair<string, object>[] Parameters)
            => ReadOne<string?>(SQL, Parameters);

        #endregion

    }
}

