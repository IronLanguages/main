using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SqlMetal {
    class SqlProviderType {
        SqlDbType sqlDbType;

        internal SqlDbType SqlDbType {
            get {
                return this.sqlDbType;
            }
        }

        internal static SqlProviderType Create(SqlDbType type) {
#if UNUSED
            switch (type) {
                case SqlDbType.BigInt:
                    return SqlProviderType.theBigInt;

                case SqlDbType.Bit:
                    return SqlProviderType.theBit;

                case SqlDbType.Char:
                    return SqlProviderType.theChar;

                case SqlDbType.DateTime:
                    return SqlProviderType.theDateTime;

                case SqlDbType.Decimal:
                    return SqlProviderType.theDefaultDecimal;

                case SqlDbType.Float:
                    return SqlProviderType.theFloat;

                case SqlDbType.Int:
                    return SqlProviderType.theInt;

                case SqlDbType.Money:
                    return SqlProviderType.theMoney;

                case SqlDbType.Real:
                    return SqlProviderType.theReal;

                case SqlDbType.UniqueIdentifier:
                    return SqlProviderType.theUniqueIdentifier;

                case SqlDbType.SmallDateTime:
                    return SqlProviderType.theSmallDateTime;

                case SqlDbType.SmallInt:
                    return SqlProviderType.theSmallInt;

                case SqlDbType.SmallMoney:
                    return SqlProviderType.theSmallMoney;

                case SqlDbType.Timestamp:
                    return SqlProviderType.theTimestamp;

                case SqlDbType.TinyInt:
                    return SqlProviderType.theTinyInt;
            }
#endif
            return new SqlProviderType(type);
        }

        private SqlProviderType(SqlDbType type) {
            this.sqlDbType = type;
        }

        internal static SqlProviderType Parse(string stype) {
            string text4 = null;
            string text5 = null;
            string text6 = null;
            int num7 = stype.IndexOf('(');
            int num8 = stype.IndexOf(' ');
            int num9 = ((num7 != -1) && (num8 != -1)) ? Math.Min(num8, num7) : ((num7 != -1) ? num7 : ((num8 != -1) ? num8 : -1));
            if (num9 == -1) {
                text4 = stype;
                num9 = stype.Length;
            }
            else {
                text4 = stype.Substring(0, num9);
            }
            int num10 = num9;
            if ((num10 < stype.Length) && (stype[num10] == '(')) {
                num10++;
                num9 = stype.IndexOf(',', num10);
                if (num9 > 0) {
                    text5 = stype.Substring(num10, num9 - num10);
                    num10 = num9 + 1;
                    num9 = stype.IndexOf(')', num10);
                    text6 = stype.Substring(num10, num9 - num10);
                }
                else {
                    num9 = stype.IndexOf(')', num10);
                    text5 = stype.Substring(num10, num9 - num10);
                }
                num10 = num9++;
            }
            if (string.Compare(text4, "rowversion", StringComparison.OrdinalIgnoreCase) == 0) {
                text4 = "Timestamp";
            }
            if (string.Compare(text4, "numeric", StringComparison.OrdinalIgnoreCase) == 0) {
                text4 = "Decimal";
            }
            if (string.Compare(text4, "sql_variant", StringComparison.OrdinalIgnoreCase) == 0) {
                text4 = "Variant";
            }
            SqlDbType type2 = (SqlDbType)Enum.Parse(typeof(SqlDbType), text4, true);
            int num11 = (text5 != null) ? int.Parse(text5) : 0;
            int num12 = (text6 != null) ? int.Parse(text6) : 0;
            switch (type2) {
                case SqlDbType.Binary:
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NVarChar:
                case SqlDbType.VarBinary:
                case SqlDbType.VarChar:
                    return SqlProviderType.Create(type2);
                //return SqlProviderType.Create(type2, num11);

                case SqlDbType.Decimal:
                case SqlDbType.Float:
                case SqlDbType.Real:
                    return SqlProviderType.Create(type2);
                //return SqlProviderType.Create(type2, num11, num12);
            }
            return SqlProviderType.Create(type2);
        }

        internal Type GetClosestRuntimeType() {
            return SqlProviderType.GetClosestRuntimeType(this.sqlDbType);
        }

        internal static Type GetClosestRuntimeType(SqlDbType sqlDbType) {
            switch (sqlDbType) {
                case SqlDbType.BigInt:
                    return typeof(long);

                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.Timestamp:
                case SqlDbType.VarBinary:
                    return typeof(byte[]);

                case SqlDbType.Bit:
                    return typeof(bool);

                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                case SqlDbType.Xml:
                    return typeof(string);

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                    return typeof(DateTime);

                case SqlDbType.Decimal:
                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    return typeof(decimal);

                case SqlDbType.Float:
                    return typeof(double);

                case SqlDbType.Int:
                    return typeof(int);

                case SqlDbType.Real:
                    return typeof(float);

                case SqlDbType.UniqueIdentifier:
                    return typeof(Guid);

                case SqlDbType.SmallInt:
                    return typeof(short);

                case SqlDbType.TinyInt:
                    return typeof(byte);

                case SqlDbType.Udt:
                    throw new Exception("Udt type is not handled.");
            }
            return typeof(object);
        }


    }
}
