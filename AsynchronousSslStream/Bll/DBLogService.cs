using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using System.Data.SqlClient;
using System.Data;
using log4net;


namespace Trader.Server.Bll
{
    public static class DBLogService
    {
        private static ILog _Log = LogManager.GetLogger(typeof(DBLogService));
        public static bool SaveLog(Token token, string msg, Guid? transactionId = null)
        {
            bool isSucced = false;
            try
            {
                SqlConnection sqlConnection = new SqlConnection(SettingManager.Default.ConnectionString);

                SqlCommand sqlCommand;
                SqlParameter sqlParameter;

                sqlCommand = new SqlCommand(((token.UserID.Equals(Guid.Empty)) ? "dbo.P_SaveLogForLoginFail" : "dbo.P_SaveLog"), sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                if (token.UserID.Equals(Guid.Empty))
                {
                    sqlParameter = sqlCommand.Parameters.Add("@LoginName", SqlDbType.VarChar, 20);
                    sqlParameter.Value = string.Empty;
                }
                else
                {
                    sqlParameter = sqlCommand.Parameters.Add("@UserID", SqlDbType.UniqueIdentifier);
                    sqlParameter.Value = token.UserID;
                }

                sqlParameter = sqlCommand.Parameters.Add("@IP", SqlDbType.NVarChar, 15);
                sqlParameter.Value = string.Empty;
                sqlParameter = sqlCommand.Parameters.Add("@Role", SqlDbType.NVarChar, 30);
                //sqlParameter.Value = isEmployee ? "Employee" : role;
                sqlParameter.Value = UserType.Customer.ToString();
                sqlParameter = sqlCommand.Parameters.Add("@ObjectIDs", SqlDbType.NVarChar, 4000);
                sqlParameter.Value = token.AppType.ToString();

                sqlParameter = sqlCommand.Parameters.Add("@Timestamp", SqlDbType.DateTime);
                sqlParameter.Value = DateTime.Now;
                sqlParameter = sqlCommand.Parameters.Add("@Event", SqlDbType.NVarChar, 4000);
                sqlParameter.Value = msg;
                if (!token.UserID.Equals(Guid.Empty))
                {
                    sqlParameter = sqlCommand.Parameters.Add("@TransactionID", SqlDbType.UniqueIdentifier);
                    if (transactionId==null)
                    {
                        sqlParameter.Value = DBNull.Value;
                    }
                    else
                    {
                        sqlParameter.Value = transactionId.Value;
                    }
                }

                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
                isSucced = true;
            }
            catch (System.Exception ex)
            {
                isSucced = false;
                _Log.Error("save log to db", ex);
            }
            return isSucced;
        }
    }
}
