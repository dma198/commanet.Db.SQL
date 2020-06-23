using System;
using System.Data;
using Xunit;

using commanet.Db;

namespace commanet.Db.Test
{
    public class TransactionTests
    {
        //private string OracleTestConnection = "Type=ORACLE;User=system;Password=manager;Connection=172.17.0.1:1521:ORCL";
        //private string SQLServerTestConnection = "Type=SqlServer;User=sa;Password=test;Connection=host";
        //private string PostgreSQLTestConnection = "Type=postgresql;User=postgres;Password=test;Connection=host";
        private string SQLiteTestConnection = "Type=SQLite;Connection=:memory:";
        [Fact]
        public void TestTransactionOracle()
        {
            /*
            using (var db = new SQLDBConnection(OracleTestConnection))
            {
                db.Open();
                db.Open();
                db.Transaction((th) =>
                {
                    try { th.ExecuteNonQuery("DROP TABLE test"); } catch (Exception) { };
                });

                db.Transaction((th) => {
                    th.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                    th.ExecuteNonQuery("INSERT INTO test(i) VALUES(1)");
                    var v = db.ReadOneInt("SELECT COUNT(*) FROM test"); // Test Reader executed in transaction context
                    th.ExecuteNonQuery("DROP TABLE test");
                });
                db.Close();
            };*/
        }

        [Fact]
        public void TestTransactionSQLServer()
        {
            /*
            using (var db = new SQLDBConnection(SQLServerTestConnection))
            {
                db.Open();
                db.Open();
                db.Transaction((th) =>
                {
                    try { th.ExecuteNonQuery("DROP TABLE test"); } catch (Exception) { };
                });
                db.Transaction((th) => {
                    th.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                    th.ExecuteNonQuery("INSERT INTO test(i) VALUES(1)");
                    var v = db.ReadOneInt("SELECT COUNT(*) FROM test"); // Test Reader executed in transaction context
                    th.ExecuteNonQuery("DROP TABLE test");
                });
                db.Close();
            };*/
        }

        [Fact]
        public void TestTransactionPostgreSQL()
        {
            /*
            using (var db = new SQLDBConnection(PostgreSQLTestConnection))
            {
                db.Open();
                db.Transaction((th) =>
                {
                    try { th.ExecuteNonQuery("DROP TABLE test"); } catch (Exception) { };
                });
                db.Transaction((th) => {
                    th.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                    th.ExecuteNonQuery("INSERT INTO test(i) VALUES(1)");
                    var v = db.ReadOneInt("SELECT COUNT(*) FROM test"); // Test Reader executed in transaction context
                    th.ExecuteNonQuery("DROP TABLE test");
                });
                db.Close();
            };*/
        }
        [Fact]

        public void TestTransactionSQLite()
        {
            using (var db = new SQLDBConnection(SQLiteTestConnection))
            {
                db.Open();
                db.Transaction((th) => {
                    th.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                    th.ExecuteNonQuery("INSERT INTO test(i) VALUES(1)");
                    var v = db.ReadOneInt("SELECT COUNT(*) FROM test"); // Test Reader executed in transaction context
                    th.ExecuteNonQuery("DROP TABLE test");
                });
                db.Close();
            };
        }

        [Fact]
        public void TestSelectOutOfTransactionOracle()
        {
            /*
            using (var db = new SQLDBConnection(OracleTestConnection))
            {
                db.Open();
                db.Transaction((th) =>
                {
                    try { th.ExecuteNonQuery("DROP TABLE test"); } catch (Exception) { };
                });

                db.Transaction((th) =>
                {
                    th.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                    th.ExecuteNonQuery("INSERT INTO test(i) VALUES(1)");
                });

                db.ExecuteReader("SELECT i FROM test", (rd) =>
                {
                    db.Transaction((th) =>
                    {
                        th.ExecuteNonQuery("UPDATE test SET i=2 WHERE i=1");
                    });
                    return true;
                });

                db.Transaction((th) =>
                {
                    th.ExecuteNonQuery("DROP TABLE test");
                });

                db.Close();
            };*/
        }

