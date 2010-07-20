using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
//using System.Data.DLinq;
//using System.Data.DLinq.ProviderBase;
using System.Text;
using System.Reflection;
//using System.Expressions;
//using SqlMetal.Mapping;

namespace SqlMetal {

    internal class Generator {
        internal static bool IsNumeric(Type type) {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GetGenericArguments()[0];
            if (type.IsEnum)
                return false;
            TypeCode tc = Type.GetTypeCode(type);
            switch (tc) {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.Char:
                    return true;
                default:
                    return false;
            }
        }
    }
}