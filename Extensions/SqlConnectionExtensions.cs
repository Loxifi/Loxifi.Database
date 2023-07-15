using System.Data.SqlClient;

namespace Loxifi.Extensions
{
    public static partial class SqlConnectionExtensions
    {
        public static void Execute(this SqlConnection connection, string query, int? commandTimeout = null)
        {
            using StatelessCommand statelessCommand = new(connection);
            statelessCommand.Execute(query, commandTimeout);
        }

        public static int ExecuteNonQuery(this SqlConnection connection, string query, int? commandTimeout = null)
        {
            using StatelessCommand statelessCommand = new(connection);
            return statelessCommand.ExecuteNonQuery(query, commandTimeout);
        }

        public static bool TryClose(this SqlConnection sqlConnection)
        {
            if (sqlConnection.State == System.Data.ConnectionState.Open)
            {
                sqlConnection.Close();
                return true;
            }

            return false;
        }

        public static bool TryOpen(this SqlConnection sqlConnection)
        {
            if (sqlConnection.State != System.Data.ConnectionState.Open)
            {
                sqlConnection.Open();
                return true;
            }

            return false;
        }
    }
}