        [Fact]
        // Test is normally fails due unsupporting feature by this DBMS Engine
        public void TestSelectOutOfTransactionSQLServer()
        {
            /*
            var hasException = true;
            try
            {
                using (var db = new SQLDBConnection(SQLServerTestConnection))
                {
                    db.Open();
                    db.Transaction((th) =>
                    {
                        try { th.ExecuteNonQuery("DROP TABLE test"); } catch (Exception) { };
                    });

                    db.Transaction((th) =>
                    {
                        th.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                        th.ExecuteNonQuery("INSERT INTO test(i) VALUES(1)");
                    });

                    db.ExecuteReader("SELECT i FROM test", (rd) =>
                    {
                        db.Transaction((th) =>
                        {
                            th.ExecuteNonQuery("UPDATE test SET i=2 WHERE i=1");
                        });
                        return true;
                    });

                    db.Transaction((th) =>
                    {
                        th.ExecuteNonQuery("DROP TABLE test");
                    });

                    db.Close();
                };

                hasException=false;
            }
            catch (Exception) { }
            if (!hasException)
                throw new Exception("Unexpected behavior");*/

        }

        [Fact]
        // Test is normally fails due unsupporting feature by this DBMS Engine
        public void TestSelectOutOfTransactionPostgreSQL()
        {
            /*
            var hasException = true;
            try
            {
                using (var db = new SQLDBConnection(PostgreSQLTestConnection))
                {
                    db.Open();
                    db.Transaction((th) =>
                    {
                        try { th.ExecuteNonQuery("DROP TABLE test"); } catch (Exception) { };
                    });

                    db.Transaction((th) =>
                    {

                        th.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                        th.ExecuteNonQuery("INSERT INTO test(i) VALUES(1)");
                    });

                    db.ExecuteReader("SELECT i FROM test", (rd) =>
                    {
                        db.Transaction((th) =>
                        {
                            th.ExecuteNonQuery("UPDATE test SET i=2 WHERE i=1");
                            //db.ExecuteReader("SELECT i FROM test", (rd) =>
                            //{
                            //    return true;
                            //});
                        });
                        return true;
                    });

                    db.Transaction((th) =>
                    {
                        th.ExecuteNonQuery("DROP TABLE test");
                    });

                    db.Close();
                };
                hasException = false;
            }
            catch (Exception) { }
            if(!hasException)
                throw new Exception("Unexpected behavior");
                */
        }

        [Fact]
        public void TestSelectOutOfTransactionSQLite()
        {
            using (var db = new SQLDBConnection(SQLiteTestConnection))
            {
                db.Open();
                db.Transaction((th) =>
                {
                    try { th.ExecuteNonQuery("DROP TABLE test"); } catch (Exception) { };
                });

                db.Transaction((th) => {

                    th.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                    th.ExecuteNonQuery("INSERT INTO test(i) VALUES(1)");
                });

                db.ExecuteReader("SELECT i FROM test", (rd) => {
                    db.Transaction((th) => {
                        th.ExecuteNonQuery("UPDATE test SET i=2 WHERE i=1");
                        //db.ExecuteReader("SELECT i FROM test", (rd) =>
                        //{
                        //    return true;
                        //});
                    });
                    return true;
                });

                db.Transaction((th) => {
                    th.ExecuteNonQuery("DROP TABLE test");
                });

                db.Close();
            };
        }

        [Fact]
        // Test is normally fails because nested/concurrent transactions does not supported by this DBMS
        public void TestMultiTransactionsOracle()
        {
            /*
            var hasException = true;
            try
            {
                using (var db = new SQLDBConnection(OracleTestConnection))
                {
                    db.Open();
                    db.Open();
                    db.Transaction((th) =>
                    {
                        try { th.ExecuteNonQuery("DROP TABLE test"); } catch (Exception) { };
                    });

                    db.Transaction((th) =>
                    {
                        try { th.ExecuteNonQuery("DROP TABLE test2"); } catch (Exception) { };
                    });

                    db.Transaction((th) =>
                    {
                        th.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                        th.ExecuteNonQuery("INSERT INTO test(i) VALUES(1)");
                        var v = db.ReadOneInt("SELECT COUNT(*) FROM test"); // Test Reader executed in transaction context
                        db.Transaction((th) =>
                        {
                            th.ExecuteNonQuery("CREATE TABLE test2(i INTEGER)");
                            th.ExecuteNonQuery("INSERT INTO test2(i) VALUES(1)");
                            th.ExecuteNonQuery("DROP TABLE test2");
                        });
                        th.ExecuteNonQuery("DROP TABLE test");
                    });
                    db.Close();
                };
                hasException = false;
            }
            catch (Exception) { }
            if (!hasException)
                throw new Exception("Unexpected behavior");
                */
        }

