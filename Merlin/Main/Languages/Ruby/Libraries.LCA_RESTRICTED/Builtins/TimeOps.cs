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

    [RubyClass("Time"), Includes(typeof(Comparable))]
    public class Time : IComparable, IComparable<Time>, IEquatable<Time>, IFormattable {
        DateTime _dateTime;

        readonly static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); //January 1, 1970 00:00 UTC

        #region Wrappers for DateTime

        public Time(DateTime dateTime) {
            _dateTime = dateTime;
        }

        public Time(long ticks) {
            _dateTime = new DateTime(ticks);
        }

        public Time(long ticks, DateTimeKind kind) {
            _dateTime = new DateTime(ticks, kind);
        }

        public Time(int year, int month, int day) {
            _dateTime = new DateTime(year, month, day);
        }

        public Time(int year, int month, int day, Calendar calendar) {
            _dateTime = new DateTime(year, month, day, calendar);
        }

        public Time(int year, int month, int day, int hour, int minute, int second) {
            _dateTime = new DateTime(year, month, day, hour, minute, second);
        }

        public Time(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind) {
            _dateTime = new DateTime(year, month, day, hour, minute, second, kind);
        }

        public override string ToString() { return _dateTime.ToString(); }
        public override bool Equals(object obj) { return (obj is Time) && (((Time)obj).DateTime == _dateTime); }
        public override int  GetHashCode() { return _dateTime.GetHashCode(); }

        public string ToString(string format, IFormatProvider provider) { return _dateTime.ToString(format, provider); }

        public bool Equals(Time other) { return (other is Time) && (_dateTime.Equals(((Time)other).DateTime)); }
        public int CompareTo(object other) { 
            Time otherTime = other as Time; 
            if (otherTime == null) {
                throw new ArgumentException("other is not a Time");
            } else {
                return _dateTime.CompareTo(((Time)other).DateTime); 
            }
        }
        public int CompareTo(Time other) { return _dateTime.CompareTo(other.DateTime); }

        public static implicit operator Time(DateTime dateTime) {
            return new Time(dateTime);
        }

        public static implicit operator DateTime(Time time) {
            return time._dateTime;
        }

        public DateTime DateTime {
            get { return _dateTime; }
            set { _dateTime = value; }
        }

        public long Ticks { get { return _dateTime.Ticks; } }
        public DateTimeKind Kind { get { return _dateTime.Kind; } }
        public Time AddTicks(long ticks) { return _dateTime.AddTicks(ticks); }
        public Time AddSeconds(double seconds) { return _dateTime.AddSeconds(seconds); }
        public Time ToUniversalTime() { return _dateTime.ToUniversalTime(); }
        public bool IsDaylightSavingTime() { return _dateTime.IsDaylightSavingTime(); }
        public Time ToLocalTime() { return _dateTime.ToLocalTime(); }
        public Time Add(TimeSpan span) { return _dateTime.Add(span); }
        public Time AddDays(double days) { return _dateTime.AddDays(days); }
        public static TimeSpan operator -(Time x, Time y) { return x.DateTime - y.DateTime; }
        public static bool operator ==(Time x, Time y) { return x.DateTime == y.DateTime; }
        public static bool operator !=(Time x, Time y) { return x.DateTime != y.DateTime; }

        #endregion

        [RubyConstructor]
        public static Time Create(RubyClass/*!*/ self) {
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
        public static Time Create(object/*!*/ self, Time other) {
            return new Time(other.Ticks, other.Kind);
        }

        [RubyMethod("at", RubyMethodAttributes.PublicSingleton)]
        public static Time Create(object/*!*/ self, double seconds) {
            return epoch.ToLocalTime().AddSeconds(seconds);
        }
        

        [RubyMethod("at", RubyMethodAttributes.PublicSingleton)]
        public static Time Create(object/*!*/ self, long seconds, long microseconds) {
            long ticks = epoch.ToLocalTime().Ticks + secondsToTicks(seconds) + microsecondsToTicks(microseconds);
            return new Time(ticks);
        }

        #endregion

        [RubyMethod("now", RubyMethodAttributes.PublicSingleton)]
        public static Time CreateTime(object/*!*/ self) {
            return DateTime.Now;
        }

        [RubyMethod("today", RubyMethodAttributes.PublicSingleton)]
        public static Time Today(object self) {
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
        public static Time CreateLocalTime(object/*!*/ self, int year) {
            return new Time(NormalizeYear(year), 1, 1);
        }

        [RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
        public static Time CreateLocalTime(object/*!*/ self, int year, int month) {
            return new Time(NormalizeYear(year), month, 1);
        }

        [RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
        public static Time CreateLocalTime(object/*!*/ self, int year, int month, int day) {
            return new Time(NormalizeYear(year), month, day);
        }

        [RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
        public static Time CreateLocalTime(object/*!*/ self, int year, int month, int day, int hour) {
            return new Time(NormalizeYear(year), month, day, hour, 0, 0);
        }

        [RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
        public static Time CreateLocalTime(object/*!*/ self, int year, int month, int day, int hour, int minute) {
            return new Time(NormalizeYear(year), month, day, hour, minute, 0);
        }

        [RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
        public static Time CreateLocalTime(object/*!*/ self, int year, int month, int day, int hour, int minute, int second) {
            return new Time(NormalizeYear(year), month, day, hour, minute, second);
        }

        [RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
        public static Time CreateLocalTime(object/*!*/ self, int year, int month, int day, int hour, int minute, int second, int microsecond) {
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
        public static Time CreateLocalTime(ConversionStorage<int>/*!*/ conversionStorage, ConversionStorage<MutableString>/*!*/ strConversionStorage, 
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
        public static Time CreateGmtTime(object/*!*/ self, int year) {
            return new Time(NormalizeYear(year), 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static Time CreateGmtTime(object/*!*/ self, int year, int month) {
            return new Time(NormalizeYear(year), month, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static Time CreateGmtTime(object/*!*/ self, int year, int month, int day) {
            return new Time(NormalizeYear(year), month, day, 0, 0, 0, DateTimeKind.Utc);
        }

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static Time CreateGmtTime(object/*!*/ self, int year, int month, int day, int hour) {
            return new Time(NormalizeYear(year), month, day, hour, 0, 0, DateTimeKind.Utc);
        }

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static Time CreateGmtTime(object/*!*/ self, int year, int month, int day, int hour, int minute) {
            return new Time(NormalizeYear(year), month, day, hour, minute, 0, DateTimeKind.Utc);
        }

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static Time CreateGmtTime(object/*!*/ self, int year, int month, int day, int hour, int minute, int second) {
            return new Time(NormalizeYear(year), month, day, hour, minute, second, DateTimeKind.Utc);
        }

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static Time CreateGmtTime(object/*!*/ self, int year, int month, int day, int hour, int minute, int second, int microsecond) {
            return new DateTime(NormalizeYear(year), month, day, hour, minute, second, DateTimeKind.Utc).AddTicks(microsecond * 10);
        }

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static Time CreateGmtTime(ConversionStorage<int>/*!*/ conversionStorage, ConversionStorage<MutableString>/*!*/ strConversionStorage, 
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
        public static MutableString/*!*/ Dump(RubyContext/*!*/ context, Time self, [Optional]int depth) {
            if (self.DateTime.Year < 1900) {
                throw RubyExceptions.CreateTypeError("unable to marshal time");
            }

            uint dword1 = 0x80000000;
            // Uncomment for Ruby 1.9 compat?
            // if (self.Kind == DateTimeKind.Utc) {
            //     dword1 |= 0x40000000;
            // } else {
            self = self.DateTime.ToUniversalTime();
            dword1 |= (unchecked((uint)(self.DateTime.Year - 1900)) << 14);
            dword1 |= (unchecked((uint)(self.DateTime.Month - 1)) << 10);
            dword1 |= ((uint)self.DateTime.Day << 5);
            dword1 |= ((uint)self.DateTime.Hour);

            uint dword2 = 0;
            dword2 |= ((uint)self.DateTime.Minute << 26);
            dword2 |= ((uint)self.DateTime.Second << 20);
            dword2 |= ((uint)((self.DateTime.Ticks % 10000000) / 10));

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
        public static Time Load(RubyContext/*!*/ context, object/*!*/ self, [NotNull]MutableString time) {
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
                Time result = new Time(year, month, day, hour, minute, second, DateTimeKind.Utc);
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
        public static Time AddSeconds(Time self, [DefaultProtocol]double seconds) {
            return self.AddSeconds(seconds);
        }

        [RubyMethod("+")]
        public static Time AddSeconds(Time self, [NotNull]Time seconds) {
            throw RubyExceptions.CreateTypeError("time + time?");
        }

        [RubyMethod("-")]
        public static Time SubtractSeconds(Time self, [DefaultProtocol]double seconds) {
            return self.AddSeconds(-1 * seconds);
        }

        [RubyMethod("-")]
        public static double SubtractTime(Time self, [NotNull]Time other) {
            return (self.DateTime - other.DateTime).TotalSeconds;
        }

        [RubyMethod("<=>")]
        public static object CompareSeconds(Time self, object other) {
            return null;
        }

        [RubyMethod("<=>")]
        public static int CompareTo(Time self, [NotNull]Time other) {
            return self.DateTime.CompareTo(other.DateTime);
        }

        // TODO: dup is not defined in MRI
        [RubyMethod("dup")]
        public static Time Clone(Time self) {
            return new Time(self.Ticks, self.Kind);
        }


        [RubyMethod("gmtime")]
        [RubyMethod("utc")]
        public static Time ToUTC(Time self) {
            self._dateTime = self._dateTime.ToUniversalTime();
            return self;
        }

        [RubyMethod("gmt?")]
        [RubyMethod("utc?")]
        public static bool IsUTC(Time self) {
            return self.DateTime.Kind == DateTimeKind.Utc;
        }

        [RubyMethod("dst?")]
        [RubyMethod("isdst")]
        public static bool IsDST(Time self) {
            return self.IsDaylightSavingTime();
        }

        [RubyMethod("localtime")]
        public static Time ToLocalTime(Time self) {
            return self.ToLocalTime();
        }

        [RubyMethod("hour")]
        public static int Hour(Time self) {
            return self.DateTime.Hour;
        }

        [RubyMethod("min")]
        public static int Minute(Time self) {
            return self.DateTime.Minute;
        }

        [RubyMethod("sec")]
        public static int Second(Time self) {
            return self.DateTime.Second;
        }

        [RubyMethod("year")]
        public static int Year(Time self) {
            return self.DateTime.Year;
        }

        [RubyMethod("mon")]
        [RubyMethod("month")]
        public static int Month(Time self) {
            return self.DateTime.Month;
        }

        [RubyMethod("mday")]
        [RubyMethod("day")]
        public static int Day(Time self) {
            return self.DateTime.Day;
        }

        [RubyMethod("yday")]
        public static int DayOfYear(Time self) {
            return self.DateTime.DayOfYear;
        }

        [RubyMethod("strftime")]
        public static MutableString/*!*/ FormatTime(Time self, [DefaultProtocol, NotNull]MutableString/*!*/ format) {
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
                    Time firstDay;
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
                            builder.AppendFormat("{0:000}", self.DateTime.DayOfYear);
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
                            firstDay = self.AddDays(1 - self.DateTime.DayOfYear);
                            Time firstSunday = firstDay.AddDays((7 - (int)firstDay.DateTime.DayOfWeek) % 7);
                            week = 1 + (int)Math.Floor((self - firstSunday).Days / 7.0);
                            builder.AppendFormat("{0:00}", week);
                            break;
                        case 'W':
                            firstDay = self.AddDays(1 - self.DateTime.DayOfYear);
                            Time firstMonday = firstDay.AddDays((8 - (int)firstDay.DateTime.DayOfWeek) % 7);
                            week = 1 + (int)Math.Floor((self - firstMonday).Days / 7.0);
                            builder.AppendFormat("{0:00}", week);
                            break;
                        case 'w':
                            builder.Append((int)self.DateTime.DayOfWeek);
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
        public static Time SuccessiveSecond(Time self) {
            return self.AddSeconds(1);
        }

        private static long GetSeconds(Time self) {
            return (self.ToUniversalTime().Ticks - epoch.Ticks) / 10000000;
        }

        [RubyMethod("to_f")]
        public static double ToFloatSeconds(Time self) {
            double seconds = (self.ToUniversalTime().Ticks - epoch.Ticks) / 10000000.0;
            return seconds;
        }

        [RubyMethod("tv_sec")]
        [RubyMethod("to_i")]
        public static object/*!*/ ToSeconds(Time self) {
            return Protocols.Normalize(GetSeconds(self));
        }

        [RubyMethod("tv_usec")]
        [RubyMethod("usec")]
        public static object/*!*/ GetMicroSeconds(Time self) {
            return Protocols.Normalize((self.Ticks % 10000000) / 10);
        }

        [RubyMethod("asctime")]
        [RubyMethod("ctime")]
        [RubyMethod("inspect")]
        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToString(Time self) {
            return MutableString.CreateAscii(self.ToString("ddd MMM dd HH:mm:ss K yyyy", CultureInfo.InvariantCulture));
        }

        [RubyMethod("gmtoff")]
        [RubyMethod("utc_offset")]
        [RubyMethod("gmt_offset")]
        public static object Offset(Time self) {
            return Protocols.Normalize((self.Ticks - self.ToUniversalTime().Ticks) / 10000000);
        }

        [RubyMethod("eql?")]
        [RubyMethod("==")]
        public static bool Eql(Time/*!*/ self, Time other) {
            return self == other;
        }

        [RubyMethod("eql?")]
        public static bool Eql(Time/*!*/ self, object other) {
            return false;
        }

        [RubyMethod("==")]
        public static object Equals(RubyContext/*!*/ context, Time/*!*/ self, object other) {
            return (context.RubyOptions.Compatibility == RubyCompatibility.Ruby18) ? (object)null : false;
        }

        [RubyMethod("getgm")]
        [RubyMethod("getutc")]
        public static Time GetUTC(Time/*!*/ self) {
            return self.ToUniversalTime();
        }

        [RubyMethod("getlocal")]
        public static Time GetLocal(Time/*!*/ self) {
            return self.ToLocalTime();
        }

        [RubyMethod("to_a")]
        public static RubyArray ToArray(Time/*!*/ self) {
            RubyArray result = new RubyArray();
            result.Add(self.DateTime.Second);
            result.Add(self.DateTime.Minute);
            result.Add(self.DateTime.Hour);
            result.Add(self.DateTime.Day);
            result.Add(self.DateTime.Month);
            result.Add(self.DateTime.Year);
            result.Add((int)self.DateTime.DayOfWeek);
            result.Add(self.DateTime.DayOfYear);
            result.Add(self.IsDaylightSavingTime());
            result.Add(GetZone(self));
            return result;
        }           

        [RubyMethod("wday")]
        public static int DayOfWeek(Time/*!*/ self) {
            return (int)self.DateTime.DayOfWeek;
        }

        [RubyMethod("hash")]
        public static int GetHash(Time/*!*/ self) {
            return self.GetHashCode();
        }

        [RubyMethod("zone")]
        public static MutableString/*!*/ GetZone(Time/*!*/ self) {
            // TODO: 
            return MutableString.CreateAscii("UTC");
        }
    }
}