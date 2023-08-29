using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Reflection;
using System.Reflection.Emit;

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

            Dictionary<string, PropertyInfo> writableProperties = new(StringComparer.OrdinalIgnoreCase);

            foreach(PropertyInfo pi in itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if(!pi.CanWrite)
                {
                    continue;
                }

                if(pi.GetCustomAttribute<ColumnAttribute>() is ColumnAttribute columnAttribute)
                {
                    writableProperties.Add(columnAttribute.Name, pi);
                } else
                {
                    writableProperties.Add(pi.Name, pi);
                }
            }

            foreach (string column in reader.GetColumns())
            {
                if (writableProperties.TryGetValue(column, out PropertyInfo pi))
                {
                    // Handle null database values
                    object value = TypeConverter.ConvertType(pi, reader[column]);
                    
                    pi.SetValue(item, value is DBNull ? null : value, null);
                }
            }

            return item;
        }


        public static T GetPrimitive<T>(this SqlDataReader reader, int column = 0) => (T)Convert.ChangeType(reader[column], typeof(T));

        public static string GetString(this SqlDataReader reader) => reader.GetString(0);
    }
}