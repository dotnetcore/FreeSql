#if NET40
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Reflection
{
    public static class PropertyInfoExtensions
    {
        public static object GetValue(this PropertyInfo property, object obj)
        {
            return property.GetValue(obj, null);
        }
        public static void SetValue(this PropertyInfo property, object obj, object value)
        {
            property.SetValue(obj, value, null);
        }
    }
}
#endif