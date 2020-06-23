
namespace commanet.Db
{
    public class SqlDbTypeInfo
    {
        public SqlDbType Type { get; set; } = SqlDbType.Sqlite;
        public string Name {get => Type.ToString();}
        public string ADOProviderAssemblyName { get; set; } = "";
        public string ADOProviderFactoryName { get; set; } = "";
        public string PARAMSYMBOL {get; set;} = "?";
        public bool Installed {get; set;} = false; 
    }
}
