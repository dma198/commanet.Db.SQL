using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

using commanet.Db;

namespace commanet.Db.Test
{
    public class DbTests
    {
        //private string OracleTestConnection = "Type=ORACLE;User=system;Password=manager;Connection=172.17.0.1:1521:ORCL";
        //private string SQLServerTestConnection = "Type=SqlServer;User=sa;Password=test;Connection=host";
        //private string PostgreSQLTestConnection = "Type=postgresql;User=postgres;Password=test;Connection=host";
        private string SQLiteTestConnection = "Type=SQLite;Connection=:memory:";

        [Fact]
        public void TestExecuteNonQuery()
        {
            using (var db = new SQLDBConnection("sqlite", "", "", ":memory:"))
            {
                db.Open();
                db.Transaction((t) =>
                {
                    try{t.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");}catch(Exception){}
                    t.ExecuteNonQuery("INSERT INTO test VALUES(:V)", DbParam.New("V", 1));
                });
                db.Close();
            };
        }

        [Fact]
        public void TestExecuteReader()
        {
            using (var db = new SQLDBConnection("sqlite", "", "", ":memory:"))
            {
                db.Open();
                try
                {
                    db.Transaction((t) =>
                    {
                        t.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                        t.ExecuteNonQuery("INSERT INTO test VALUES(:V)", DbParam.New("V", 1));
                    });
                }
                catch (Exception) { }
                var i = 0;
                db.ExecuteReader(@"SELECT * FROM test
                                   WHERE i=:V",
                                   (rd)=> { 
                                       i++; 
                                       return true; 
                                   }, DbParam.New("V", 1));
                if (i != 1) throw new Exception("Data read not successfull");
                db.Close();
            };
        }

        [Fact]
        public void TestReadSingleRow()
        {
            using (var db = new SQLDBConnection("sqlite", "", "", ":memory:"))
            {
                db.Transaction((t) =>
                {
                    try{t.ExecuteNonQuery("DROP TABLE test");}catch(Exception){}
                    t.ExecuteNonQuery("CREATE TABLE test(i INTEGER,d REAL, dt DATETIME)");
                    t.ExecuteNonQuery("INSERT INTO test VALUES(:I,:F,:DT)", 
                        DbParam.New("I", 1), DbParam.New("F", 123.456),DbParam.New("DT", DateTime.Now));
                });
                var v = db.ReadVector<long>("SELECT i FROM test");
                if(v[0]!=1) throw new Exception("Data read not successfull");
                var d = db.ReadOneDouble("SELECT d FROM test");
                if (d != 123.456) throw new Exception("Data read not successfull");
                var dt = db.ReadOneDateTime("SELECT dt FROM test");
                if (dt == null) throw new Exception("Data read not successfull");
            };
        }

        [Fact]
        public void TestDBException()
        {
            var s = "";
            using (var db = new SQLDBConnection("sqlite", "", "", ":memory:"))
            {
                db.Open();
                try
                {
                    var SQL = "SELECT * FROM NotExistedTable";
                    db.ExecuteReader(SQL, (rd) =>
                    {
                        return true;
                    });
                }catch(Exception ex)
                {
                    s = ex.Message;
                }
                db.Close();
            };
            if (s == "") throw new Exception("Exception was not generated in ExecuteReader");
        }

        [Fact]
        public void TestNestedReaderOracle()
        {
            /*
            using (var db = new SQLDBConnection(OracleTestConnection))
            {
                db.Open();
                try
                {
                    db.Transaction((t) =>
                    {
                        try { t.ExecuteNonQuery("DROP TABLE test"); } catch (Exception) { }
                    });

                    db.Transaction((t) =>
                    {
                        t.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                        t.ExecuteNonQuery("INSERT INTO test VALUES(:V)", DbParam.New("V", 1));
                    });
                }
                catch (Exception) { }
                var i = 0;
                db.ExecuteReader("SELECT * FROM test WHERE i=:V", (rd) => {
                    db.ExecuteReader("SELECT * FROM test", (rd) =>
                    {
                        return true;
                    });
                    i++; 
                    return true; 
                }, DbParam.New("V", 1));
                if (i != 1) throw new Exception("Data read not successfull");
            };
            */
        }

        [Fact]
        public void TestNestedReaderSqlServer()
        {
            /*
            using (var db = new SQLDBConnection(SQLServerTestConnection))
            {
                db.Open();
                try
                {
                    db.Transaction((t) =>
                    {
                        try { t.ExecuteNonQuery("DROP TABLE test"); } catch (Exception) { }
                    });
                    db.Transaction((t) =>
                    {
                        t.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                        t.ExecuteNonQuery("INSERT INTO test VALUES(:V)", DbParam.New("V", 1));
                    });
                }
                catch (Exception) { }
                var i = 0;
                db.ExecuteReader("SELECT * FROM test WHERE i=:V", (rd) => {
                    db.ExecuteReader("SELECT * FROM test", (rd) =>
                    {
                        return true;
                    });
                    i++;
                    return true;
                }, DbParam.New("V", 1));
                if (i != 1) throw new Exception("Data read not successfull");
            };
            */
        }

        [Fact]
        // This test normally fails because feature does not supported by PostgreSQL  
        public void TestNestedReaderPostgreSQL()
        {            
            /*
            var hasException = true;
            try
            {
                using (var db = new SQLDBConnection(PostgreSQLTestConnection))
                {
                    db.Open();
                    try
                    {
                        db.Transaction((t) =>
                        {
                            try { t.ExecuteNonQuery("DROP TABLE test"); } catch (Exception) { }
                        });
                        db.Transaction((t) =>
                        {
                            t.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                            t.ExecuteNonQuery("INSERT INTO test VALUES(:V)", DbParam.New("V", 1));
                        });
                    }
                    catch (Exception) { }
                    var i = 0;
                    db.ExecuteReader("SELECT * FROM test WHERE i=:V", (rd) =>
                    {
                        db.ExecuteReader("SELECT * FROM test", (rd) =>
                        {
                            return true;
                        });
                        i++;
                        return true;
                    }, DbParam.New("V", 1));
                    if (i != 1) throw new Exception("Data read not successfull");
                };
                hasException = false;
            }
            catch (Exception) { }
            if (!hasException)
                throw new Exception("Unexpected behaviour");

                */
        }

        [Fact]
        public void TestNestedReaderSQLite()
        {
            using (var db = new SQLDBConnection(SQLiteTestConnection))
            {
                db.Open();
                try
                {
                    db.Transaction((t) =>
                    {
                        try { t.ExecuteNonQuery("DROP TABLE test"); } catch (Exception) { }
                    });
                    db.Transaction((t) =>
                    {
                        t.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                        t.ExecuteNonQuery("INSERT INTO test VALUES(:V)", DbParam.New("V", 1));
                    });
                }
                catch (Exception) { }
                var i = 0;
                db.ExecuteReader("SELECT * FROM test WHERE i=:V", (rd) => {
                    db.ExecuteReader("SELECT * FROM test", (rd) =>
                    {
                        return true;
                    });
                    i++;
                    return true;
                }, DbParam.New("V", 1));
                if (i != 1) throw new Exception("Data read not successfull");
            };
        }


    }
}