        [Fact]
        // Test is normally fails because nested/concurrent transactions does not supported by this DBMS
        public void TestMultiTransactionsSqlServer()
        {
            /*
            var hasException = true;
            try
            {
                using (var db = new SQLDBConnection(SQLServerTestConnection))
            {
                db.Open();
                db.Open();
                db.Transaction((th) =>
                {
                    try { th.ExecuteNonQuery("DROP TABLE test"); } catch (Exception) { };
                });

                db.Transaction((th) =>
                {
                    try { th.ExecuteNonQuery("DROP TABLE test2"); } catch (Exception) { };
                });

                db.Transaction((th) => {
                    th.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                    th.ExecuteNonQuery("INSERT INTO test(i) VALUES(1)");
                    var v = db.ReadOneInt("SELECT COUNT(*) FROM test"); // Test Reader executed in transaction context
                    db.Transaction((th) =>
                    {
                        th.ExecuteNonQuery("CREATE TABLE test2(i INTEGER)");
                        th.ExecuteNonQuery("INSERT INTO test2(i) VALUES(1)");
                        th.ExecuteNonQuery("DROP TABLE test2");
                    });
                    th.ExecuteNonQuery("DROP TABLE test");
                });
                db.Close();
            };
                hasException = false;
            }
            catch (Exception) { }
            if (!hasException)
                throw new Exception("Unexpected behavior");
                */
        }

        [Fact]
        // Test is normally fails because nested/concurrent transactions does not supported by this DBMS
        public void TestMultiTransactionsPostgreSQL()
        {
            /*
            var hasException = true;
            try
            {
                using (var db = new SQLDBConnection(PostgreSQLTestConnection))
            {
                db.Open();
                db.Open();
                db.Transaction((th) =>
                {
                    try { th.ExecuteNonQuery("DROP TABLE test"); } catch (Exception) { };
                });

                db.Transaction((th) =>
                {
                    try { th.ExecuteNonQuery("DROP TABLE test2"); } catch (Exception) { };
                });

                db.Transaction((th) => {
                    th.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                    th.ExecuteNonQuery("INSERT INTO test(i) VALUES(1)");
                    var v = db.ReadOneInt("SELECT COUNT(*) FROM test"); // Test Reader executed in transaction context
                    db.Transaction((th) =>
                    {
                        th.ExecuteNonQuery("CREATE TABLE test2(i INTEGER)");
                        th.ExecuteNonQuery("INSERT INTO test2(i) VALUES(1)");
                        th.ExecuteNonQuery("DROP TABLE test2");
                    });
                    th.ExecuteNonQuery("DROP TABLE test");
                });
                db.Close();
            };
            }
            catch (Exception) { }
            if (!hasException)
                throw new Exception("Unexpected behavior");
                */
        }

        [Fact]
        public void TestMultiTransactionsSQLite()
        {
            using (var db = new SQLDBConnection(SQLiteTestConnection))
            {
                db.Open();
                db.Open();
                db.Transaction((th) =>
                {
                    try { th.ExecuteNonQuery("DROP TABLE test"); } catch (Exception) { };
                });

                db.Transaction((th) =>
                {
                    try { th.ExecuteNonQuery("DROP TABLE test2"); } catch (Exception) { };
                });

                db.Transaction((th) => {
                    th.ExecuteNonQuery("CREATE TABLE test(i INTEGER)");
                    th.ExecuteNonQuery("INSERT INTO test(i) VALUES(1)");
                    var v = db.ReadOneInt("SELECT COUNT(*) FROM test"); // Test Reader executed in transaction context
                    db.Transaction((th) =>
                    {
                        th.ExecuteNonQuery("CREATE TABLE test2(i INTEGER)");
                        th.ExecuteNonQuery("INSERT INTO test2(i) VALUES(1)");
                        th.ExecuteNonQuery("DROP TABLE test2");
                    });
                    th.ExecuteNonQuery("DROP TABLE test");
                });
                db.Close();
            };
        }


    }
}
