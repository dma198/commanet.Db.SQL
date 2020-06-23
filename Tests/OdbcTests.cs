using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

using commanet.Db;

namespace commanet.Db.Test
{
    public class OdbcTests
    {
        [Fact]
        public void TestOdbcParameters()
        {
            /*
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
            var fname = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "..", "..", "TestData", "MWCONFIG.MDB"));

            using (var db = new SQLDBConnection("odbc", "", "", @"Driver={Microsoft Access Driver (*.mdb, *.accdb)};DBQ=" + fname))
            {
                db.Open();
                // Notice that ODBC does not supports named parameters. Name passed in DbParam.New is ignored - position will be used
                // for parameters binding 
                var cnt =db.ReadOneInt("SELECT COUNT(*) FROM dpi_messages WHERE message_id=:ID",DbParam.New("ID","CCM_GEN_STATUS"));
                db.Close();
                if (cnt != 1) throw new Exception("ODBC query with parameter does not returns data");
            };*/
        }
    }
}
