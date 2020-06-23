
namespace commanet.Db
{
    public class SqlDbConnectionInfo
    {
        public string DbUser { get; set; } = "";
        public string DbPassword { get; set; } = "";
        public string DbConnectionString { get; set; } = "";
        public bool Pooling { get; set; } = true;
        public uint ConnectionTimeout { get; set; } = 60; // Supported by ORACLE, SqlServer, PostgreSQL
        public uint ConnectionLifetime { get; set; } = 120; // Supported by ORACLE, SqlServer
        public uint DecrPoolSize { get; set; } = 5; // Supported by ORACLE
        public uint IncrPoolSize { get; set; } = 2; // Supported by ORACLE
        public bool LoadBalancing { get; set; } = false; // Supported by ORACLE
        public bool HAEvents { get; set; } = false; // Supported by ORACLE

    }
}
