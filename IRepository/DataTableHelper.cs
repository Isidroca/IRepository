﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace ProFix.Repository
{
   public static class DataTableHelper
    {
      public static List<T> DataTableToList<T>(this DataTable table) where T : class, new() {

        try {
            List<T> _list = new List<T>();

            foreach (var _row in table.AsEnumerable()) {
                T _obj = new T();

                foreach (var _prop in _obj.GetType().GetProperties()) {
                    try {
                    
                        PropertyInfo propertyInfo = _obj.GetType().GetProperty(_prop.Name);
                        propertyInfo.SetValue(_obj, ChangeType.ChangeEntityType(_row[_prop.Name], propertyInfo.PropertyType), null);
                    }
                    catch {
                        continue;
                    }
                }
                _list.Add(_obj);
            }
            return _list;
        }
        catch {
            return null;
        }
    }
  }
}
