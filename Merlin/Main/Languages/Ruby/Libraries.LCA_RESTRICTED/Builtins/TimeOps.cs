/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using System.Globalization;

namespace IronRuby.Builtins {

    [RubyClass("Time", Extends = typeof(DateTime), Inherits = typeof(Object)), Includes(typeof(Comparable))]
    public static class TimeOps {
        readonly static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); //January 1, 1970 00:00 UTC

        [RubyConstructor]
        public static DateTime Create(RubyClass/*!*/ self) {
            return DateTime.Now;
        }

        // TODO: I removed all of the constructor overloads since Ruby doesn't define any non-default constructors for Time.
        // In the future, however, we need to fix this problem per RubyForge bug #20035

        #region "Singleton Methods"

        #region at 

        private static long microsecondsToTicks(long microseconds) {
            return microseconds * 10;
        }

        private static long secondsToTicks(long seconds) {
            return seconds * 10000000;
        }

        [RubyMethod("at", RubyMethodAttributes.PublicSingleton)]
        public static DateTime Create(object/*!*/ self, DateTime other) {
            return new DateTime(other.Ticks, other.Kind);
        }

        [RubyMethod("at", RubyMethodAttributes.PublicSingleton)]
        public static DateTime Create(object/*!*/ self, double seconds) {
            return epoch.ToLocalTime().AddSeconds(seconds);
        }
        

        [RubyMethod("at", RubyMethodAttributes.PublicSingleton)]
        public static DateTime Create(object/*!*/ self, long seconds, long microseconds) {
            long ticks = epoch.ToLocalTime().Ticks + secondsToTicks(seconds) + microsecondsToTicks(microseconds);
            return new DateTime(ticks);
        }

        #endregion

