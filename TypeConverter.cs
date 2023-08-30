using Loxifi.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Loxifi
{
	static class TypeConverter
	{
		private static readonly Type[] _enumNumerics = new Type[]
		{
			typeof(byte),
			typeof(sbyte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong)
		};

		public static object? ConvertType(PropertyInfo targetProperty, object value)
		{
			if (value is null)
			{
				return Activator.CreateInstance(targetProperty.PropertyType);
			}

            if (value is DBNull)
            {
				return null;
            }

            if (targetProperty.PropertyType == value.GetType())
			{
				return value;
			}

			if (targetProperty.PropertyType.IsEnum)
			{
				if (value is string s)
				{
					return StringAsEnum(s, targetProperty.PropertyType);
				}

				if (_enumNumerics.Contains(value.GetType()))
				{
					return NumericAsEnum(targetProperty.PropertyType, value);
				}
			}

			throw new NotImplementedException($"No conversion method between property type '{targetProperty.PropertyType}' on property '{targetProperty.Name}' and database return type '{value.GetType()}'");
		}

		public static object NumericAsEnum(Type enumType, object value)
		{
			if (!enumType.IsEnum)
			{
				throw new ArgumentException("Provided type is not an enum.", nameof(enumType));
			}

			Type underlyingType = Enum.GetUnderlyingType(enumType);

			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			// Convert the value to the underlying type of the enum
			object convertedValue = Convert.ChangeType(value, underlyingType);

			// Now, convert the value to the enum type
			return Enum.ToObject(enumType, convertedValue);
		}

		private static object StringAsEnum(string value, Type enumType)
		{
			Dictionary<string, object> enumValues = new();

			foreach (Enum e in Enum.GetValues(enumType))
			{
				if (e.GetAttributeOfType<DisplayAttribute>() is DisplayAttribute display)
				{
					enumValues.Add(display.Name, e);
				}
				else
				{
					enumValues.Add(e.ToString(), e);
				}
			}

			if (!enumValues.TryGetValue(value, out object v))
			{
				throw new ArgumentOutOfRangeException($"Enum type '{enumType}' does not contain value for '{value}'");
			}

			return v;
		}
	}
}
