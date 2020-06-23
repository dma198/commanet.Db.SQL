using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace commanet.Db
{
    public class TransactionHelper
    {
        private readonly SQLDBConnection connection;
        private readonly DbTransaction? defaultTransaction;
        public TransactionHelper(SQLDBConnection con, IsolationLevel isolation, bool NoTransaction = false)
        {
            if (con == null || !con.IsConnected || con.SqlConnection == null)
            {
                #pragma warning disable CA1303 // Do not pass literals as localized parameters
                throw new Exception("Can not open transaction on closed connection");
                #pragma warning restore CA1303 // Do not pass literals as localized parameters
            }
            connection = con;
            defaultTransaction = NoTransaction ? null
                                               : con.SqlConnection.BeginTransaction(isolation);
            con.CurrentTransaction = defaultTransaction;
        }
        public int ExecuteNonQuery(string SQL, params KeyValuePair<string, object>[] Parameters)
        {

            try
            {
                if (connection.SqlConnection == null)
                {
                    #pragma warning disable CA1303 // Do not pass literals as localized parameters
                    throw new Exception("Can not execute queryn on closed connection");
                    #pragma warning restore CA1303 // Do not pass literals as localized parameters
                }
                using var cmd = connection.SqlConnection.CreateCommand();
                cmd.Transaction = defaultTransaction;

                #pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                cmd.CommandText = connection.SubstParamSymb(
                                   SQLDBConnection.Pack(SQL)
                                  );
                #pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

                cmd.Parameters.Clear();
                SQLDBConnection.BindParameters(cmd, Parameters);
                var res = cmd.ExecuteNonQuery();
                return res;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in SQL:\n{SQL}\n{ex.Message}");
            }
        }

        public void Commit()
        {
            defaultTransaction?.Commit();
            connection.CurrentTransaction = null;
        }
        public void Rollback()
        {
            defaultTransaction?.Rollback();
            connection.CurrentTransaction = null;
        }
    }

}
