using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProFix.Repository
{
   internal static class ChangeType
    {
        internal static object ChangeEntityType(object _value, Type _conversion) {

            var _t = _conversion;

            if (_value == null || _value is DBNull) {
                return null;
            }
            if (_t.IsGenericType && _t.GetGenericTypeDefinition().Equals(typeof(Nullable<>))) {
                _t = Nullable.GetUnderlyingType(_t);
            }
            return Convert.ChangeType(_value, _t);
        }
    }
}
