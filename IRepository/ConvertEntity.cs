using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;

namespace ProFix.Repository
{
    public class ConvertEntity {

        public static DataTable ConvertToDataTable<T>(IEnumerable<T> list) {

            Type type = typeof(T);
            var properties = type.GetProperties();
            string className = type.UnderlyingSystemType.Name;
            DataTable dataTable = new DataTable(className);

            foreach (PropertyInfo info in properties) {

                DataColumn col = new DataColumn();
                col.ColumnName = info.Name;

                Type dataType = info.PropertyType;

                if (IsNullable(dataType)) {

                    if (dataType.IsGenericType) {
                        dataType = Nullable.GetUnderlyingType(dataType);
                    }
                }
                else { 
                    col.AllowDBNull = false;
                }
                col.DataType = dataType;
                dataTable.Columns.Add(info.Name, dataType);
            }

            foreach (T entity in list) {

                object[] values = new object[properties.Length];

                for (int i = 0; i < properties.Length; i++) {
                    values[i] = properties[i].GetValue(entity, null);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static bool IsNullable(Type Input)
        {
            if (!Input.IsValueType) return true; // Is a ref-type, such as a class
            if (Nullable.GetUnderlyingType(Input) != null) return true; // Nullable
            return false; // Must be a value-type
        }
    }
}
