/***** BEGIN LICENSE BLOCK *****
 * Version: CPL 1.0
 *
 * The contents of this file are subject to the Common Public
 * License Version 1.0 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.eclipse.org/legal/cpl-v10.html
 *
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 *
 * Copyright (C) 2007 Ola Bini <ola@ologix.com>
 * Copyright (c) Microsoft Corporation.
 * 
 ***** END LICENSE BLOCK *****/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using IronRuby.Builtins;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Yaml {

    public class SafeConstructor : BaseConstructor {
        private readonly static Dictionary<string, YamlConstructor> _yamlConstructors = new Dictionary<string, YamlConstructor>();
        private readonly static Dictionary<string, YamlMultiConstructor> _yamlMultiConstructors = new Dictionary<string, YamlMultiConstructor>();
        private readonly static Dictionary<string, Regex> _yamlMultiRegexps = new Dictionary<string, Regex>();

        public override YamlConstructor GetYamlConstructor(string key) {
            YamlConstructor result;
            if (_yamlConstructors.TryGetValue(key, out result)) {
                return result;
            }
            return base.GetYamlConstructor(key);
        }

        public override YamlMultiConstructor GetYamlMultiConstructor(string key) {
            YamlMultiConstructor result;
            if (_yamlMultiConstructors.TryGetValue(key, out result)) {
                return result;
            }
            return base.GetYamlMultiConstructor(key);
        }

        public override Regex GetYamlMultiRegexp(string key) {
            Regex result;
            if (_yamlMultiRegexps.TryGetValue(key, out result)) {
                return result;
            }
            return base.GetYamlMultiRegexp(key);
        }

        public override ICollection<string> GetYamlMultiRegexps() {
            return _yamlMultiRegexps.Keys;
        }

        public new static void AddConstructor(string tag, YamlConstructor ctor) {
            _yamlConstructors.Add(tag, ctor);
        }

        public new static void AddMultiConstructor(string tagPrefix, YamlMultiConstructor ctor) {
            _yamlMultiConstructors.Add(tagPrefix, ctor);
            _yamlMultiRegexps.Add(tagPrefix, new Regex("^" + tagPrefix, RegexOptions.Compiled));
        }

        public SafeConstructor(/*!*/NodeProvider nodeProvider, RubyGlobalScope/*!*/ scope)
            : base(nodeProvider, scope) {
        }

        private static Dictionary<string, bool> BOOL_VALUES = new Dictionary<string, bool>();

        public static object ConstructYamlNull(IConstructor ctor, Node node) {
            return null;
        }

        public static object ConstructYamlBool(IConstructor ctor, Node node) {
            bool result;
            if (TryConstructYamlBool(ctor, node, out result)) {
                return result;
            }
            return null;
        }

        public static bool TryConstructYamlBool(IConstructor ctor, Node node, out bool result) {            
            if (BOOL_VALUES.TryGetValue(ctor.ConstructScalar(node).ToString(), out result)) {
                return true;
            }
            return false;
        }

        public static object ConstructYamlOmap(IConstructor ctor, Node node) {
            return ctor.ConstructPairs(node);
        }

        public static object ConstructYamlPairs(IConstructor ctor, Node node) {
            return ConstructYamlOmap(ctor, node);
        }

        public static ICollection ConstructYamlSet(IConstructor ctor, Node node) {
            return ctor.ConstructMapping(node).Keys;
        }

        public static string ConstructYamlStr(IConstructor ctor, Node node) {
            string value = ctor.ConstructScalar(node).ToString();
            return value.Length != 0 ? value : null;
        }

        public static RubyArray ConstructYamlSeq(IConstructor ctor, Node node) {
            return ctor.ConstructSequence(node);
        }

        public static Hash ConstructYamlMap(IConstructor ctor, Node node) {
            return ctor.ConstructMapping(node);
        }        

        public static object constructUndefined(IConstructor ctor, Node node) {
            throw new ConstructorException("could not determine a constructor for the tag: " + node.Tag);
        }

        private static Regex TIMESTAMP_REGEXP = new Regex("^(-?[0-9][0-9][0-9][0-9])-([0-9][0-9]?)-([0-9][0-9]?)(?:(?:[Tt]|[ \t]+)([0-9][0-9]?):([0-9][0-9]):([0-9][0-9])(?:\\.([0-9]*))?(?:[ \t]*(Z|([-+][0-9][0-9]?)(?::([0-9][0-9])?)?)))?$");
        internal static Regex YMD_REGEXP = new Regex("^(-?[0-9][0-9][0-9][0-9])-([0-9][0-9]?)-([0-9][0-9]?)$");

        public static object ConstructYamlTimestampYMD(IConstructor ctor, Node node) {
            ScalarNode scalar = node as ScalarNode;
            if (scalar == null) {
                throw new ConstructorException("can only contruct timestamp from scalar node");
            }

            Match match = YMD_REGEXP.Match(scalar.Value);
            if (match.Success) {
                int year_ymd = int.Parse(match.Groups[1].Value);
                int month_ymd = int.Parse(match.Groups[2].Value);
                int day_ymd = int.Parse(match.Groups[3].Value);

                return new DateTime(year_ymd, month_ymd, day_ymd);
            }
            throw new ConstructorException("Invalid tag:yaml.org,2002:timestamp#ymd value.");
        }

        public static object ConstructYamlTimestamp(IConstructor ctor, Node node) {
            ScalarNode scalar = node as ScalarNode;
            if (scalar == null) {
                throw new ConstructorException("can only contruct timestamp from scalar node");
            }
            
            Match match = TIMESTAMP_REGEXP.Match(scalar.Value);

            if (!match.Success) {
                return ctor.ConstructPrivateType(node);
            }

            string year_s = match.Groups[1].Value;
            string month_s = match.Groups[2].Value;
            string day_s = match.Groups[3].Value;
            string hour_s = match.Groups[4].Value;
            string min_s = match.Groups[5].Value;
            string sec_s = match.Groups[6].Value;
            string fract_s = match.Groups[7].Value;
            string utc = match.Groups[8].Value;
            string timezoneh_s = match.Groups[9].Value;
            string timezonem_s = match.Groups[10].Value;

            bool isUtc = utc == "Z" || utc == "z";

            DateTime dt = new DateTime(
                year_s != "" ? int.Parse(year_s) : 0,
                month_s != "" ? int.Parse(month_s) : 1,
                day_s != "" ? int.Parse(day_s) : 1,
                hour_s != "" ? int.Parse(hour_s) : 0,
                min_s != "" ? int.Parse(min_s) : 0,
                sec_s != "" ? int.Parse(sec_s) : 0,
                isUtc? DateTimeKind.Utc : DateTimeKind.Local
            );

            if (!string.IsNullOrEmpty(fract_s)) {
                long fract = int.Parse(fract_s);
                if (fract > 0) {
                    while (fract < 1000000) {
                        fract *= 10;
                    }
                    dt = dt.AddTicks(fract);
                }
            }

            if (!isUtc) {
                if (timezoneh_s != "" || timezonem_s != "") {
                    int zone = 0;
                    int sign = +1;
                    if (timezoneh_s != "") {
                        if (timezoneh_s.StartsWith("-")) {
                            sign = -1;
                        }
                        zone += int.Parse(timezoneh_s.Substring(1)) * 3600000;
                    }
                    if (timezonem_s != "") {
                        zone += int.Parse(timezonem_s) * 60000;
                    }
                    double utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(dt).TotalMilliseconds;
                    dt = dt.AddMilliseconds(utcOffset - sign * zone);
                }
            }
            return dt;
        }

        public static object ConstructYamlInt(IConstructor ctor, Node node) {
            string value = ctor.ConstructScalar(node).ToString().Replace("_","").Replace(",","");
            int sign = +1;
            char first = value[0];
            if(first == '-') {
                sign = -1;
                value = value.Substring(1);
            } else if(first == '+') {
                value = value.Substring(1);
            }
            int @base = 10;
            if (value == "0") {
                return 0;
            } else if (value.StartsWith("0b")) {
                value = value.Substring(2);
                @base = 2;
            } else if (value.StartsWith("0x")) {
                value = value.Substring(2);
                @base = 16;
            } else if (value.StartsWith("0")) {
                value = value.Substring(1);
                @base = 8;
            } else if (value.IndexOf(':') != -1) {
                string[] digits = value.Split(':');
                int bes = 1;
                int val = 0;
                for (int i = 0, j = digits.Length; i < j; i++) {
                    val += (int.Parse(digits[(j - i) - 1]) * bes);
                    bes *= 60;
                }
                return sign*val;
            }

            try {
                // LiteralParser.ParseInteger delegate handles parsing & conversion to BigInteger (if needed)
                return LiteralParser.ParseInteger(sign, value, @base);
            } catch (Exception e) {
                throw new ConstructorException(string.Format("Could not parse integer value: '{0}' (sign {1}, base {2})", value, sign, @base), e);
            }
        }

        public static object ConstructYamlFloat(IConstructor ctor, Node node) {
            string value = ctor.ConstructScalar(node).ToString().Replace("_", "").Replace(",", "");
            int sign = +1;
            char first = value[0];
            if (first == '-') {
                sign = -1;
                value = value.Substring(1);
            } else if (first == '+') {
                value = value.Substring(1);
            }
            string valLower = value.ToLower();
            if (valLower == ".inf") {
                return sign == -1 ? double.NegativeInfinity : double.PositiveInfinity;
            } else if (valLower == ".nan") {
                return double.NaN;
            } else if (value.IndexOf(':') != -1) {
                string[] digits = value.Split(':');
                int bes = 1;
                double val = 0.0;
                for (int i = 0, j = digits.Length; i < j; i++) {
                    val += (double.Parse(digits[(j - i) - 1]) * bes);
                    bes *= 60;
                }
                return sign * val;
            } else {
                return sign * double.Parse(value);
            }
        }

        public static byte[] ConstructYamlBinary(IConstructor ctor, Node node) {
            string val = ctor.ConstructScalar(node).ToString().Replace("\r", "").Replace("\n", "");
            return Convert.FromBase64String(val);
        }

        public static object ConstructSpecializedSequence(IConstructor ctor, string pref, Node node) {
            RubyArray result = null;
            try {
                result = (RubyArray)Type.GetType(pref).GetConstructor(Type.EmptyTypes).Invoke(null);
            } catch (Exception e) {
                throw new ConstructorException("Can't construct a sequence from class: " + pref, e);
            }
            foreach (object x in ctor.ConstructSequence(node)) {
                result.Add(x);
            }
            return result;
        }

        public static object ConstructSpecializedMap(IConstructor ctor, string pref, Node node) {
            Hash result = null;
            try {
                result = (Hash)Type.GetType(pref).GetConstructor(Type.EmptyTypes).Invoke(null);
            } catch (Exception e) {
                throw new ConstructorException("Can't construct a mapping from class: " + pref, e);
            }
            foreach (KeyValuePair<object, object> e in ctor.ConstructMapping(node)) {
                result.Add(e.Key, e.Value);
            }
            return result;
        }

        public static object ConstructCliObject(IConstructor ctor, string pref, Node node) {
            // TODO: should this use serialization or some more standard CLR mechanism?
            //       (it is very ad-hoc)
            // TODO: use DLR APIs instead of reflection
            try {
                Type type = Type.GetType(pref);
                object result = type.GetConstructor(Type.EmptyTypes).Invoke(null);

                foreach (KeyValuePair<object, object> e in ctor.ConstructMapping(node)) {
                    string name = e.Key.ToString();
                    name = "" + char.ToUpper(name[0]) + name.Substring(1);
                    PropertyInfo prop = type.GetProperty(name);

                    prop.SetValue(result, Convert.ChangeType(e.Value, prop.PropertyType), null);
                }
                return result;

            } catch (Exception e) {
                throw new ConstructorException("Can't construct a CLI object from class: " + pref, e);
            }
        }

        static SafeConstructor() {
            BOOL_VALUES.Add("yes", true);
            BOOL_VALUES.Add("Yes", true);
            BOOL_VALUES.Add("YES", true);
            BOOL_VALUES.Add("no", false);
            BOOL_VALUES.Add("No", false);
            BOOL_VALUES.Add("NO", false);
            BOOL_VALUES.Add("true", true);
            BOOL_VALUES.Add("True", true);
            BOOL_VALUES.Add("TRUE", true);
            BOOL_VALUES.Add("false", false);
            BOOL_VALUES.Add("False", false);
            BOOL_VALUES.Add("FALSE", false);
            BOOL_VALUES.Add("on", true);
            BOOL_VALUES.Add("On", true);
            BOOL_VALUES.Add("ON", true);
            BOOL_VALUES.Add("off", false);
            BOOL_VALUES.Add("Off", false);
            BOOL_VALUES.Add("OFF", false);

            BaseConstructor.AddConstructor("tag:yaml.org,2002:null", ConstructYamlNull);
            AddConstructor("tag:yaml.org,2002:bool", ConstructYamlBool);
            AddConstructor("tag:yaml.org,2002:omap", ConstructYamlOmap);
            AddConstructor("tag:yaml.org,2002:pairs", ConstructYamlPairs);
            AddConstructor("tag:yaml.org,2002:set", ConstructYamlSet);
            AddConstructor("tag:yaml.org,2002:int", ConstructYamlInt);
            AddConstructor("tag:yaml.org,2002:float", ConstructYamlFloat);
            AddConstructor("tag:yaml.org,2002:timestamp", ConstructYamlTimestamp);
            AddConstructor("tag:yaml.org,2002:timestamp#ymd", ConstructYamlTimestampYMD);
            AddConstructor("tag:yaml.org,2002:str", ConstructYamlStr);
            AddConstructor("tag:yaml.org,2002:binary", ConstructYamlBinary);
            AddConstructor("tag:yaml.org,2002:seq", ConstructYamlSeq);
            AddConstructor("tag:yaml.org,2002:map", ConstructYamlMap);            
            AddConstructor("", BaseConstructor.CONSTRUCT_PRIVATE);
            AddMultiConstructor("tag:yaml.org,2002:seq:", ConstructSpecializedSequence);
            AddMultiConstructor("tag:yaml.org,2002:map:", ConstructSpecializedMap);
            AddMultiConstructor("!cli/object:", ConstructCliObject);
            AddMultiConstructor("tag:cli.yaml.org,2002:object:", ConstructCliObject);
        }

    }
}