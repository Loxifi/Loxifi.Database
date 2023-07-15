using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace Loxifi
{
    public class SqlGenerator
    {
        public string? FormatArgument(object? o)
        {
            if (o is null)
            {
                return "null";
            }

            if (o is string s)
            {
                return $"N'{s.Replace("'", "''")}'";
            }

            if (o is DateTime dt)
            {
                return $"'{dt:yyyy-MM-dd HH:mm:ss}'";
            }

            if (o is bool b)
            {
                return b ? "1" : "0";
            }

            if (o is Enum e)
            {
                return ((int)(object)e).ToString();
            }

            if (o is byte[] ba)
            {
                StringBuilder hex = new(ba.Length * 2 + 2);
                hex.Append("0x");
                foreach (byte bv in ba)
                {
                    hex.AppendFormat("{0:x2}", bv);
                }

                return hex.ToString();
            }

            return o.ToString();
        }

        public string GenerateInsert<T>(T toInsert, out bool hasKey) where T : class
        {
            if (toInsert is null)
            {
                throw new ArgumentNullException(nameof(toInsert));
            }

            Type objectType = toInsert.GetType();

            StringBuilder stringBuilder = new();

            stringBuilder.Append($"INSERT INTO [dbo].[{objectType.Name}] (");

            stringBuilder.Append(this.JoinedPropertyNameList(objectType, true));

            stringBuilder.Append(')');

            if (hasKey = this.TryGetKey(objectType, out PropertyInfo keyProperty))
            {
                stringBuilder.Append($" output INSERTED.{keyProperty.Name} ");
            }

            stringBuilder.Append(" VALUES (");

            stringBuilder.Append(this.JoinedPropertyValueList(objectType, toInsert, true));

            stringBuilder.Append(')');

            return stringBuilder.ToString();
        }

        public string GenerateUpdate<T>(T toInsert) where T : class
        {
            if (toInsert is null)
            {
                throw new ArgumentNullException(nameof(toInsert));
            }

            Type objectType = toInsert.GetType();

            if (!this.TryGetKey(objectType, out PropertyInfo keyProperty))
            {
                throw new ArgumentException($"Can not update entity of type {objectType} with no Key");
            }

            long keyValue = (long)keyProperty.GetValue(toInsert);

            StringBuilder stringBuilder = new();

            stringBuilder.Append($"UPDATE [dbo].[{objectType.Name}] SET ");

            List<string> propertyNames = this.PropertyNameList(objectType, true).ToList();
            List<string> propertyValues = this.PropertyValueList(objectType, toInsert, true).ToList();

            for (int i = 0; i < propertyNames.Count; i++)
            {
                if (i != 0)
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append($"{propertyNames[i]} = {propertyValues[i]}");
            }

            stringBuilder.Append($" WHERE [{keyProperty.Name}] = {keyValue}");

            return stringBuilder.ToString();
        }

        private IEnumerable<PropertyInfo> GetMappedProperties(Type type, bool skipKey)
        {
            PropertyInfo key = null;

            if (skipKey)
            {
                _ = this.TryGetKey(type, out key);
            }

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property == key)
                {
                    continue;
                }

                if (property.GetGetMethod() is null)
                {
                    continue;
                }

                if (property.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue;
                }

                yield return property;
            }
        }

        private string JoinedPropertyNameList(Type type, bool skipKey) => string.Join(", ", this.PropertyNameList(type, skipKey));

        private IEnumerable<string> PropertyNameList(Type type, bool skipKey)
        {
            PropertyInfo[] properties = this.GetMappedProperties(type, skipKey).ToArray();

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo pi = properties[i];

                yield return $"[{pi.Name}]";
            }
        }

        private string JoinedPropertyValueList(Type type, object instance, bool skipKey) => string.Join(", ", this.PropertyValueList(type, instance, skipKey));

        private IEnumerable<string> PropertyValueList(Type type, object instance, bool skipKey)
        {
            PropertyInfo[] properties = this.GetMappedProperties(type, skipKey).ToArray();

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo pi = properties[i];

                yield return this.FormatArgument(pi.GetValue(instance));
            }
        }

        private bool TryGetKey(Type type, out PropertyInfo keyProperty)
        {
            PropertyInfo[] properties = type.GetProperties().Where(p => p.GetGetMethod() != null).ToArray();

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo pi = properties[i];

                if (pi.GetCustomAttribute<KeyAttribute>() != null)
                {
                    keyProperty = pi;
                    return true;
                }
            }

            keyProperty = null;
            return false;
        }
    }
}