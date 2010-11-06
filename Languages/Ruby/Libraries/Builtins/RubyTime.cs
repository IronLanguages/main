/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
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
using System.Security;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace IronRuby.Builtins {
    #region RubyTime

    public class RubyTime : IComparable, IComparable<RubyTime>, IEquatable<RubyTime>, IFormattable {
        public readonly static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); //January 1, 1970 00:00 UTC
        
        #region Time Zones

#if SILVERLIGHT
        public TimeSpan GetCurrentZoneOffset() {
            DateTime time = DateTime.Now;
            return time.ToLocalTime() - time.ToUniversalTime();
        }

        public static string GetCurrentZoneName() {
            return DateTime.Now.ToString("%K");
        }

        public bool GetCurrentDst(RubyContext/*!*/ context) { 
            return _dateTime.IsDaylightSavingTime(); 
        }

        public static DateTime ToUniversalTime(DateTime dateTime) {
            return dateTime.ToUniversalTime(); 
        }

        public static DateTime ToLocalTime(DateTime dateTime) {
            return dateTime.ToLocalTime(); 
        }
#else
        internal static TimeZone/*!*/ _CurrentTimeZone;
        private static Regex _tzPattern;

        static RubyTime() {
            string tz;
            try {
                tz = Environment.GetEnvironmentVariable("TZ");
            } catch (SecurityException) {
                tz = null;
            }
            TimeZone zone;
            RubyTime.TryParseTimeZone(tz, out zone);
            RubyTime._CurrentTimeZone = zone ?? TimeZone.CurrentTimeZone;
        }

        // TODO: Use Olson TZ database names
        // See http://www.opengroup.org/onlinepubs/007908799/xbd/envvar.html
        //     http://www.twinsun.com/tz/tz-link.htm
        //     http://blogs.msdn.com/bclteam/archive/2006/04/03/567119.aspx
        public static bool TryParseTimeZone(string timeZoneEnvSpec, out TimeZone timeZone) {
            if (String.IsNullOrEmpty(timeZoneEnvSpec)) {
                timeZone = TimeZone.CurrentTimeZone;
                return true;
            }

            if (_tzPattern == null) {
                // TODO: we require offset and don't recognize DST rules
                _tzPattern = new Regex(@"^\s*
                    (?<std>[^-+:,0-9\0]{3,})
                    (?<sh>[+-]?[0-9]{1,2})((:(?<sm>[0-9]{1,2}))?(:(?<ss>[0-9]{1,2}))?)?                    
                    ", 
                    RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant
                );
            }

            Match match = _tzPattern.Match(timeZoneEnvSpec);
            if (!match.Success) {
                timeZone = null;
                return false;
            }

            timeZone = new TZ(
                new TimeSpan(
                    -Int32.Parse(match.Groups["sh"].Value, CultureInfo.InvariantCulture), 
                    match.Groups["sm"].Success ? Int32.Parse(match.Groups["sm"].Value, CultureInfo.InvariantCulture) : 0,
                    match.Groups["ss"].Success ? Int32.Parse(match.Groups["ss"].Value, CultureInfo.InvariantCulture) : 0
                ),
                match.Groups["std"].Value
            );

            return true;
        }

        private sealed class TZ : TimeZone {
            private readonly TimeSpan _offset;
            private readonly string/*!*/ _standardName;

            public TZ(TimeSpan offset, string/*!*/ standardName) {
                _offset = offset;
                _standardName = standardName;
            }

            public override string/*!*/ DaylightName {
                get { return _standardName; }
            }

            public override string/*!*/ StandardName {
                get { return _standardName; }
            }

            public override TimeSpan GetUtcOffset(DateTime time) {
                return time.Kind == DateTimeKind.Local ? _offset : TimeSpan.Zero;
            }

            public override DateTime ToLocalTime(DateTime time) {
                return Adjust(time, _offset, DateTimeKind.Local);
            }

            public override DateTime ToUniversalTime(DateTime time) {
                return Adjust(time, -_offset, DateTimeKind.Utc);
            }

            private static DateTime Adjust(DateTime time, TimeSpan targetOffset, DateTimeKind targetKind) {
                if (time.Kind == targetKind) {
                    return time;
                }
                long ticks = time.Ticks + targetOffset.Ticks;

                if (ticks > DateTime.MaxValue.Ticks) {
                    return new DateTime(DateTime.MaxValue.Ticks, targetKind);
                }

                if (ticks < DateTime.MinValue.Ticks) {
                    return new DateTime(DateTime.MinValue.Ticks, targetKind);
                }

                return new DateTime(ticks, targetKind);
            }

            public override bool IsDaylightSavingTime(DateTime time) {
                throw new NotSupportedException();
            }

            public override DaylightTime GetDaylightChanges(int year) {
                throw new NotSupportedException();
            }
        }

        public TimeSpan GetCurrentZoneOffset() {
            return _CurrentTimeZone.GetUtcOffset(_dateTime);
        }

        public static string GetCurrentZoneName() {
            return _CurrentTimeZone.StandardName;
        }

        public bool GetCurrentDst(RubyContext/*!*/ context) {
            var zone = _CurrentTimeZone;
            if (zone is TZ) {
                var stdName = zone.StandardName;
                zone = TimeZone.CurrentTimeZone;

                context.ReportWarning(String.Format(CultureInfo.InvariantCulture,
                    "Daylight savings rule not available for time zone `{0}'; using the default time zone `{1}'", stdName, zone.StandardName
                ));
            } 
            return zone.IsDaylightSavingTime(_dateTime);
        }

        public static DateTime ToUniversalTime(DateTime dateTime) {
            return _CurrentTimeZone.ToUniversalTime(dateTime);
        }

        public static DateTime ToLocalTime(DateTime dateTime) {
            return _CurrentTimeZone.ToLocalTime(dateTime);
        }
#endif

        public DateTime ToUniversalTime() {
            return ToUniversalTime(_dateTime);
        }

        public DateTime ToLocalTime() {
            return ToLocalTime(_dateTime);
        }

        public static DateTime GetCurrentLocalTime() {
            return ToLocalTime(DateTime.UtcNow);
        }

        #endregion

        private DateTime _dateTime;

        public RubyTime(DateTime dateTime) {
            _dateTime = Round(dateTime);
        }

        // Used by derived Ruby classes.
        public RubyTime()
            : this(ToLocalTime(Epoch)) {
        }

        public RubyTime(long ticks, DateTimeKind kind) 
            : this(new DateTime(ticks, kind)) {
        }

        private static DateTime Round(DateTime dateTime) {
            // Ruby time precision is 10s of ticks - this is important for time comparisons
            long ticks = dateTime.Ticks;
            if (ticks % 10 >= 5) {
                ticks = ticks - ticks % 10 + 10;
            } else {
                ticks = ticks - ticks % 10;
            }
            return new DateTime(ticks, dateTime.Kind);
        }

        public long TicksSinceEpoch {
            get { return ToUniversalTime().Ticks - Epoch.Ticks; }
        }

        public DateTime DateTime {
            get { return _dateTime; }
            set { _dateTime = Round(value); }
        }

        internal void SetDateTime(DateTime value) {
            Debug.Assert(value.Ticks % 10 == 0);
            _dateTime = value;
        }

        public long Ticks {
            get { return _dateTime.Ticks; }
        }

        public int Microseconds {
            get { return (int)((_dateTime.Ticks / 10) % 1000000); }
        }

        public DateTimeKind Kind {
            get { return _dateTime.Kind; }
        }

        internal static long ToTicks(long seconds, long microseconds) {
            return seconds * 10000000 + microseconds * 10;
        }

        internal static DateTime AddSeconds(DateTime dateTime, double seconds) {
            bool isLocal = dateTime.Kind == DateTimeKind.Local;
            if (isLocal) {
                dateTime = ToUniversalTime(dateTime);
            }

            // add in UTC to handle DST transitions correctly:
            dateTime = dateTime.AddTicks((long)(Math.Round(seconds, 6) * 10000000));

            if (isLocal) {
                dateTime = ToLocalTime(dateTime);
            }

            return dateTime;
        }

        public override string ToString() { 
            return _dateTime.ToString(); 
        }

        public override int GetHashCode() { 
            return _dateTime.GetHashCode();
        }

        int IComparable.CompareTo(object other) {
            return CompareTo(other as RubyTime);
        }

        public int CompareTo(RubyTime other) {
            return other != null ? ToUniversalTime(_dateTime).CompareTo(ToUniversalTime(other._dateTime)) : -1;
        }

        public static bool operator <(RubyTime x, RubyTime y) {
            return x.CompareTo(y) < 0;
        }

        public static bool operator <=(RubyTime x, RubyTime y) {
            return x.CompareTo(y) <= 0;
        }

        public static bool operator >(RubyTime x, RubyTime y) {
            return x.CompareTo(y) > 0;
        }

        public static bool operator >=(RubyTime x, RubyTime y) {
            return x.CompareTo(y) >= 0;
        }

        public static TimeSpan operator -(RubyTime x, DateTime y) {
            return ToUniversalTime(x._dateTime) - ToUniversalTime(y);
        }

        public static TimeSpan operator -(RubyTime x, RubyTime y) {
            return x - y._dateTime;
        }

        public override bool Equals(object obj) {
            return Equals(obj as RubyTime);
        }

        public bool Equals(RubyTime other) {
            return CompareTo(other) == 0;
        }

        public static bool operator ==(RubyTime x, RubyTime y) {
            return ReferenceEquals(x, null) ? ReferenceEquals(y, null) : x.Equals(y);
        }

        public static bool operator !=(RubyTime x, RubyTime y) {
            return !(x == y);
        }

        // RubyTime is less precise (+-5 ticks)
        public static explicit operator RubyTime(DateTime dateTime) {
            return new RubyTime(dateTime);
        }

        public static implicit operator DateTime(RubyTime time) {
            return time._dateTime;
        }

        public string ToString(string/*!*/ format, IFormatProvider/*!*/ provider) {
            return _dateTime.ToString(format, provider);
        }

        internal string FormatUtcOffset() {
            TimeSpan utcOffset = GetCurrentZoneOffset();

            return String.Format(CultureInfo.InvariantCulture,
                "{0}{1:D2}{2:D2}",
                (utcOffset.Hours >= 0) ? "+" : null,
                utcOffset.Hours,
                utcOffset.Minutes
            );
        }
    }

    #endregion

    // Keep the Ruby methods separate so that we can expose CLR methods on RubyTime.
    [RubyClass("Time", Extends = typeof(RubyTime), Inherits = typeof(object))]
    [Includes(typeof(Comparable))]
    public static class RubyTimeOps {
        #region Construction

        [RubyConstructor]
        public static RubyTime Create(RubyClass/*!*/ self) {
            return new RubyTime(RubyTime.GetCurrentLocalTime());
        }

        // Reinitialization. Not called when a factory/non-default ctor is called.
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static RubyTime/*!*/ Reinitialize(RubyTime/*!*/ self) {
            self.DateTime = RubyTime.GetCurrentLocalTime();
            return self;
        }

        [RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
        public static RubyTime/*!*/ InitializeCopy(RubyTime/*!*/ self, [NotNull]RubyTime/*!*/ other) {
            self.SetDateTime(other.DateTime);
            return self;
        }

        #region at

        [RubyMethod("at", RubyMethodAttributes.PublicSingleton)]
        public static RubyTime/*!*/ Create(RubyClass/*!*/ self, [NotNull]RubyTime/*!*/ other) {
            return new RubyTime(other.Ticks, other.Kind);
        }

        [RubyMethod("at", RubyMethodAttributes.PublicSingleton)]
        public static RubyTime/*!*/ Create(RubyClass/*!*/ self, double seconds) {
            return new RubyTime(RubyTime.ToLocalTime(RubyTime.AddSeconds(RubyTime.Epoch, seconds)));
        }

        [RubyMethod("at", RubyMethodAttributes.PublicSingleton)]
        public static RubyTime/*!*/ Create(RubyClass/*!*/ self, int seconds, int microseconds) {
            return new RubyTime(RubyTime.ToLocalTime(RubyTime.Epoch.AddTicks(RubyTime.ToTicks(seconds, microseconds))));
        }

        #endregion

        #region now

        [RubyMethod("now", RubyMethodAttributes.PublicSingleton)]
        public static RubyTime/*!*/ Now(RubyClass/*!*/ self) {
            return Create(self);
        }

        #endregion

        #region local, mktime, utc, gm

        private static int NormalizeYear(int year) {
            if (year == 0) {
                return 2000;
            } else {
                return year;
            }
        }

        private static RubyTime/*!*/ CreateTime(int year, int month, int day, int hour, int minute, int second, int microsecond, DateTimeKind kind) {
            DateTime result = new DateTime(NormalizeYear(year), month, day, hour, minute, second == 60 ? 59 : second, 0, kind);
            if (second == 60) {
                result = result.AddSeconds(1);
            }
            return new RubyTime(result.AddTicks(microsecond * 10));
        }

        [RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
        public static RubyTime/*!*/ CreateLocalTime(object/*!*/ self, 
            int year, 
            [DefaultParameterValue(1)]int month,
            [DefaultParameterValue(1)]int day, 
            [Optional]int hour,
            [Optional]int minute,
            [Optional]int second,
            [Optional]int microsecond) {
            return CreateTime(year, month, day, hour, minute, second, microsecond, DateTimeKind.Local);
        }

        [RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
        public static RubyTime/*!*/ CreateLocalTime(ConversionStorage<int>/*!*/ conversionStorage, ConversionStorage<MutableString>/*!*/ strConversionStorage,
            RubyClass/*!*/ self, params object[]/*!*/ components) {

            return CreateTime(conversionStorage, strConversionStorage, components, DateTimeKind.Local);
        }

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static RubyTime/*!*/ CreateGmtTime(object/*!*/ self,
            int year,
            [DefaultParameterValue(1)]int month,
            [DefaultParameterValue(1)]int day,
            [Optional]int hour,
            [Optional]int minute,
            [Optional]int second,
            [Optional]int microsecond) {

            return CreateTime(year, month, day, hour, minute, second, microsecond, DateTimeKind.Utc);
        }

        [RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
        public static RubyTime/*!*/ CreateGmtTime(ConversionStorage<int>/*!*/ conversionStorage, ConversionStorage<MutableString>/*!*/ strConversionStorage,
            RubyClass/*!*/ self, params object[]/*!*/ components) {

            return CreateTime(conversionStorage, strConversionStorage, components, DateTimeKind.Utc);
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
        
        private static RubyTime/*!*/ CreateTime(ConversionStorage<int>/*!*/ conversionStorage, ConversionStorage<MutableString>/*!*/ strConversionStorage, 
            object[]/*!*/ components, DateTimeKind kind) {

            if (components.Length == 10) {
                // 10 arguments in the order output by Time#to_a are permitted.
                // The last 4 are ignored. The first 6 need to be used in the reverse order
                object[] newComponents = new object[6];
                Array.Copy(components, newComponents, 6);
                Array.Reverse(newComponents);
                components = newComponents;
            } else if (components.Length > 7 || components.Length == 0) {
                throw RubyExceptions.CreateArgumentError("wrong number of arguments ({0} for 7)", components.Length);
            }

            return CreateTime(
                GetYearComponent(conversionStorage, components, 0),
                GetMonthComponent(conversionStorage, strConversionStorage, components, 1),
                GetComponent(conversionStorage, components, 2, 1),
                GetComponent(conversionStorage, components, 3, 0),
                GetComponent(conversionStorage, components, 4, 0),
                GetComponent(conversionStorage, components, 5, 0),
                GetComponent(conversionStorage, components, 6, 0),
                kind
            );
        }

        #endregion

        #endregion

        #region times (deprecated)
#if !SILVERLIGHT
        [RubyMethod("times", RubyMethodAttributes.PublicSingleton, Compatibility = RubyCompatibility.Ruby186, BuildConfig = "!SILVERLIGHT")]
        public static RubyStruct/*!*/ Times(RubyClass/*!*/ self) {
            return RubyProcess.GetTimes(self);
        }
#endif
        #endregion

        #region _dump, _load

        [RubyMethod("_dump")]
        public static MutableString/*!*/ Dump(RubyContext/*!*/ context, RubyTime/*!*/ self, [Optional]int depth) {
            if (self.DateTime.Year < 1900 || self.DateTime.Year > 2038) {
                throw RubyExceptions.CreateTypeError("unable to marshal time");
            }

            DateTime value = RubyTime.ToUniversalTime(self.DateTime);
            
            // Little Endian
            //            32            |                32                  |
            // minute:6|second:6|usec:20|1|utc:1|year:16|month:4|day:5|hour:5|
            uint dword1 = self.Kind == DateTimeKind.Utc ? 0xC0000000 : 0x80000000;
            dword1 |= (unchecked((uint)(value.Year - 1900)) << 14);
            dword1 |= (unchecked((uint)(value.Month - 1)) << 10);
            dword1 |= ((uint)value.Day << 5);
            dword1 |= ((uint)value.Hour);

            uint dword2 = 0;
            dword2 |= ((uint)value.Minute << 26);
            dword2 |= ((uint)value.Second << 20);
            dword2 |= ((uint)((value.Ticks % 10000000) / 10));

            MemoryStream buf = new MemoryStream(8);
            RubyEncoder.Write(buf, dword1, !BitConverter.IsLittleEndian);
            RubyEncoder.Write(buf, dword2, !BitConverter.IsLittleEndian);
            return MutableString.CreateBinary(buf.ToArray());
        }

        private static uint GetUint(byte[] data, int start) {
            Assert.NotNull(data);
            return (((((((uint)data[start + 3] << 8) + (uint)data[start + 2]) << 8) + (uint)data[start + 1]) << 8) + (uint)data[start + 0]);
        }

        [RubyMethod("_load", RubyMethodAttributes.PublicSingleton)]
        public static RubyTime/*!*/ Load(RubyContext/*!*/ context, RubyClass/*!*/ self, [NotNull]MutableString/*!*/ time) {
            byte[] data = time.ConvertToBytes();
            if (data.Length != 8) {
                throw RubyExceptions.CreateTypeError("marshaled time format differ");
            }

            uint dword1 = GetUint(data, 0);
            uint dword2 = GetUint(data, 4);

            if ((data[3] & 0x80) == 0) {
                int secondsSinceEpoch = (int)dword1;
                uint microseconds = dword2;

                return new RubyTime(RubyTime.ToLocalTime(RubyTime.Epoch.AddTicks(RubyTime.ToTicks(secondsSinceEpoch, microseconds))));
            } else {
                bool isUtc = (data[3] & 0x40) != 0;
                int year = 1900 + (int)((dword1 >> 14) & 0xffff);
                int month = 1 + (int)((dword1 >> 10) & 0x0f);
                int day = (int)((dword1 >> 5) & 0x01f);
                int hour = (int)(dword1 & 0x01f);

                int minute = (int)((dword2 >> 26) & 0x2f);
                int second = (int)((dword2 >> 20) & 0x2f);
                int usec = (int)(dword2 & 0xfffff);

                DateTime result;
                try {
                    result = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).AddTicks(usec * 10);
                } catch (ArgumentOutOfRangeException) {
                    throw RubyExceptions.CreateTypeError("marshaled time format differ");
                }

                return new RubyTime(isUtc ? result : RubyTime.ToLocalTime(result));
            }
        }

        #endregion

        #region succ, +, -, <=>, eql?, hash

        [RubyMethod("succ")]
        public static RubyTime/*!*/ SuccessiveSecond(RubyTime/*!*/ self) {
            return AddSeconds(self, 1.0);
        }

        [RubyMethod("+")]
        public static RubyTime/*!*/ AddSeconds(RubyTime/*!*/ self, [DefaultProtocol]double seconds) {
            try {
                return new RubyTime(RubyTime.AddSeconds(self.DateTime, seconds));
            } catch (OverflowException) {
                throw RubyExceptions.CreateRangeError("time + {0:F6} out of Time range", seconds);
            }
        }

        [RubyMethod("+")]
        public static RubyTime/*!*/ AddSeconds(RubyTime/*!*/ self, [NotNull]RubyTime/*!*/ seconds) {
            throw RubyExceptions.CreateTypeError("time + time?");
        }

        [RubyMethod("-")]
        public static RubyTime/*!*/ SubtractSeconds(RubyTime/*!*/ self, [DefaultProtocol]double seconds) {
            DateTime result;
            try {
                result = RubyTime.AddSeconds(self.DateTime, -seconds);
            } catch (OverflowException) {
                throw RubyExceptions.CreateRangeError("time - {0:F6} out of Time range", seconds);
            }
            return new RubyTime(result);
        }

        [RubyMethod("-")]
        public static double SubtractTime(RubyTime/*!*/ self, [NotNull]RubyTime/*!*/ other) {
            return (self - other).TotalSeconds;
        }

        [RubyMethod("-")]
        public static double SubtractTime(RubyTime/*!*/ self, DateTime other) {
            return (self.DateTime - other).TotalSeconds;
        }

        [RubyMethod("<=>")]
        public static int CompareTo(RubyTime/*!*/ self, [NotNull]RubyTime/*!*/ other) {
            return self.CompareTo(other);
        }

        [RubyMethod("<=>")]
        public static object CompareSeconds(RubyTime/*!*/ self, object other) {
            return null;
        }

        [RubyMethod("eql?")]
        public static bool Eql(RubyTime/*!*/ self, [NotNull]RubyTime/*!*/ other) {
            return self.Equals(other);
        }

        [RubyMethod("eql?")]
        public static bool Eql(RubyTime/*!*/ self, object other) {
            return false;
        }

        [RubyMethod("hash")]
        public static int GetHash(RubyTime/*!*/ self) {
            return self.GetHashCode();
        }

        #endregion

        #region utc, utc?, dst?, utc_offset, getlocal, zone

        [RubyMethod("gmtime")]
        [RubyMethod("utc")]
        public static RubyTime/*!*/ SwitchToUtc(RubyTime/*!*/ self) {
            self.SetDateTime(self.ToUniversalTime());
            return self;
        }

        [RubyMethod("localtime")]
        [RubyMethod("getlocal")]
        public static RubyTime/*!*/ ToLocalTime(RubyTime/*!*/ self) {
            self.SetDateTime(self.ToLocalTime());
            return self;
        }

        [RubyMethod("gmt?")]
        [RubyMethod("utc?")]
        public static bool IsUts(RubyTime/*!*/ self) {
            return self.DateTime.Kind == DateTimeKind.Utc;
        }

        [RubyMethod("dst?")]
        [RubyMethod("isdst")]
        public static object IsDst(RubyContext/*!*/ context, RubyTime/*!*/ self) {
            return self.GetCurrentDst(context);
        }

        [RubyMethod("gmtoff")]
        [RubyMethod("utc_offset")]
        [RubyMethod("gmt_offset")]
        public static object Offset(RubyTime/*!*/ self) {
            return Protocols.Normalize(self.GetCurrentZoneOffset().Ticks / 10000000);
        }

        [RubyMethod("getgm")]
        [RubyMethod("getutc")]
        public static RubyTime/*!*/ GetUTC(RubyTime/*!*/ self) {
            return new RubyTime(self.ToUniversalTime());
        }

        [RubyMethod("zone")]
        public static MutableString/*!*/ GetZone(RubyContext/*!*/ context, RubyTime/*!*/ self) {
            if (self.Kind == DateTimeKind.Utc) {
                return MutableString.CreateAscii("UTC");
            } else {
                var name = RubyTime.GetCurrentZoneName();
                if (name.IsAscii()) {
                    return MutableString.CreateAscii(name);
                } else {
                    // TODO: what encoding should we use?
                    return MutableString.Create(name, context.GetPathEncoding());
                }
            }
        }

        #endregion

        #region hour, min, sec, usec, year, mon, day, yday, wday

        [RubyMethod("hour")]
        public static int Hour(RubyTime self) {
            return self.DateTime.Hour;
        }

        [RubyMethod("min")]
        public static int Minute(RubyTime self) {
            return self.DateTime.Minute;
        }

        [RubyMethod("sec")]
        public static int Second(RubyTime self) {
            return self.DateTime.Second;
        }

        [RubyMethod("tv_usec")]
        [RubyMethod("usec")]
        public static int GetMicroSeconds(RubyTime self) {
            return self.Microseconds;
        }

        [RubyMethod("year")]
        public static int Year(RubyTime self) {
            return self.DateTime.Year;
        }

        [RubyMethod("mon")]
        [RubyMethod("month")]
        public static int Month(RubyTime self) {
            return self.DateTime.Month;
        }

        [RubyMethod("mday")]
        [RubyMethod("day")]
        public static int Day(RubyTime self) {
            return self.DateTime.Day;
        }

        [RubyMethod("yday")]
        public static int DayOfYear(RubyTime self) {
            return self.DateTime.DayOfYear;
        }

        [RubyMethod("wday")]
        public static int DayOfWeek(RubyTime/*!*/ self) {
            return (int)self.DateTime.DayOfWeek;
        }

        #endregion

        #region strftime

        [RubyMethod("strftime")]
        public static MutableString/*!*/ FormatTime(RubyContext/*!*/ context, RubyTime/*!*/ self, 
            [DefaultProtocol, NotNull]MutableString/*!*/ format) {

            MutableString result = MutableString.CreateMutable(format.Encoding);
            bool inFormat = false;

            var charEnum = format.GetCharacters();
            while (charEnum.MoveNext()) {
                var character = charEnum.Current;
                int c = character.IsValid ? character.Value : -1;

                if (!inFormat) {
                    if (c == '%') {
                        inFormat = true;
                    } else {
                        result.Append(character);
                    }
                    continue;
                } 
                inFormat = false;
                string dateTimeFormat = null;
                switch (c) {
                    case '%':
                        result.Append('%');
                        break;

                    case 'a':
                        dateTimeFormat = "ddd";
                        break;

                    case 'A':
                        dateTimeFormat = "dddd";
                        break;

                    case 'b':
                        dateTimeFormat = "MMM";
                        break;

                    case 'B':
                        dateTimeFormat = "MMMM";
                        break;

                    case 'c':
                        dateTimeFormat = "g";
                        break;

                    case 'd':
                        dateTimeFormat = "dd";
                        break;

                    case 'D':
                        dateTimeFormat = "MM/dd/yy";
                        break;

                    case 'e': { // Day of the month, blank-padded ( 1..31)
                            int day = self.DateTime.Day;
                            if (day < 10) {
                                result.Append(' ');
                            }
                            result.Append(day.ToString(CultureInfo.InvariantCulture));
                            break;
                        }

                    case 'H':
                        dateTimeFormat = "HH";
                        break;

                    case 'I':
                        dateTimeFormat = "hh";
                        break;

                    case 'j':
                        result.AppendFormat("{0:000}", self.DateTime.DayOfYear);
                        break;

                    case 'l': {
                            int hour = self.DateTime.Hour;
                            if (hour == 0) {
                                hour = 12;
                            } else if (hour > 12) {
                                hour -= 12;
                            }
                            if (hour < 10) {
                                result.Append(' ');
                            }
                            result.Append(hour.ToString(CultureInfo.InvariantCulture));
                            break;
                        }

                    case 'm':
                        dateTimeFormat = "MM";
                        break;

                    case 'M':
                        dateTimeFormat = "mm";
                        break;

                    case 'p':
                        dateTimeFormat = "tt";
                        break;

                    case 'S':
                        dateTimeFormat = "ss";
                        break;

                    case 'T':
                        dateTimeFormat = "HH:mm:ss";
                        break;

                    case 'U': 
                        FormatDayOfWeek(result, self.DateTime, 7);
                        break;

                    case 'W':
                        FormatDayOfWeek(result, self.DateTime, 8); 
                        break;

                    case 'w':
                        result.Append(((int)self.DateTime.DayOfWeek).ToString(CultureInfo.InvariantCulture));
                        break;

                    case 'x':
                        dateTimeFormat = "d";
                        break;

                    case 'X':
                        dateTimeFormat = "t";
                        break;

                    case 'y':
                        dateTimeFormat = "yy";
                        break;

                    case 'Y':
                        dateTimeFormat = "yyyy";
                        break;

                    case 'Z':
                        dateTimeFormat = "%K";
                        break;

                    case 'z':
                        if (context.RubyOptions.Compatibility > RubyCompatibility.Ruby186) {
                            result.Append(self.FormatUtcOffset());
                        } else {
                            result.Append(RubyTime.GetCurrentZoneName());
                        }
                        break;

                    default:
                        if (context.RubyOptions.Compatibility > RubyCompatibility.Ruby186) {
                            result.Append(character);
                            break;
                        } 
                        return MutableString.CreateEmpty();
                }

                if (dateTimeFormat != null) {
                    result.Append(self.ToString(dateTimeFormat, CultureInfo.InvariantCulture));
                }
            }

            if (inFormat) {
                if (context.RubyOptions.Compatibility > RubyCompatibility.Ruby186) {
                    return result.Append('%');
                }
                return MutableString.CreateEmpty();
            }

            return result;
        }

        private static void FormatDayOfWeek(MutableString/*!*/ result, DateTime dateTime, int start) {
            DateTime firstDay = dateTime.AddDays(1 - dateTime.DayOfYear);
            DateTime firstSunday = firstDay.AddDays((start - (int)firstDay.DayOfWeek) % 7);
            int week = 1 + (int)Math.Floor((dateTime - firstSunday).Days / 7.0);
            result.AppendFormat("{0:00}", week);
        }

        #endregion

        #region to_f, to_i, to_a, usec

        [RubyMethod("to_f")]
        public static double ToFloatSeconds(RubyTime/*!*/ self) {
            return self.TicksSinceEpoch / 10000000.0;
        }

        [RubyMethod("tv_sec")]
        [RubyMethod("to_i")]
        public static object/*!*/ ToSeconds(RubyTime/*!*/ self) {
            return Protocols.Normalize(self.TicksSinceEpoch / 10000000);
        }

        [RubyMethod("asctime")]
        [RubyMethod("ctime")]
        public static MutableString/*!*/ CTime(RubyTime/*!*/ self) {
            return MutableString.CreateAscii(String.Format(CultureInfo.InvariantCulture,
                "{0:ddd MMM} {1,2} {0:HH:mm:ss yyyy}",
                self.DateTime,
                self.DateTime.Day
            ));
        }

        [RubyMethod("inspect")]
        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToString(RubyContext/*!*/ context, RubyTime/*!*/ self) {            
            return MutableString.CreateAscii(String.Format(CultureInfo.InvariantCulture, 
                "{0:yyyy-MM-dd HH:mm:ss} {1}", self.DateTime, (self.Kind == DateTimeKind.Utc) ? "UTC" : self.FormatUtcOffset()
            ));
        }

        [RubyMethod("to_a")]
        public static RubyArray ToArray(RubyContext/*!*/ context, RubyTime/*!*/ self) {
            RubyArray result = new RubyArray();
            result.Add(self.DateTime.Second);
            result.Add(self.DateTime.Minute);
            result.Add(self.DateTime.Hour);
            result.Add(self.DateTime.Day);
            result.Add(self.DateTime.Month);
            result.Add(self.DateTime.Year);
            result.Add((int)self.DateTime.DayOfWeek);
            result.Add(self.DateTime.DayOfYear);
            result.Add(self.GetCurrentDst(context));
            result.Add(GetZone(context, self));
            return result;
        }

        #endregion
    }
}