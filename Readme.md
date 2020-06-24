## commanet.Db
--------------------------
Provides interface with SQL Databases. ADO.Net based.

Opening Connection:
```c#

// 1. With minimum parameters
var db = new SQLDBConnection(DbType, DbUser, DbPassword, DbConStr);

// 2. With advanced parameters
var db = SQLDBConnection(DbTypeew SqlDbConnectionInfo()
            {
                DbUser = dbUser,
                DbPassword = dbPassword,
                DbConnectionString = dbConnection,
                Pooling  = true,
                ConnectionTimeout = 60,   // Parameter supported by ORACLE, SqlServer, PostgreSQL
                ConnectionLifetime = 120, // Parameter supported by ORACLE, SqlServer
                DecrPoolSize = 5;         // Parameter supported by ORACLE
                IncrPoolSize = 2;         // Parameter supported by ORACLE
                LoadBalancing = false;    // Supported by ORACLE (Used for ORACLE custer)
                HAEvents = false;         // Supported by ORACLE (Used for ORACLE custer)
            });

// 3. With single connection string
var db = new SQLDBConnection(FullConnectionString);

```
**DbType**      : String value, one of: *ORACLE*,*SqlServer*,*PostgreSQL*,*MySQL*,*SQLite*
**DbConStr**    : Connection string. Format depends of DBMS Used

**FullConnectionString**    : Contains *';'* separated key-value pairs in format *<key>=<value>*. Variable names (not case sensitive) are "Type", "User", "Password","Connection" and other matching with property names in SqlDbConnectionInfo type presented above.

Example: *"Type=ORACLE;User=system;Password=manager;connection=localhost:orcl;Pooling=False"*



To use particular database type will need to add to project dependencies related package.

DB Type    | Required package              | Connection String Format
-----------|-------------------------------|---------------------------------------------
ORACLE     |Oracle.ManagedDataAccess.Core  |host[ [:service_name]&nbsp;\|&nbsp;[:port:service_name] ]<br>Examples:<br>- *localhost:MYDB*<br>- *localhost:1521:LEVEL2*        
SQLServer  |System.Data.SqlClient          |host[:database_name]<br>Examples:<br>- *localhost*<br>- *localhost:LEVEL2*
PostgreSQL |Npgsql                         |host[ [:database_name]&nbsp;\|&nbsp[:port:database_name] ]<br>Examples:<br>- *localhost*<br>- *localhost:MYDB* 
MySQL      |MySql.Data                     |host[ [:database_name]&nbsp;\|&nbsp[:port:database_name] ]<br>Examples:<br>- *localhost*<br>- *localhost:MYDB* 
SQLite     |System.Data.SQLite.Core        |[filename]&nbsp;\|&nbsp;:memory:<br>Examples:<br>- *C:\MyDB.db3*<br>- *:memory:*
ODBC       |System.Data.Odbc               |[ODBC Connection String]<br>Examples:<br>- *Driver={Microsoft Access Driver (\*.mdb, \*.accdb)};DBQ=MyDb.mdb*<br>- *Driver={Microsoft Excel Driver (\*.xls)};DBQ=book1.xls*   

### Notes About ODBC
You should check that driver you use is installed in your OS. It must match with your application architecture:
x86 or x64. In Windows can check it in **ODBC Datasource Administrator**: Control Panel / Administrative Tools / Odbc Data Sources (32-bit/64-bit).
Driver name passed in connection string must exactly match with one showed in **ODBC Datasource Administrator**.
If required driver is missing then it must be installed. For example, Windows10 usually does not have 64-bit driver
for Microsoft Access. You have to install **AccessDatabaseEngine_X64.exe**. It can be downloaded for free from Microsoft's web site.  

Another specific of ODBC is that it does not support named parameters. By this reason all parameters passed will be 
identified by postion only. Parameter names are ignored.


### Parameters in SQL Operators
Regardless to parameter name agreement used for different databases names used in SQL queries should use ORACLE notation
with semicolon prefix:
```sql
 SELECT * FROM mytable WHERE id=:MYPARAMETER
```
Names will be automatically converted to specific database format if it needs.


### Read Data from Database

Read multiple rows: 
```c#
 var db = new SQLDBConnection(DbType, DbUser, DbPassword, DbConStr);
 var SQL = "SELECT value FROM mytable";
 db.ExecuteReader(SQL,(rd)=>{
     var value = rd.GetString(0);
     .....
     return true; // true  - continue read next row 
                  // false - stop reading   
 });
```
Read single value:

```c#
 var db = new SQLDBConnection(DbType, DbUser, DbPassword, DbConStr);
 var SQL = "SELECT int_value FROM mytable";
 int? value = db.ReadOneInt(SQL);
 //There are many versions of method ReadOneXXX: ReadOneString, ReadOneDouble, ReadOneDateTime...
 ...   
```


### Using SQL Parameters
Parameters accepted as last optional arguments in methods ExecuteReader, ReadOneXXX and others.

```c#
 var db = new SQLDBConnection(DbType, DbUser, DbPassword, DbConStr);
 var SQL = "SELECT int_value FROM mytable WHERE key=:K";
 int? value = db.ReadOneInt(SQL,
                            DbParam.New("K",1) // <= DbParam is helper class to create  
              );                               //    SQL Parameters
 ...   
```

### Data Manipulation (DML)
All data changes can be performed only inside transaction.

Transaction wrapped in *Transaction* method where as parameter passed an *Action* 
(lambda or other class method) with signature:

```c#

void MyTransactionMethod( TransactionHelper ac, IsolationLevel isolation = IsolationLevel.ReadCommitted);
{
  // TO-DO: implement DML operations
}
```

Example:

```c#
 var db = new SQLDBConnection(DbType, DbUser, DbPassword, DbConStr);
 
 db.Transaction((th)=>{ // th is instance of transaction helper class provides methods for execute DML or DDL operations
    var SQL = "INSERT INTO mytable (fieldq) VALUES(:V)";
    th.ExecuteNonQuery(SQL,DbParam.New("V","Hello World"));
 }); // Transaction will be automatically COMMITed after finishing passed action
     // In case of unmanaged exception happens - ROLLBACK will be called
```

Full *Transaction* method signature:

```C#
public void Transaction(Action<TransactionHelper> ac, IsolationLevel isolation = IsolationLevel.ReadCommitted, bool NoTransaction = false)
```

Last parameter *NoTransaction*, if it is *true* then transaction will not be open.
Transaction helper instance will just provide methods to execute DB writing operations (DDL, DML)
which will works out of any transaction.
It can be useful for some DBMS engines not supported DDL operations in transactions.
Another reason is to improve performance in case if consistency is not important.

 