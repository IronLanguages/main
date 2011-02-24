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
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Text;
using Microsoft.Scripting.Math;
using IronRuby.Runtime;
using SM = System.Math;
using IronRuby.Runtime.Calls;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Generation;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Builtins {

    public sealed class StringFormatterSiteStorage : RubyCallSiteStorage {
        private CallSite<Func<CallSite, object, int>> _fixnumCast;
        private CallSite<Func<CallSite, object, double>> _tofConversion;
        private CallSite<Func<CallSite, object, MutableString>> _tosConversion;
        private CallSite<Func<CallSite, object, IntegerValue>> _integerConversion;

        [Emitted]
        public StringFormatterSiteStorage(RubyContext/*!*/ context) : base(context) {
        }

        public int CastToFixnum(object value) {
            var site = RubyUtils.GetCallSite(ref _fixnumCast, ConvertToFixnumAction.Make(Context));
            return site.Target(site, value);
        }

        public double CastToDouble(object value) {
            var site = RubyUtils.GetCallSite(ref _tofConversion, ConvertToFAction.Make(Context));
            return site.Target(site, value);
        }

        public MutableString/*!*/ ConvertToString(object value) {
            var site = RubyUtils.GetCallSite(ref _tosConversion, ConvertToSAction.Make(Context));
            return site.Target(site, value);
        }

        public IntegerValue ConvertToInteger(object value) {
            var site = RubyUtils.GetCallSite(ref _integerConversion, CompositeConversionAction.Make(Context, CompositeConversion.ToIntToI));
            return site.Target(site, value);
        }
    }

    /// <summary>
    /// StringFormatter provides Ruby's sprintf style string formatting services.
    /// 
    /// TODO: Many dynamic languages have similar printf style functionality.
    ///       Combine this with IronPython's StringFormatter and move the common code into the DLR
    /// 
    /// TODO: Support negative numbers for %u and %o and %x
    /// </summary>
    internal sealed class StringFormatter {      

        // This is a ThreadStatic since so that formatting operations on one thread do not interfere with other threads
        [ThreadStatic]
        private static NumberFormatInfo NumberFormatInfoForThread;

        private static NumberFormatInfo nfi {
            get {
                if (NumberFormatInfoForThread == null) {
                    NumberFormatInfo numberFormatInfo = new CultureInfo("en-US").NumberFormat;
                    // The CLI formats as "Infinity", but Ruby formats differently:
                    //   sprintf("%f", 1.0/0) => "Inf"
                    //   sprintf("%f", -1.0/0) => "-Inf"
                    //   sprintf("%f", 0.0/0) => "Nan"
                    numberFormatInfo.PositiveInfinitySymbol = "Infinity";
                    numberFormatInfo.NegativeInfinitySymbol = "-Infinity";
                    numberFormatInfo.NaNSymbol = "NaN";
                    NumberFormatInfoForThread = numberFormatInfo;
                }
                return NumberFormatInfoForThread;
            }
        }

        public bool TrailingZeroAfterWholeFloat {
            get { return _TrailingZeroAfterWholeFloat; }
            set { _TrailingZeroAfterWholeFloat = value; }
        }

        const int UnspecifiedPrecision = -1; // Use the default precision

        private readonly IList/*!*/ _data;
        private readonly string/*!*/ _format;
        private readonly RubyContext/*!*/ _context;

        private bool? _useAbsolute;
        private int _relativeIndex;
        private bool _tainted;

        private int _index;
        private char _curCh;

        // The options for formatting the current formatting specifier in the format string
        private FormatSettings _opts;
        // Should ddd.0 be displayed as "ddd" or "ddd.0". "'%g' % ddd.0" needs "ddd", but str(ddd.0) needs "ddd.0"
        private bool _TrailingZeroAfterWholeFloat;

        /// TODO: Use MutableString instead of StringBuilder for building the string + encodings
        private StringBuilder _buf;

        // TODO (encoding):
        private readonly RubyEncoding/*!*/ _encoding;

        private readonly StringFormatterSiteStorage/*!*/ _siteStorage;

        #region Constructors

        // TODO: remove
        internal StringFormatter(RubyContext/*!*/ context, string/*!*/ format, RubyEncoding/*!*/ encoding, IList/*!*/ data) {
            Assert.NotNull(context, format, data, encoding);

            _context = context;
            _format = format;
            _data = data;

            // TODO (encoding):
            _encoding = encoding;
        }

        internal StringFormatter(StringFormatterSiteStorage/*!*/ siteStorage, string/*!*/ format, RubyEncoding/*!*/ encoding, IList/*!*/ data)
            : this(siteStorage.Context, format, encoding, data) {
            Assert.NotNull(siteStorage);
            _siteStorage = siteStorage;
        }

        #endregion

        #region Public API Surface

        public MutableString/*!*/ Format() {
            _index = 0;
            _buf = new StringBuilder();
            _tainted = false;
            int modIndex;

            while ((modIndex = _format.IndexOf('%', _index)) != -1) {
                _buf.Append(_format, _index, modIndex - _index);
                _index = modIndex + 1;
                DoFormatCode();
            }

            if (_context.DomainManager.Configuration.DebugMode) {
                if ((!_useAbsolute.HasValue || !_useAbsolute.Value) && _relativeIndex != _data.Count) {
                    throw RubyExceptions.CreateArgumentError("too many arguments for format string");
                }
            }

            _buf.Append(_format, _index, _format.Length - _index);

            MutableString result = MutableString.Create(_buf.ToString(), _encoding);

            if (_tainted) {
                result.IsTainted = true;
            }

            return result;
        }

        #endregion

        #region Private APIs

        private void DoFormatCode() {
            // we already pulled the first %

            if (_index == _format.Length || _format[_index] == '\n' || _format[_index] == '\0') {
                // '%' at the end of the string. Just print it and we are done.
                _buf.Append('%');
                return;
            }

            _curCh = _format[_index++];

            if (_curCh == '%') {
                // Escaped '%' character using "%%". Just print it and we are done
                _buf.Append('%');
                return;
            }

            _opts = new FormatSettings();

            ReadConversionFlags();

            ReadArgumentIndex(); // This can be before or after width and precision

            ReadMinimumFieldWidth();

            ReadPrecision();

            ReadArgumentIndex(); // This can be before or after width and precision

            _opts.Value = GetData(_opts.ArgIndex);

            WriteConversion();
        }

        private void ReadConversionFlags() {
            while(true) {
                switch (_curCh) {
                    case '#': _opts.AltForm = true; break;
                    case '-': _opts.LeftAdj = true; _opts.ZeroPad = false; break;
                    case '0': if (!_opts.LeftAdj) _opts.ZeroPad = true; break;
                    case '+': _opts.SignChar = true; _opts.Space = false; break;
                    case ' ': if (!_opts.SignChar) _opts.Space = true; break;
                    default:
                        return;
                }

                if (_index >= _format.Length) {
                    throw RubyExceptions.CreateArgumentError("illegal format character - %");
                }

                _curCh = _format[_index++];
            }
        }

        private void ReadArgumentIndex() {
            int? argIndex = TryReadArgumentIndex();
            if (argIndex.HasValue) {
                if (_opts.ArgIndex.HasValue) {
                    RubyExceptions.CreateArgumentError("value given twice");
                }
                _opts.ArgIndex = argIndex;
            }
        }

        private int? TryReadArgumentIndex() {
            if (char.IsDigit(_curCh)) {
                int end = _index;
                while (end < _format.Length && char.IsDigit(_format[end])) {
                    end++;
                }
                if (end < _format.Length && _format[end] == '$') {
                    int argIndex = int.Parse(_format.Substring(_index - 1, end - _index + 1), CultureInfo.InvariantCulture);
                    _index = end + 1; // Point past the '$'
                    if (_index < _format.Length) {
                        _curCh = _format[_index++];
                        return argIndex;
                    }
                }
            }
            return null;
        }

        private int ReadNumberOrStar() {
            int res = 0; // default value
            if (_curCh == '*') {
                _curCh = _format[_index++]; // Skip the '*'
                int? argindex = TryReadArgumentIndex();

                res = _siteStorage.CastToFixnum(GetData(argindex));
                if (res < 0) {
                    _opts.LeftAdj = true;
                    res = -res;
                }
            } else {
                if (Char.IsDigit(_curCh)) {
                    res = 0;
                    while (Char.IsDigit(_curCh) && _index < this._format.Length) {
                        res = res * 10 + ((int)(_curCh - '0'));
                        _curCh = _format[_index++];
                    }
                }
            }
            return res;
        }

        private void ReadMinimumFieldWidth() {
            _opts.FieldWidth = ReadNumberOrStar();
            if (_opts.FieldWidth == Int32.MaxValue) {
                // TODO: this should be thrown by the converter
                throw RubyExceptions.CreateRangeError("bignum too big to convert into `long'");
            }
        }

        private void ReadPrecision() {
            if (_curCh == '.') {
                _curCh = _format[_index++];
                // possibility: "8.f", "8.0f", or "8.2f"
                _opts.Precision = ReadNumberOrStar();
            } else {
                _opts.Precision = UnspecifiedPrecision;
            }
        }

        private void WriteConversion() {
            // conversion type (required)
            switch (_curCh) {
                // binary number
                case 'b':
                case 'B': AppendBinary(_curCh); return;
                // single character (int or single char str)
                case 'c': AppendChar(); return;
                // signed integer decimal
                case 'd':
                case 'i': AppendInt('D'); return;
                // floating point exponential format 
                case 'e':
                case 'E':
                // floating point decimal
                case 'f':
                // Same as "e" if exponent is less than -4 or more than precision, "f" otherwise.
                case 'G':
                case 'g': AppendFloat(_curCh); return;
                // unsigned octal
                case 'o': AppendOctal(); return;
                // call inspect on argument
                case 'p': AppendInspect(); return;
                // string
                case 's': AppendString(); return;
                // unsigned decimal
                case 'u': AppendInt(_curCh); return;
                // unsigned hexadecimal
                case 'x':
                case 'X': AppendHex(_curCh); return;
                default: throw RubyExceptions.CreateArgumentError("malformed format string - %" + _curCh);
            }
        }

        private object GetData(int? absoluteIndex) {
            if (_useAbsolute.HasValue) {
                // All arguments must use absolute or relative index. They can't be mixed
                if (_useAbsolute.Value && !absoluteIndex.HasValue) {
                    throw RubyExceptions.CreateArgumentError("unnumbered({0}) mixed with numbered", _relativeIndex + 1);
                } else if (!_useAbsolute.Value && absoluteIndex.HasValue) {
                    throw RubyExceptions.CreateArgumentError("numbered({0}) after unnumbered({1})", absoluteIndex.Value, _relativeIndex + 1);
                }
            } else {
                // First time through, set _useAbsolute based on our current value
                _useAbsolute = absoluteIndex.HasValue;
            }

            int index = _useAbsolute.Value ? (absoluteIndex.Value - 1) : _relativeIndex++;
            if (index < _data.Count) {
                return _data[index];
            }

            throw RubyExceptions.CreateArgumentError("too few arguments");
        }

        // TODO: encodings
        private void AppendChar() {
            int value = _siteStorage.CastToFixnum(_opts.Value);
            if (value < 0 && _context.RubyOptions.Compatibility >= RubyCompatibility.Ruby19) {
                throw RubyExceptions.CreateArgumentError("invalid character: {0}", value);
            }

            char c = (char)(value & 0xff);
            if (_opts.FieldWidth > 1) {
                if (!_opts.LeftAdj) {
                    _buf.Append(' ', _opts.FieldWidth - 1);
                }
                _buf.Append(c);
                if (_opts.LeftAdj) {
                    _buf.Append(' ', _opts.FieldWidth - 1);
                }
            } else {
                _buf.Append(c);
            }
        }

        private void AppendInt(char format) {
            IntegerValue integer = (_opts.Value == null) ? 0 : _siteStorage.ConvertToInteger(_opts.Value);

            object val;
            bool isPositive;
            if (integer.IsFixnum) {
                isPositive = integer.Fixnum >= 0;
                val = integer.Fixnum;
            } else {
                isPositive = integer.Bignum.IsZero() || integer.Bignum.IsPositive();
                val = integer.Bignum;
            }

            if (_opts.LeftAdj) {
                AppendLeftAdj(val, isPositive, 'D');
            } else if (_opts.ZeroPad) {
                AppendZeroPad(val, isPositive, 'D');
            } else {
                AppendNumeric(val, isPositive, 'D', format == 'u');
            }
        }

        private static readonly char[] zero = new char[] { '0' };

        // Return the new type char to use
        // opts.Precision will be set to the nubmer of digits to display after the decimal point
        private char AdjustForG(char type, double v) {
            if (type != 'G' && type != 'g')
                return type;
            if (Double.IsNaN(v) || Double.IsInfinity(v))
                return type;

            double absV = SM.Abs(v);

            if ((v != 0.0) && // 0.0 should not be displayed as scientific notation
                absV < 1e-4 || // Values less than 0.0001 will need scientific notation
                absV >= SM.Pow(10, _opts.Precision)) { // Values bigger than 1e<precision> will need scientific notation

                // One digit is displayed before the decimal point. Hence, we need one fewer than the precision after the decimal point
                int fractionDigitsRequired = (_opts.Precision - 1);
                string expForm = absV.ToString("E" + fractionDigitsRequired, CultureInfo.InvariantCulture);
                string mantissa = expForm.Substring(0, expForm.IndexOf('E')).TrimEnd(zero);

                // We do -2 to ignore the digit before the decimal point and the decimal point itself
                Debug.Assert(mantissa[1] == '.');
                _opts.Precision = mantissa.Length - 2;

                type = (type == 'G') ? 'E' : 'e';
            } else {
                // "0.000ddddd" is allowed when the precision is 5. The 3 leading zeros are not counted
                int numberDecimalDigits = _opts.Precision;
                if (absV < 1e-3) numberDecimalDigits += 3;
                else if (absV < 1e-2) numberDecimalDigits += 2;
                else if (absV < 1e-1) numberDecimalDigits += 1;

                string fixedPointForm = absV.ToString("F" + numberDecimalDigits, CultureInfo.InvariantCulture).TrimEnd(zero);
                string fraction = fixedPointForm.Substring(fixedPointForm.IndexOf('.') + 1);
                if (absV < 1.0) {
                    _opts.Precision = fraction.Length;
                } else {
                    int digitsBeforeDecimalPoint = 1 + (int)SM.Log10(absV);
                    _opts.Precision = SM.Min(_opts.Precision - digitsBeforeDecimalPoint, fraction.Length);
                }

                type = 'f';
            }

            return type;
        }

        private void AppendFloat(char type) {
            double v;
            if (_siteStorage != null) {
                v = _siteStorage.CastToDouble(_opts.Value);
            } else {
                v = (double)_opts.Value;
            }

            // scientific exponential format 
            Debug.Assert(type == 'E' || type == 'e' ||
                // floating point decimal
                         type == 'f' ||
                // Same as "e" if exponent is less than -4 or more than precision, "f" otherwise.
                         type == 'G' || type == 'g');

            bool forceDot = false;
            // update our precision first...
            if (_opts.Precision != UnspecifiedPrecision) {
                if (_opts.Precision == 0 && _opts.AltForm) forceDot = true;
                if (_opts.Precision > 50)
                    _opts.Precision = 50;
            } else {
                // alternate form (#) specified, set precision to zero...
                if (_opts.AltForm) {
                    _opts.Precision = 0;
                    forceDot = true;
                } else _opts.Precision = 6;
            }

            type = AdjustForG(type, v);
            nfi.NumberDecimalDigits = _opts.Precision;

            // then append
            if (_opts.LeftAdj) {
                AppendLeftAdj(v, v >= 0, type);
            } else if (_opts.ZeroPad) {
                AppendZeroPadFloat(v, type);
            } else {
                AppendNumeric(v, v >= 0, type, false);
            }
            if (v <= 0 && v > -1 && _buf[0] != '-') {
                FixupFloatMinus(v);
            }

            if (forceDot) {
                FixupAltFormDot();
            }
        }

        private void FixupAltFormDot() {
            _buf.Append('.');
            if (_opts.FieldWidth != 0) {
                // try and remove the extra character we're adding.
                for (int i = 0; i < _buf.Length; i++) {
                    char c = _buf[i];
                    if (c == ' ' || c == '0') {
                        _buf.Remove(i, 1);
                        break;
                    } else if (c != '-' && c != '+') {
                        break;
                    }
                }
            }
        }

        private void FixupFloatMinus(double value) {
            // Ruby always appends a minus sign even if precision is 0 and the value would appear to be zero.
            //   sprintf("%.0f", -0.1) => "-0"
            // Ruby also displays a "-0.0" for a negative zero whereas the CLR displays just "0.0"
            bool fNeedMinus;
            if (value == 0.0) {
                fNeedMinus = MathUtils.IsNegativeZero(value);
            } else {
                fNeedMinus = true;
                for (int i = 0; i < _buf.Length; i++) {
                    char c = _buf[i];
                    if (c != '.' && c != '0' && c != ' ') {
                        fNeedMinus = false;
                        break;
                    }
                }
            }

            if (fNeedMinus) {
                if (_opts.FieldWidth != 0) {
                    // trim us back down to the correct field width...
                    if (_buf[_buf.Length - 1] == ' ') {
                        _buf.Insert(0, "-");
                        _buf.Remove(_buf.Length - 1, 1);
                    } else {
                        int index = 0;
                        while (_buf[index] == ' ') index++;
                        if (index > 0) index--;
                        _buf[index] = '-';
                    }
                } else {
                    _buf.Insert(0, "-");
                }
            }
        }

        private void AppendZeroPad(object val, bool fPos, char format) {
            if (fPos && (_opts.SignChar || _opts.Space)) {
                // produce [' '|'+']0000digits
                // first get 0 padded number to field width
                string res = String.Format(nfi, "{0:" + format + _opts.FieldWidth.ToString(CultureInfo.InvariantCulture) + "}", val);

                char signOrSpace = _opts.SignChar ? '+' : ' ';
                // then if we ended up with a leading zero replace it, otherwise
                // append the space / + to the front.
                if (res[0] == '0' && res.Length > 1) {
                    res = signOrSpace + res.Substring(1);
                } else {
                    res = signOrSpace + res;
                }
                _buf.Append(res);
            } else {
                string res = String.Format(nfi, "{0:" + format + _opts.FieldWidth.ToString(CultureInfo.InvariantCulture) + "}", val);

                // Difference: 
                //   System.String.Format("{0:D3}", -1)      '-001'
                //   "%03d" % -1                             '-01'

                if (res[0] == '-') {
                    // negative
                    _buf.Append("-");
                    if (res[1] != '0') {
                        _buf.Append(res.Substring(1));
                    } else {
                        _buf.Append(res.Substring(2));
                    }
                } else {
                    // positive
                    _buf.Append(res);
                }
            }
        }

        private void AppendZeroPadFloat(double val, char format) {
            if (val >= 0) {
                StringBuilder res = new StringBuilder(val.ToString(format.ToString(), nfi));
                if (res.Length < _opts.FieldWidth) {
                    res.Insert(0, new string('0', _opts.FieldWidth - res.Length));
                }
                if (_opts.SignChar || _opts.Space) {
                    char signOrSpace = _opts.SignChar ? '+' : ' ';
                    // then if we ended up with a leading zero replace it, otherwise
                    // append the space / + to the front.
                    if (res[0] == '0' && res[1] != '.') {
                        res[0] = signOrSpace;
                    } else {
                        res.Insert(0, signOrSpace.ToString());
                    }
                }
                _buf.Append(res);
            } else {
                StringBuilder res = new StringBuilder(val.ToString(format.ToString(), nfi));
                if (res.Length < _opts.FieldWidth) {
                    res.Insert(1, new string('0', _opts.FieldWidth - res.Length));
                }
                _buf.Append(res);
            }
        }

        private void AppendNumeric(object val, bool fPos, char format, bool unsigned) {
            bool isNegative = false;

            if (val is BigInteger && ((BigInteger)val).Sign == -1)
                isNegative = true;
            else if (val is int && (int)val < 0)
                isNegative = true;
            else if (val is float && (float)val < 0)
                isNegative = true;

            if (isNegative && unsigned) {
                val = val is BigInteger ? CastToUnsignedBigInteger(val as BigInteger) : (object)(uint)(int)val;
            }

            if (fPos && (_opts.SignChar || _opts.Space)) {
                string strval = (_opts.SignChar ? "+" : " ") + String.Format(nfi, "{0:" + format.ToString() + "}", val);
                if (strval.Length < _opts.FieldWidth) {
                    _buf.Append(' ', _opts.FieldWidth - strval.Length);
                }
                _buf.Append(strval);
            } else if (_opts.Precision == UnspecifiedPrecision) {
                _buf.AppendFormat(nfi, "{0," + _opts.FieldWidth.ToString(CultureInfo.InvariantCulture) + ":" + format + "}", val);
                if (unsigned && isNegative)
                    _buf.Insert(0, "..");
            } else if (_opts.Precision < 100) {
                //CLR formatting has a maximum precision of 100.
                _buf.AppendFormat(nfi, "{0," + _opts.FieldWidth.ToString(CultureInfo.InvariantCulture) + ":" + format + _opts.Precision.ToString(CultureInfo.InvariantCulture) + "}", val);
            } else {
                StringBuilder res = new StringBuilder();
                res.AppendFormat("{0:" + format + "}", val);
                if (res.Length < _opts.Precision) {
                    char padding = unsigned ? '.' : '0';
                    res.Insert(0, new String(padding, _opts.Precision - res.Length));
                }
                if (res.Length < _opts.FieldWidth) {
                    res.Insert(0, new String(' ', _opts.FieldWidth - res.Length));
                }
                _buf.Append(res.ToString());
            }

            // If AdjustForG() sets opts.Precision == 0, it means that no significant digits should be displayed after
            // the decimal point. ie. 123.4 should be displayed as "123", not "123.4". However, we might still need a 
            // decorative ".0". ie. to display "123.0"
            if (_TrailingZeroAfterWholeFloat && (format == 'f') && _opts.Precision == 0)
                _buf.Append(".0");
        }

        private void AppendLeftAdj(object val, bool fPos, char type) {
            string str = String.Format(nfi, "{0:" + type.ToString() + "}", val);
            if (fPos) {
                if (_opts.SignChar) str = '+' + str;
                else if (_opts.Space) str = ' ' + str;
            }
            _buf.Append(str);
            if (str.Length < _opts.FieldWidth) _buf.Append(' ', _opts.FieldWidth - str.Length);
        }

        private static bool NeedsAltForm(char format, char last) {
            if (format == 'X' || format == 'x') return true;

            if (last == '0') return false;
            return true;
        }

        // Note backwards formats
        private static string GetAltFormPrefixForRadix(char format, int radix) {
            switch (radix) {
                case 2:
                    return format == 'b' ? "b0" : "B0";
                case 8: return "0";
                case 16: return format + "0";
            }
            return "";
        }

        private static uint[] _Mask = new uint[] { 0x0, 0x1, 0x0, 0x7, 0xF };
        private static char[] _UpperDigits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        private static char[] _LowerDigits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        private StringBuilder/*!*/ AppendBase(object/*!*/ value, int bitsToShift, bool lowerCase) {
            if (value is BigInteger)
                return AppendBaseBigInteger(value as BigInteger, bitsToShift, lowerCase);

            StringBuilder/*!*/ result = new StringBuilder();
            bool isNegative = IsNegative(value);
            uint val = unchecked((uint)(int)value);
            uint limit = isNegative ? 0xFFFFFFFF : 0;
            uint mask = _Mask[bitsToShift];
            char[] digits = lowerCase ? _LowerDigits : _UpperDigits;

            if (IsZero(value)) {
                result.Append(digits[0]);
                return result;
            }

            while (val != limit) {
                result.Append(digits[val & mask]);
                val = val >> bitsToShift;
                limit = limit >> bitsToShift;
            }

            if (isNegative)
                result.Append(digits[mask]);

            return result;
        }

        private StringBuilder/*!*/ AppendBaseInt(int value, int radix) {
            StringBuilder/*!*/ str = new StringBuilder();

            if (value == 0) str.Append('0');
            while (value != 0) {
                int digit = value % radix;
                str.Append(_LowerDigits[digit]);
                value /= radix;
            }
            return str;
        }

        private StringBuilder/*!*/ AppendBaseUnsignedInt(uint value, uint radix) {
            StringBuilder/*!*/ str = new StringBuilder();

            if (value == 0) str.Append('0');
            while (value != 0) {
                uint digit = value % radix;
                str.Append(_LowerDigits[digit]);
                value /= radix;
            }
            return str;
        }

        private StringBuilder/*!*/ AppendBase2(object/*!*/ value, int radix, bool unsigned) {
            if (value is BigInteger)
                return AppendBaseBigInteger(value as BigInteger, radix);

            if (unsigned)
                return AppendBaseInt((int)value, radix);
            else
                return AppendBaseUnsignedInt((uint)value, (uint)radix);
        }

        private StringBuilder/*!*/ AppendBaseBigInteger(BigInteger/*!*/ value, int radix) {
            StringBuilder/*!*/ str = new StringBuilder();
            if (value == 0) str.Append('0');
            while (value != 0) {
                int digit = (int)(value % radix);
                str.Append(_LowerDigits[digit]);
                value /= radix;
            }
            return str;
        }

        private BigInteger/*!*/ MakeBigIntegerFromByteArray(byte[] bytes) {
            uint[] data = new uint[(bytes.Length / 4) + 1];

            int j = 0;
            for (int i = 0; i < bytes.Length; i += 4) {
                uint word = 0;
                int diff = bytes.Length - i;
                if (diff > 3) {
                    word = (uint)bytes[i] | (uint)(bytes[i + 1] << 8) | (uint)(bytes[i + 2] << 16) | (uint)((uint)bytes[i + 3] << 24);
                } else if (diff == 3) {
                    word = (uint)bytes[i] | (uint)(bytes[i + 1] << 8) | (uint)(bytes[i + 2] << 16);
                } else if (diff == 2) {
                    word = (uint)bytes[i] | (uint)(bytes[i + 1] << 8);
                } else if (diff == 1) {
                    word = (uint)bytes[i];
                }
                data[j++] = word;
            }

            return new BigInteger(1, data);
        }

        private BigInteger/*!*/ CastToUnsignedBigInteger(BigInteger/*!*/ value) {
            return MakeBigIntegerFromByteArray(value.ToByteArray());
        }

        private BigInteger/*!*/ GenerateMask(BigInteger/*!*/ value) {
            byte[] bytes = new byte[value.ToByteArray().Length];
            for (int i = 0; i < bytes.Length; i++) {
                bytes[i] = 0xFF;
            }
            return MakeBigIntegerFromByteArray(bytes);
        }

        private StringBuilder/*!*/ AppendBaseBigInteger(BigInteger value, int bitsToShift, bool lowerCase) {
            StringBuilder/*!*/ result = new StringBuilder();
            bool isNegative = value.Sign == -1;
            BigInteger/*!*/ val = CastToUnsignedBigInteger(value);
            BigInteger/*!*/ limit = isNegative ? GenerateMask(value) : BigInteger.Zero;
            uint mask = _Mask[bitsToShift];
            char[] digits = lowerCase ? _LowerDigits : _UpperDigits;

            while (val != limit) {
                result.Append(digits[(int)(val & mask)]);
                val >>= bitsToShift;
                limit >>= bitsToShift;
            }

            if (isNegative)
                result.Append(digits[mask]);

            return result;
        }

        private object/*!*/ Negate(object/*!*/ value) {
            if (value is BigInteger)
                return ((BigInteger)value).OnesComplement();
            else
                return -((int)value);
        }

        private bool IsZero(object/*!*/ value) {
            if (value is BigInteger)
                return ((BigInteger)value).IsZero();
            else
                return (int)value == 0;
        }

        private bool IsNegative(object/*!*/ value) {
            if (value is BigInteger)
                return ((BigInteger)value).Sign == -1;
            else
                return (int)value < 0;
        }

        /// <summary>
        /// AppendBase appends an integer at the specified radix doing all the
        /// special forms for Ruby.  We have a copy and paste version of this
        /// for BigInteger below that should be kept in sync.
        /// </summary>
        private void AppendBase(char format, int radix) {
            IntegerValue integer = (_opts.Value == null) ? 0 : _siteStorage.ConvertToInteger(_opts.Value);

            // TODO: split paths for bignum and fixnum
            object value = integer.IsFixnum ? (object)integer.Fixnum : (object)integer.Bignum;

            bool isNegative = IsNegative(value);
            if (isNegative) {
                // These options mean we're not looking at the one's complement
                if (_opts.Space || _opts.SignChar)
                    value = Negate(value);

                // if negative number, the leading space has no impact
                if (radix != 2 && radix != 8 && radix != 16)
                    _opts.Space = false;
            }

            // we build up the number backwards inside a string builder,
            // and after we've finished building this up we append the
            // string to our output buffer backwards.

            StringBuilder str;
            switch (radix) {
                case 2:
                    str = AppendBase(value, 1, true);
                    break;
                case 8:
                    str = AppendBase(value, 3, true);
                    break;
                case 16:
                    str = AppendBase(value, 4, format == 'x');
                    break;
                default:
                    str = AppendBase2(value, 10, format == 'u');
                    break;
            }

            // pad out for additional precision
            if (str.Length < _opts.Precision) {
                int len = _opts.Precision - str.Length;
                char padding = '0';
                if (radix == 2 && isNegative)
                    padding = '1';
                else if (radix == 8 && isNegative)
                    padding = '7';
                else if (radix == 16 && isNegative)
                    padding = format == 'x' ? 'f' : 'F';

                str.Append(padding, len);
            }

            // pad result to minimum field width
            if (_opts.FieldWidth != 0) {
                int signLen = (isNegative || _opts.SignChar) ? 1 : 0;
                int spaceLen = _opts.Space ? 1 : 0;
                int len = _opts.FieldWidth - (str.Length + signLen + spaceLen);

                if (len > 0) {
                    // we account for the size of the alternate form, if we'll end up adding it.
                    if (_opts.AltForm && NeedsAltForm(format, (!_opts.LeftAdj && _opts.ZeroPad) ? '0' : str[str.Length - 1])) {
                        len -= GetAltFormPrefixForRadix(format, radix).Length;
                    }

                    if (len > 0) {
                        // and finally append the right form
                        if (_opts.LeftAdj) {
                            str.Insert(0, " ", len);
                        } else {
                            if (_opts.ZeroPad) {
                                str.Append('0', len);
                            } else {
                                _buf.Append(' ', len);
                            }
                        }
                    }
                }
            }

            // append the alternate form
            if (_opts.AltForm && NeedsAltForm(format, str[str.Length - 1]))
                str.Append(GetAltFormPrefixForRadix(format, radix));

            // add any sign if necessary
            if (isNegative) {
                if (radix == 2 || radix == 8 || radix == 16) {
                    if (_opts.SignChar || _opts.Space)
                        _buf.Append('-');
                    else if (!_opts.ZeroPad && _opts.Precision == -1)
                        _buf.Append("..");
                } else {
                    _buf.Append("-");
                }
            } else if (_opts.SignChar) {
                _buf.Append('+');
            } else if (_opts.Space) {
                _buf.Append(' ');
            }

            // append the final value
            for (int i = str.Length - 1; i >= 0; i--) {
                _buf.Append(str[i]);
            }
        }

        private void AppendBinary(char format) {
            AppendBase(format, 2);
        }

        private void AppendHex(char format) {
            AppendBase(format, 16);
        }

        private void AppendOctal() {
            AppendBase('o', 8);
        }

        private void AppendInspect() {
            MutableString result = _context.Inspect(_opts.Value);
            if (KernelOps.Tainted(_context, result)) {
                _tainted = true;
            }

            AppendString(result);
        }

        private void AppendString() {
            MutableString/*!*/ str = _siteStorage.ConvertToString(_opts.Value);
            if (KernelOps.Tainted(_context, str)) {
                _tainted = true;
            }

            AppendString(str);
        }

        private void AppendString(MutableString/*!*/ mutable) {
            string str = mutable.ConvertToString();

            if (_opts.Precision != UnspecifiedPrecision && str.Length > _opts.Precision) {
                str = str.Substring(0, _opts.Precision);
            }

            if (!_opts.LeftAdj && _opts.FieldWidth > str.Length) {
                _buf.Append(' ', _opts.FieldWidth - str.Length);
            }

            _buf.Append(str);

            if (_opts.LeftAdj && _opts.FieldWidth > str.Length) {
                _buf.Append(' ', _opts.FieldWidth - str.Length);
            }
        }

        #endregion

        #region Private data structures

        // The conversion specifier format is as follows:
        //   % conversionFlags fieldWidth . precision conversionType
        // where:
        //   mappingKey - value to be formatted
        //   conversionFlags - # 0 - + * <space>
        //   conversionType - b c d E e f G g i o p s u X x %
        // Note:
        //   conversionFlags can also contain: number$
        //     where number is the index of the argument to get data from (uses 1-based indexing, so >= 1)
        //     This is called "abolute indexing", and if it's used anywhere it must be used everywhere.
        //     If absolute indexing is used, the "*" conversionFlag must be followed by number$ to indicate
        //     the (1-based) index of the argument containing the field width.
        // Ex:
        //   %#4o - Display argument as octal and prepend with leading 0 if necessary,
        //                   for a total of at least 4 characters

        [Flags]
        internal enum FormatOptions {
            ZeroPad = 0x01, // Use zero-padding to fit FieldWidth
            LeftAdj = 0x02, // Use left-adjustment to fit FieldWidth. Overrides ZeroPad
            AltForm = 0x04, // Add a leading 0 if necessary for octal, or add a leading 0x or 0X for hex
            Space = 0x08, // Leave a white-space
            SignChar = 0x10 // Force usage of a sign char even if the value is positive
        }

        internal struct FormatSettings {

            #region FormatOptions property accessors

            public bool ZeroPad {
                get {
                    return ((Options & FormatOptions.ZeroPad) != 0);
                }
                set {
                    if (value) {
                        Options |= FormatOptions.ZeroPad;
                    } else {
                        Options &= (~FormatOptions.ZeroPad);
                    }
                }
            }
            public bool LeftAdj {
                get {
                    return ((Options & FormatOptions.LeftAdj) != 0);
                }
                set {
                    if (value) {
                        Options |= FormatOptions.LeftAdj;
                    } else {
                        Options &= (~FormatOptions.LeftAdj);
                    }
                }
            }
            public bool AltForm {
                get {
                    return ((Options & FormatOptions.AltForm) != 0);
                }
                set {
                    if (value) {
                        Options |= FormatOptions.AltForm;
                    } else {
                        Options &= (~FormatOptions.AltForm);
                    }
                }
            }
            public bool Space {
                get {
                    return ((Options & FormatOptions.Space) != 0);
                }
                set {
                    if (value) {
                        Options |= FormatOptions.Space;
                    } else {
                        Options &= (~FormatOptions.Space);
                    }
                }
            }
            public bool SignChar {
                get {
                    return ((Options & FormatOptions.SignChar) != 0);
                }
                set {
                    if (value) {
                        Options |= FormatOptions.SignChar;
                    } else {
                        Options &= (~FormatOptions.SignChar);
                    }
                }
            }
            #endregion

            internal FormatOptions Options;

            // Minimum number of characters that the entire formatted string should occupy.
            // Smaller results will be left-padded with white-space or zeros depending on Options
            internal int FieldWidth;

            // Number of significant digits to display, before and after the decimal point.
            // For floats, it gets adjusted to the number of digits to display after the decimal point since
            // that is the value required by StringBuilder.AppendFormat.
            // For clarity, we should break this up into the two values - the precision specified by the
            // format string, and the value to be passed in to StringBuilder.AppendFormat
            internal int Precision;

            internal object Value;

            // If using absolute indexing, the index of the argument that has the data value
            internal int? ArgIndex;
        }
        #endregion
    }
}
