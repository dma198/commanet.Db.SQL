using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

using commanet.Db;

namespace commanet.Db.Test
{
    public class TestConnections
    {
        //private string OracleTestConnection = "Type=ORACLE;User=system;Password=manager;Connection=172.17.0.1:1521:ORCL";
        //private string SQLServerTestConnection = "Type=SqlServer;User=sa;Password=test;Connection=host";
        //private string PostgreSQLTestConnection = "Type=postgresql;User=postgres;Password=test;Connection=host";
        private string SQLiteTestConnection = "Type=SQLite;Connection=:memory:";

        [Fact]
        public void TestOracleConnection()
        {
            /*
            using (var db = new SQLDBConnection(OracleTestConnection))
            {
                db.Open();
                db.Close();
            };
            */
        }
        [Fact]
        public void TestSqlServerConnection()
        {
            /*
            using (var db = new SQLDBConnection(SQLServerTestConnection))
            {
                db.Open();
                db.Close();
            };
            */
        }
        [Fact]
        public void TestSqliteConnection()
        {
            using (var db = new SQLDBConnection(SQLiteTestConnection))
            {
                db.Open();
                db.Close();
            };
        }
        [Fact]
        public void TestPostgreSQLConnection()
        {
            /*
            using (var db = new SQLDBConnection(PostgreSQLTestConnection))
            {
                db.Open();
                db.Close();
            };
            */
        }

        [Fact]
        public void TestOdbcConnection()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
            var fname=Path.GetFullPath(Path.Combine(location,"..","..","..","TestData","MWCONFIG.MDB"));

            using (var db = new SQLDBConnection("odbc", "", "", @"Driver={Microsoft Access Driver (*.mdb, *.accdb)};DBQ="+fname))
            {
                //db.Open();
                //db.Close();
            };
        }

    }
}
