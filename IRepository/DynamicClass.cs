using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityRepository {

	public class Field {

		public string FieldName;
		public Type FieldType;

		public Field(string v, Type type) {

			this.FieldName = v;
			this.FieldType = type;
		}
	}

	public class DynamicClass : DynamicObject {

    private Dictionary<string, KeyValuePair<Type, object>> _fields;

    public DynamicClass(List<Field> fields) {

        _fields = new Dictionary<string, KeyValuePair<Type, object>>();
        fields.ForEach(x => _fields.Add(x.FieldName, new KeyValuePair<Type, object>(x.FieldType, null)));
    }

    public override bool TrySetMember(SetMemberBinder binder, object value) {

        if (_fields.ContainsKey(binder.Name))
        {
            var type = _fields[binder.Name].Key;
            if (value.GetType() == type)
            {
                _fields[binder.Name] = new KeyValuePair<Type, object>(type, value);
                return true;
            }
            else throw new Exception("Value " + value + " is not of type " + type.Name);
        }
        return false;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object result) {

        result = _fields[binder.Name].Value;
        return true;
    }

	public class trst {

			private void xxxx() {


				var fields = new List<Field>() { 
							 new Field("EmployeeID", typeof(int)),
							 new Field("EmployeeName", typeof(string)),
							 new Field("Designation", typeof(string)) 
							};

				dynamic obj = new DynamicClass(fields);

				//set
				obj.EmployeeID = 123456;
				obj.EmployeeName = "John";
				obj.Designation = "Tech Lead";

				obj.Age = 25;             //Exception: DynamicClass does not contain a definition for 'Age'
				obj.EmployeeName = 777;   //Exception: Value 777 is not of type String

				//get
				Console.WriteLine(obj.EmployeeID);     //123456
				Console.WriteLine(obj.EmployeeName);   //John
				Console.WriteLine(obj.Designation); 

		}
     }
  }
}
