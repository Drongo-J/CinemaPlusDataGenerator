using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CinemaDataGenerator.Helpers
{
    public class SqlStatementGenerator
    {
        public static string GenerateInsertStatement<T>(T entity)
        {
            Type entityType = typeof(T);
            PropertyInfo[] properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            StringBuilder columns = new StringBuilder();
            StringBuilder values = new StringBuilder();

            foreach (PropertyInfo prop in properties)
            {
                if (!prop.GetGetMethod().IsVirtual)  // Skip virtual properties
                {
                    if (IsListOrEnumerable(prop.PropertyType))  // Skip properties that are lists
                    {
                        continue;
                    }

                    string propName = prop.Name;
                    object propValue = prop.GetValue(entity);

                    if (propValue != null)
                    {
                        columns.Append(propName + ", ");
                        values.Append(GetFormattedValue(propValue) + ", ");
                    }
                }
            }

            if (columns.Length == 0)  // No non-virtual properties to insert
            {
                throw new InvalidOperationException("No non-virtual properties to insert.");
            }

            string tableName = GetTableName(entityType);

            string columnsString = columns.ToString().TrimEnd(',', ' ');
            string valuesString = values.ToString().TrimEnd(',', ' ');

            string insertStatement = $"INSERT INTO {tableName} ({columnsString})\nVALUES ({valuesString})";

            return insertStatement;
        }

        private static string GetFormattedValue(object value)
        {
            if (value is string)
                return $"'{value.ToString().Replace("'", "''")}'";

            if (value is DateTime dateTime)
                return $"'{dateTime:yyyy-MM-dd HH:mm:ss}'";

            if (value is bool boolValue)
                return boolValue ? "1" : "0";

            return value.ToString();
        }

        private static string GetTableName(Type entityType)
        {
            string className = entityType.Name;
            return className.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? className + "es" : className + "s";
        }

        private static bool IsListOrEnumerable(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);
        }
    }
}