using System;
using System.Collections.Generic;
using System.IO;
using System.Data.Common;
using System.Reflection;
using System.Globalization;


namespace commanet.Db
{
    public static class SqlDbInfo
    {
        private static readonly List<SqlDbTypeInfo> infos = new List<SqlDbTypeInfo>();
        static SqlDbInfo()
        {
            var di = new SqlDbTypeInfo()
            {
                Type = SqlDbType.Oracle,
                ADOProviderAssemblyName = "Oracle.ManagedDataAccess",
                ADOProviderFactoryName = "Oracle.ManagedDataAccess.Client.OracleClientFactory",
                PARAMSYMBOL = ":"
            };
            infos.Add(di);

            di = new SqlDbTypeInfo()
            {
                Type = SqlDbType.SqlServer,
                ADOProviderAssemblyName = "System.Data.SqlClient",
                ADOProviderFactoryName = "System.Data.SqlClient.SqlClientFactory",
                PARAMSYMBOL = "@"
            };
            infos.Add(di);

            di = new SqlDbTypeInfo()
            {
                Type = SqlDbType.PostgreSQL,
                ADOProviderAssemblyName = "Npgsql",
                ADOProviderFactoryName = "Npgsql.NpgsqlFactory",
                PARAMSYMBOL = ":"
            };
            infos.Add(di);

            di = new SqlDbTypeInfo()
            {
                Type = SqlDbType.Sqlite,
                ADOProviderAssemblyName = "System.Data.SQLite",
                ADOProviderFactoryName = "System.Data.SQLite.SQLiteFactory",
                PARAMSYMBOL = ":"
            };
            infos.Add(di);

            di = new SqlDbTypeInfo()
            {
                Type = SqlDbType.Odbc,
                ADOProviderAssemblyName = "System.Data.Odbc",
                ADOProviderFactoryName = "System.Data.Odbc.OdbcFactory",
                PARAMSYMBOL = "@" // Not used for ODBC. Parameters identified only by position not by name
            };
            infos.Add(di);
        }

