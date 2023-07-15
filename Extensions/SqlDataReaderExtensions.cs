using System.Data.SqlClient;
using System.Reflection;

namespace Loxifi.Extensions
{
    public static class SqlDataReaderExtensions
    {
        public static string[] GetColumns(this SqlDataReader reader)
        {
            // Get column names from reader
            List<string> columns = new();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                columns.Add(reader.GetName(i));
            }

            return columns.ToArray();
        }

        public static T GetComplex<T>(this SqlDataReader reader) where T : new()
        {
            T item = new();

            Type itemType = item.GetType();

            foreach (string column in reader.GetColumns())
            {
                PropertyInfo? property = itemType.GetProperty(column, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (property != null && property.CanWrite)
                {
                    // Handle null database values
                    object value = reader[column];
                    property.SetValue(item, value is DBNull ? null : value, null);
                }
            }

            return item;
        }

        public static T GetPrimitive<T>(this SqlDataReader reader, int column = 0) => (T)Convert.ChangeType(reader[column], typeof(T));

        public static string GetString(this SqlDataReader reader) => reader.GetString(0);
    }
}