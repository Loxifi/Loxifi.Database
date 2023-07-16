using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace Loxifi.Extensions
{
    public static partial class SqlConnectionExtensions
    {
        private static readonly SqlGenerator _sqlGenerator = new();

        public static void Update<T>(this SqlConnection connection, T toAdd, int? commandTimeout = null) where T : class
        {
            string parsedQuery = _sqlGenerator.GenerateUpdate(toAdd);
            connection.ExecuteNonQuery(parsedQuery, commandTimeout);
        }

        public static void InsertRange<T>(this SqlConnection connection, IEnumerable<T> toInsert, int batch = 1000, int? commandTimeout = null) where T : class
        {
            StringBuilder queryBuilder = new();

            if (batch <= 0)
            {
                throw new ArgumentException("Batch size must be greater than 0");
            }

            using StatelessCommand statelessCommand = new(connection);

            foreach (IEnumerable<T> thisBatch in toInsert.GroupByCount(batch))
            {
                foreach (T item in thisBatch)
                {
                    string thisInsert = _sqlGenerator.GenerateInsert(item, out _);

                    queryBuilder.AppendLine(thisInsert);
                }

                statelessCommand.ExecuteNonQuery(queryBuilder.ToString(), commandTimeout);

                queryBuilder.Clear();
            }
        }

        public static void Insert<T>(this SqlConnection connection, T toAdd, int? commandTimeout = null) where T : class
        {
            string parsedQuery = _sqlGenerator.GenerateInsert(toAdd, out PropertyInfo keyProperty);

            StatelessCommand statelessCommand = new(connection);

            connection.TryOpen();

            if (keyProperty is not null)
            {
                object newKey = statelessCommand.ExecuteScalar(parsedQuery, commandTimeout);
                keyProperty.SetValue(toAdd, newKey);
            }
            else
            {
                statelessCommand.ExecuteNonQuery(parsedQuery, commandTimeout);
            }
        }

        public static IEnumerable<T> Query<T>(this SqlConnection connection, string query, int? commandTimeout = null) where T : new()
        {
            connection.TryOpen();

            using SqlCommand command = new(query, connection);

            if (commandTimeout.HasValue)
            {
                command.CommandTimeout = commandTimeout.Value;
            }

            using SqlDataReader reader = command.ExecuteReader();

            Type itemType = typeof(T);

            // Get column names from reader
            string[] columns = reader.GetColumns();

            // Generate instances of T for each row
            while (reader.Read())
            {
                if (typeof(T) == typeof(string))
                {
                    yield return (T)(object)reader.GetString();

                }
                else if (itemType.IsPrimitive)
                {
                    yield return reader.GetPrimitive<T>();
                }
                else
                {
                    yield return reader.GetComplex<T>();
                }
            }
        }
    }
}