        public static DbProviderFactory? GetDbProviderFactory(SqlDbTypeInfo typeInfo)
        {
            if (typeInfo == null)
                throw new ArgumentNullException(nameof(typeInfo));
            Assembly? providerAssembly;
            try
            {
                providerAssembly = Assembly.Load(typeInfo.ADOProviderAssemblyName);
            }
            catch (FileNotFoundException)
            {
                providerAssembly = null;
                throw new Exception(
                    $"Not found assembly {typeInfo.ADOProviderAssemblyName}.dll. It must be referenced in project or placed in bin directory"
                );
            }
            catch (Exception) {
                throw;
            }

            if (providerAssembly == null)
                throw new Exception(
$@"ADO.Net provider assembly {typeInfo.ADOProviderAssemblyName}not found. 
Put this assembly in same folder where is executable file or make reference in the project.
NOTICE! Use Dot.Net Core compatible version of provider assemblies");

            var t = providerAssembly.GetType(typeInfo.ADOProviderFactoryName, true);
            if (t != null)
            {
                var fi = t.GetField("Instance");
                if(fi!=null)
                    return fi.GetValue(null) as DbProviderFactory;
            }
            return null;
        }

        public static SqlDbTypeInfo? GetDbTypeInfo(string DbTypeName)
        {
            return infos.Find(i => i.Name.ToUpperInvariant() == DbTypeName.Trim().ToUpperInvariant());
        }

        public static string CreateConnectionString(SqlDbTypeInfo dbInfo, SqlDbConnectionInfo connectionInfo)
        {
            if (dbInfo == null)
                throw new ArgumentNullException(nameof(dbInfo));
            if (connectionInfo == null)
                throw new ArgumentNullException(nameof(connectionInfo));

            var builder = new DbConnectionStringBuilder();
            switch (dbInfo.Type)
            {
                case SqlDbType.Oracle:
                    {
                        builder["User ID"] = connectionInfo.DbUser;
                        builder["Password"] = connectionInfo.DbPassword;
                        builder["Pooling"] = connectionInfo.Pooling;
                        builder["Connection Timeout"] = connectionInfo.ConnectionTimeout;
                        builder["Connection Lifetime"] = connectionInfo.ConnectionLifetime;
                        builder["Incr Pool Size"] = connectionInfo.IncrPoolSize;
                        builder["Decr Pool Size"] = connectionInfo.DecrPoolSize;
                        if(connectionInfo.LoadBalancing)
                            builder["Load Balancing"] = connectionInfo.LoadBalancing;
                        if(connectionInfo.HAEvents)
                            builder["HA Events"] = connectionInfo.HAEvents;
                        var tmp = connectionInfo.DbConnectionString.Split(':');
                        var host = tmp[0];
                        var port = 1521;
                        var service = "LEVEL2";
                        if (tmp.Length == 3)
                        {
                            int.TryParse(tmp[1],NumberStyles.Integer,CultureInfo.InvariantCulture.NumberFormat,  out port);
                            service = tmp[2];
                        }
                        if (tmp.Length == 2) service = tmp[1];
                        var ds = $"(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = {host})(PORT = {port})))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = {service})))";
                        builder.Add("Data Source", ds);
                        return builder.ConnectionString;
                    }
                case SqlDbType.SqlServer:
                    {
                        builder["User ID"] = connectionInfo.DbUser;
                        builder["Password"] = connectionInfo.DbPassword;
                        builder["Pooling"] = connectionInfo.Pooling;
                        builder["Connection Timeout"] = connectionInfo.ConnectionTimeout;
                        builder["Connection Lifetime"] = connectionInfo.ConnectionLifetime;
                        builder["MultipleActiveResultSets"] = true;

                        var tmp = connectionInfo.DbConnectionString.Split(':');
                        var host = "localhost";
                        var database = "";
                        if (tmp.Length > 0) host = tmp[0];
                        if (tmp.Length > 1) database = tmp[1];
                        builder.Add("Data Source",host);
                        if (!string.IsNullOrEmpty(database))
                            builder.Add("Database",database);                        
                        return builder.ConnectionString;
                    }
                case SqlDbType.PostgreSQL:
                    {
                        builder["User ID"] = connectionInfo.DbUser;
                        builder["Password"] = connectionInfo.DbPassword;
                        builder["Pooling"] = connectionInfo.Pooling;
                        builder["Timeout"] = connectionInfo.ConnectionTimeout;

                        var tmp = connectionInfo.DbConnectionString.Split(':');
                        var host = tmp[0];
                        var port = 5432;
                        var database = "postgres";
                        if (tmp.Length > 1) int.TryParse(tmp[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out port);
                        if (tmp.Length > 2) database = tmp[2];
                        builder["Server"] = host;
                        builder["Port"] = port;
                        builder["Database"] = database;
                        return builder.ConnectionString;
                    }
                case SqlDbType.Sqlite:
                    {
                        var path = connectionInfo.DbConnectionString;
                        builder["Pooling"] = connectionInfo.Pooling;
                        if (!Path.IsPathRooted(path) && path.Trim().ToUpperInvariant()!=":MEMORY:")
                        {
                            var asmbl= Assembly.GetCallingAssembly() ?? Assembly.GetEntryAssembly();
                            if(asmbl!=null)
                            {
                                var epath = Path.GetDirectoryName(asmbl.Location);
                                if(epath !=null)
                                    path = Path.GetFullPath(Path.Combine(epath,path));
                            }
                        }
                        builder.Add("Data Source", path);
                        return builder.ConnectionString;
                    }
                case SqlDbType.Odbc:
                    {
                        builder["Pooling"] = connectionInfo.Pooling;
                        return connectionInfo.DbConnectionString;
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }
    }
}