        [RubyMethod("now", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateTime(object/*!*/ self) {
            return DateTime.Now;
        }

        [RubyMethod("today", RubyMethodAttributes.PublicSingleton)]
        public static DateTime Today(object self) {
            return DateTime.Today;
        }

        #region local, mktime

        private static int NormalizeYear(int year) {
            if (year == 0) {
                return 2000;
            } else {
                return year;
            }
        }

        [RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateLocalTime(object/*!*/ self, int year) {
            return new DateTime(NormalizeYear(year), 1, 1);
        }

        [RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateLocalTime(object/*!*/ self, int year, int month) {
            return new DateTime(NormalizeYear(year), month, 1);
        }

        [RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateLocalTime(object/*!*/ self, int year, int month, int day) {
            return new DateTime(NormalizeYear(year), month, day);
        }

        [RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateLocalTime(object/*!*/ self, int year, int month, int day, int hour) {
            return new DateTime(NormalizeYear(year), month, day, hour, 0, 0);
        }

        [RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateLocalTime(object/*!*/ self, int year, int month, int day, int hour, int minute) {
            return new DateTime(NormalizeYear(year), month, day, hour, minute, 0);
        }

        [RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateLocalTime(object/*!*/ self, int year, int month, int day, int hour, int minute, int second) {
            return new DateTime(NormalizeYear(year), month, day, hour, minute, second);
        }

        [RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateLocalTime(object/*!*/ self, int year, int month, int day, int hour, int minute, int second, int microsecond) {
            return new DateTime(NormalizeYear(year), month, day, hour, minute, second).AddTicks(microsecond*10);
        }

        private static int GetComponent(ConversionStorage<int>/*!*/ conversionStorage, object[] components, int index, int defValue, bool zeroOk) {
            if (index >= components.Length || components[index] == null) {
                return defValue;
            }

            object component = components[index];

            int result;
            try {
                result = Protocols.CastToFixnum(conversionStorage, component);
            } catch (InvalidOperationException) {
                MutableString str = component as MutableString;
                if (str == null) {
                    throw;
                }
                result = checked((int)MutableStringOps.ToInteger(str, 10));
            }

            if (result == 0 && !zeroOk) {
                return defValue;
            } else {
                return result;
            }
        }

        private static int GetComponent(ConversionStorage<int>/*!*/ conversionStorage, object[] components, int index, int defValue) {
            return GetComponent(conversionStorage, components, index, defValue, true);
        }

        private static int GetYearComponent(ConversionStorage<int>/*!*/ conversionStorage, object[] components, int index) {
            return GetComponent(conversionStorage, components, index, 2000, false);
        }

        private static string[] /*!*/ _Months = new string[12] {
            "jan",
            "feb",
            "mar",
            "apr",
            "may",
            "jun",
            "jul",
            "aug",
            "sep",
            "oct",
            "nov",
            "dec"
        };

        private static int GetMonthComponent(ConversionStorage<int>/*!*/ conversionStorage, ConversionStorage<MutableString>/*!*/ strConversionStorage, 
            object[] components, int index) {
            const int defValue = 1;
            if (index >= components.Length || components[index] == null) {
                return defValue;
            }

            MutableString asStr = Protocols.TryCastToString(strConversionStorage, components[index]);
            if (asStr != null) {
                string str = asStr.ConvertToString();

                if (str.Length == 3) {
                    string strLower = str.ToLowerInvariant();
                    int monthIndex = _Months.FindIndex(delegate(string obj) { return obj == strLower; });
                    if (monthIndex != -1) {
                        return monthIndex + 1;
                    }
                }

                components[index] = asStr; // fall through after modifying the array
            }

            return GetComponent(conversionStorage, components, index, defValue, false);
        }

        [RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateLocalTime(ConversionStorage<int>/*!*/ conversionStorage, ConversionStorage<MutableString>/*!*/ strConversionStorage, 
            object/*!*/ self, [NotNull]params object[]/*!*/ components) {

            if (components.Length == 10) {
                // 10 arguments in the order output by Time#to_a are permitted.
                // The last 4 are ignored. The first 6 need to be used in the reverse order
                object[] newComponents = new object[6];
                Array.Copy(components, newComponents, 6);
                Array.Reverse(newComponents);
                components = newComponents;
            } else if (components.Length > 7 || components.Length == 0) {
                throw RubyExceptions.CreateArgumentError(String.Format("wrong number of arguments ({0} for 7)", components.Length));
            }
            
            return new DateTime(
                GetYearComponent(conversionStorage, components, 0),
                GetMonthComponent(conversionStorage, strConversionStorage, components, 1),
                GetComponent(conversionStorage, components, 2, 1),
                GetComponent(conversionStorage, components, 3, 0),
                GetComponent(conversionStorage, components, 4, 0),
                GetComponent(conversionStorage, components, 5, 0),
                GetComponent(conversionStorage, components, 6, 0)
            );
        }

        #endregion

        #region utc, gm

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateGmtTime(object/*!*/ self, int year) {
            return new DateTime(NormalizeYear(year), 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateGmtTime(object/*!*/ self, int year, int month) {
            return new DateTime(NormalizeYear(year), month, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateGmtTime(object/*!*/ self, int year, int month, int day) {
            return new DateTime(NormalizeYear(year), month, day, 0, 0, 0, DateTimeKind.Utc);
        }

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateGmtTime(object/*!*/ self, int year, int month, int day, int hour) {
            return new DateTime(NormalizeYear(year), month, day, hour, 0, 0, DateTimeKind.Utc);
        }

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateGmtTime(object/*!*/ self, int year, int month, int day, int hour, int minute) {
            return new DateTime(NormalizeYear(year), month, day, hour, minute, 0, DateTimeKind.Utc);
        }

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateGmtTime(object/*!*/ self, int year, int month, int day, int hour, int minute, int second) {
            return new DateTime(NormalizeYear(year), month, day, hour, minute, second, DateTimeKind.Utc);
        }

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateGmtTime(object/*!*/ self, int year, int month, int day, int hour, int minute, int second, int microsecond) {
            return new DateTime(NormalizeYear(year), month, day, hour, minute, second, DateTimeKind.Utc).AddTicks(microsecond * 10);
        }

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateGmtTime(ConversionStorage<int>/*!*/ conversionStorage, ConversionStorage<MutableString>/*!*/ strConversionStorage, 
            RubyContext/*!*/ context, object/*!*/ self, 
            [NotNull]params object[]/*!*/ components) {

            if (components.Length == 10) {
                // 10 arguments in the order output by Time#to_a are permitted.
                // The last 4 are ignored. The first 6 need to be used in the reverse order
                object[] newComponents = new object[6];
                Array.Copy(components, newComponents, 6);
                Array.Reverse(newComponents);
                components = newComponents;
            } else if (components.Length > 10 || components.Length == 0) {
                throw RubyExceptions.CreateArgumentError(String.Format("wrong number of arguments ({0} for 7)", components.Length));
            }

            return new DateTime(
                GetYearComponent(conversionStorage, components, 0),
                GetMonthComponent(conversionStorage, strConversionStorage, components, 1),
                GetComponent(conversionStorage, components, 2, 1),
                GetComponent(conversionStorage, components, 3, 0),
                GetComponent(conversionStorage, components, 4, 0),
                GetComponent(conversionStorage, components, 5, 0), DateTimeKind.Utc).AddTicks(
                    GetComponent(conversionStorage, components, 6, 0) * 10
                );
        }

        #endregion

        #endregion "Singleton Methods"

        #region _dump, _load

        [RubyMethod("_dump")]
        public static MutableString/*!*/ Dump(RubyContext/*!*/ context, DateTime self, [Optional]int depth) {
            if (self.Year < 1900) {
                throw RubyExceptions.CreateTypeError("unable to marshal time");
            }

            uint dword1 = 0x80000000;
            // Uncomment for Ruby 1.9 compat?
            // if (self.Kind == DateTimeKind.Utc) {
            //     dword1 |= 0x40000000;
            // } else {
            self = self.ToUniversalTime();
            dword1 |= (unchecked((uint)(self.Year - 1900)) << 14);
            dword1 |= (unchecked((uint)(self.Month - 1)) << 10);
            dword1 |= ((uint)self.Day << 5);
            dword1 |= ((uint)self.Hour);

            uint dword2 = 0;
            dword2 |= ((uint)self.Minute << 26);
            dword2 |= ((uint)self.Second << 20);
            dword2 |= ((uint)((self.Ticks % 10000000) / 10));

            MemoryStream buf = new MemoryStream(8);
            BinaryWriter writer = new BinaryWriter(buf);
            writer.Write(dword1);
            writer.Write(dword2);

            return MutableString.CreateBinary(buf.ToArray());
        }

        private static uint GetUint(byte[] data, int start) {
            Assert.NotNull(data);
            return (((((((uint)data[start + 3] << 8) + (uint)data[start + 2]) << 8) + (uint)data[start + 1]) << 8) + (uint)data[start + 0]);
        }

        [RubyMethod("_load", RubyMethodAttributes.PublicSingleton)]
        public static DateTime Load(RubyContext/*!*/ context, object/*!*/ self, [NotNull]MutableString time) {
            byte[] data = time.ConvertToBytes();
            if (data.Length != 8 || (data[3] & 0x80) != 0x80) {
                throw RubyExceptions.CreateTypeError("marshaled time format differ");
            }
            bool isUtc = (data[3] & 0x40) != 0;
            uint dword1 = GetUint(data, 0);
            int year = 1900 + (int)((dword1 >> 14) & 0xffff);
            int month = 1 + (int)((dword1 >> 10) & 0x0f);
            int day = (int)((dword1 >> 5) & 0x01f);
            int hour = (int)(dword1 & 0x01f);

            uint dword2 = GetUint(data, 4);
            int minute = (int)((dword2 >> 26) & 0x2f);
            int second = (int)((dword2 >> 20) & 0x2f);
            int usec = (int)(dword2 & 0xfffff);

            try {
                DateTime result = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
                result = result.AddTicks(usec * 10L);
                if (!isUtc) {
                    result = result.ToLocalTime();
                }
                return result;
            } catch (ArgumentOutOfRangeException) {
                throw RubyExceptions.CreateTypeError("marshaled time format differ");
            }
        }

        #endregion _dump, _load

        [RubyMethod("+")]
        public static DateTime AddSeconds(DateTime self, [DefaultProtocol]double seconds) {
            return self.AddSeconds(seconds);
        }

        [RubyMethod("+")]
        public static DateTime AddSeconds(DateTime self, DateTime seconds) {
            throw RubyExceptions.CreateTypeError("time + time?");
        }

        [RubyMethod("-")]
        public static DateTime SubtractSeconds(DateTime self, [DefaultProtocol]double seconds) {
            return self.AddSeconds(-1 * seconds);
        }

        [RubyMethod("-")]
        public static double SubtractTime(DateTime self, DateTime other) {
            return (self - other).TotalSeconds;
        }

        [RubyMethod("<=>")]
        public static object CompareSeconds(DateTime self, object other) {
            return null;
        }

        [RubyMethod("<=>")]
        public static int CompareTo(DateTime self, DateTime other) {
            return self.CompareTo(other);
        }

        // TODO: dup is not defined in MRI
        [RubyMethod("dup")]
        public static DateTime Clone(DateTime self) {
            return new DateTime(self.Ticks, self.Kind);
        }


        [RubyMethod("gmtime")]
        [RubyMethod("utc")]
        public static DateTime ToUTC(DateTime self) {
            return self.ToUniversalTime();
        }

        [RubyMethod("gmt?")]
        [RubyMethod("utc?")]
        public static bool IsUTC(DateTime self) {
            return self.Equals(self.ToUniversalTime());
        }

        [RubyMethod("dst?")]
        [RubyMethod("isdst")]
        public static bool IsDST(DateTime self) {
            return self.IsDaylightSavingTime();
        }

        [RubyMethod("localtime")]
        public static DateTime ToLocalTime(DateTime self) {
            return self.ToLocalTime();
        }

        [RubyMethod("hour")]
        public static int Hour(DateTime self) {
            return self.Hour;
        }

        [RubyMethod("min")]
        public static int Minute(DateTime self) {
            return self.Minute;
        }

        [RubyMethod("sec")]
        public static int Second(DateTime self) {
            return self.Second;
        }

        [RubyMethod("year")]
        public static int Year(DateTime self) {
            return self.Year;
        }

        [RubyMethod("mon")]
        [RubyMethod("month")]
        public static int Month(DateTime self) {
            return self.Month;
        }

        [RubyMethod("mday")]
        [RubyMethod("day")]
        public static int Day(DateTime self) {
            return self.Day;
        }

        [RubyMethod("yday")]
        public static int DayOfYear(DateTime self) {
            return self.DayOfYear;
        }

        [RubyMethod("strftime")]
        public static MutableString/*!*/ FormatTime(DateTime self, [DefaultProtocol, NotNull]MutableString/*!*/ format) {
            // TODO (encoding):

            bool inEscape = false;
            StringBuilder builder = new StringBuilder();
            foreach (char c in format.ToString()) {
                if (c == '%' && !inEscape) {
                    inEscape = true;
                    continue;
                }
                if (inEscape) {
                    string thisFormat = null;
                    DateTime firstDay;
                    int week;
                    switch (c) {
                        case 'a':
                            thisFormat = "ddd";
                            break;
                        case 'A':
                            thisFormat = "dddd";
                            break;
                        case 'b':
                            thisFormat = "MMM";
                            break;
                        case 'B':
                            thisFormat = "MMMM";
                            break;
                        case 'c':
                            thisFormat = "g";
                            break;
                        case 'd':
                            thisFormat = "dd";
                            break;
                        case 'H':
                            thisFormat = "HH";
                            break;
                        case 'I':
                            thisFormat = "hh";
                            break;
                        case 'j':
                            builder.AppendFormat("{0:000}", self.DayOfYear);
                            break;
                        case 'm':
                            thisFormat = "MM";
                            break;
                        case 'M':
                            thisFormat = "mm";
                            break;
                        case 'p':
                            thisFormat = "tt";
                            break;
                        case 'S':
                            thisFormat = "ss";
                            break;
                        case 'U':
                            firstDay = self.AddDays(1 - self.DayOfYear);
                            DateTime firstSunday = firstDay.AddDays((7 - (int)firstDay.DayOfWeek) % 7);
                            week = 1 + (int)Math.Floor((self - firstSunday).Days / 7.0);
                            builder.AppendFormat("{0:00}", week);
                            break;
                        case 'W':
                            firstDay = self.AddDays(1 - self.DayOfYear);
                            DateTime firstMonday = firstDay.AddDays((8 - (int)firstDay.DayOfWeek) % 7);
                            week = 1 + (int)Math.Floor((self - firstMonday).Days / 7.0);
                            builder.AppendFormat("{0:00}", week);
                            break;
                        case 'w':
                            builder.Append((int)self.DayOfWeek);
                            break;
                        case 'x':
                            thisFormat = "d";
                            break;
                        case 'X':
                            thisFormat = "t";
                            break;
                        case 'y':
                            thisFormat = "yy";
                            break;
                        case 'Y':
                            thisFormat = "yyyy";
                            break;
                        case 'Z':
                            thisFormat = "%K";
                            break;
                        default:
                            builder.Append(c);
                            break;
                    }
                    if (thisFormat != null) {
                        builder.Append(self.ToString(thisFormat, CultureInfo.InvariantCulture));
                    }
                    inEscape = false;
                } else {
                    builder.Append(c);
                }
            }

            return MutableString.Create(builder.ToString(), format.Encoding);
        }

        [RubyMethod("succ")]
        public static DateTime SuccessiveSecond(DateTime self) {
            return self.AddSeconds(1);
        }

        private static long GetSeconds(DateTime self) {
            return (self.ToUniversalTime().Ticks - epoch.Ticks) / 10000000;
        }

        [RubyMethod("to_f")]
        public static double ToFloatSeconds(DateTime self) {
            double seconds = (self.ToUniversalTime().Ticks - epoch.Ticks) / 10000000.0;
            return seconds;
        }

        [RubyMethod("tv_sec")]
        [RubyMethod("to_i")]
        public static object/*!*/ ToSeconds(DateTime self) {
            return Protocols.Normalize(GetSeconds(self));
        }

        [RubyMethod("tv_usec")]
        [RubyMethod("usec")]
        public static object/*!*/ GetMicroSeconds(DateTime self) {
            return Protocols.Normalize((self.Ticks % 10000000) / 10);
        }

        [RubyMethod("asctime")]
        [RubyMethod("ctime")]
        [RubyMethod("inspect")]
        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToString(DateTime self) {
            return MutableString.CreateAscii(self.ToString("ddd MMM dd HH:mm:ss K yyyy", CultureInfo.InvariantCulture));
        }

        [RubyMethod("gmtoff")]
        [RubyMethod("utc_offset")]
        [RubyMethod("gmt_offset")]
        public static object Offset(DateTime self) {
            return Protocols.Normalize((self.Ticks - self.ToUniversalTime().Ticks) / 10000000);
        }

        [RubyMethod("eql?")]
        [RubyMethod("==")]
        public static bool Eql(DateTime/*!*/ self, DateTime other) {
            return self == other;
        }

        [RubyMethod("eql?")]
        public static bool Eql(DateTime/*!*/ self, object other) {
            return false;
        }

        [RubyMethod("==")]
        public static object Equals(RubyContext/*!*/ context, DateTime/*!*/ self, object other) {
            return (context.RubyOptions.Compatibility == RubyCompatibility.Ruby18) ? (object)null : false;
        }

        [RubyMethod("getgm")]
        [RubyMethod("getutc")]
        public static DateTime GetUTC(DateTime/*!*/ self) {
            return self.ToUniversalTime();
        }

        [RubyMethod("getlocal")]
        public static DateTime GetLocal(DateTime/*!*/ self) {
            return self.ToLocalTime();
        }

        [RubyMethod("to_a")]
        public static RubyArray ToArray(DateTime/*!*/ self) {
            RubyArray result = new RubyArray();
            result.Add(self.Second);
            result.Add(self.Minute);
            result.Add(self.Hour);
            result.Add(self.Day);
            result.Add(self.Month);
            result.Add(self.Year);
            result.Add((int)self.DayOfWeek);
            result.Add(self.DayOfYear);
            result.Add(self.IsDaylightSavingTime());
            result.Add(GetZone(self));
            return result;
        }           

        [RubyMethod("wday")]
        public static int DayOfWeek(DateTime/*!*/ self) {
            return (int)self.DayOfWeek;
        }

        [RubyMethod("hash")]
        public static int GetHash(DateTime/*!*/ self) {
            return self.GetHashCode();
        }

        [RubyMethod("zone")]
        public static MutableString/*!*/ GetZone(DateTime/*!*/ self) {
            // TODO: 
            return MutableString.CreateAscii("UTC");
        }
    }
}