using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EntityRepository {
    public	class Reflection {

	static void WriteColumnMappings<T>(T item) where T : new() {
			// Just grabbing this to get hold of the type name:
			var type = item.GetType();
  
			// Get the PropertyInfo object:
			var properties = item.GetType().GetProperties();

			foreach(var property in properties) {

				var attributes = property.GetCustomAttributes(false);
				var columnMapping = attributes.FirstOrDefault(a => a.GetType() == typeof(ColumnNameAttribute));

				if(columnMapping != null) {
					var mapsto = columnMapping as ColumnNameAttribute;
					//Console.WriteLine(msg, property.Name, mapsto.Name);
				}
			}
      }

	 static void WritePK<T>(T item) where T : new() {

		var type = item.GetType();
		var properties = type.GetProperties();

		// This replaces all the iteration above:
		var property = properties
				.FirstOrDefault(p => p.GetCustomAttributes(false)
					.Any(a => a.GetType() == typeof(PrimaryKeyAttribute)));

		if (property != null) {

				//Console.WriteLine(msg, type.Name, property.Name);
			}
        }

	  public List<string> GetColumnsNames<T>(T Entity) {

			var list = new List<string>();

			PropertyInfo[] propertyInfos;
			propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Static);
			// sort properties by name
			Array.Sort(propertyInfos, delegate(PropertyInfo propertyInfo1, PropertyInfo propertyInfo2) {
				       return propertyInfo1.Name.CompareTo(propertyInfo2.Name);
			});

			// write property names
			foreach (PropertyInfo propertyInfo in propertyInfos) {
			         list.Add(propertyInfo.Name);
			}

			return list;
		}
	}
}
