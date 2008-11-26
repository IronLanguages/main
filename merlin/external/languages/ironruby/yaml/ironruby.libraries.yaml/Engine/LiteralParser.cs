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
 * Copyright (c) Microsoft Corporation.
 * 
 ***** END LICENSE BLOCK *****/

using System;
using Microsoft.Scripting.Math;
using IronRuby.Compiler;

namespace IronRuby.StandardLibrary.Yaml {

    public delegate object ParseInteger(int sign, string digits, int @base);

    /// <summary>
    /// This must be hooked to allow big integer parsing
    /// </summary>
    public static class LiteralParser {
        /// <summary>
        /// Parses an integer/biginteger given sign, base, and digit string.
        /// The sign, base prefix, and numeric seperator characters are already stripped off
        /// </summary>
        public static ParseInteger ParseInteger = DefaultParseInteger;

        #region simple integer parsing

        public static object DefaultParseInteger(int sign, string digits, int @base) {
            int result;
            if (!TryParseInteger(sign, digits, @base, out result)) {
                return ParseBigInteger(sign, digits, @base);                
            }
            return result;
        }

        private static bool HexValue(char ch, out int value) {
            switch (ch) {
                case '0':
                case '\x660': value = 0; break;
                case '1':
                case '\x661': value = 1; break;
                case '2':
                case '\x662': value = 2; break;
                case '3':
                case '\x663': value = 3; break;
                case '4':
                case '\x664': value = 4; break;
                case '5':
                case '\x665': value = 5; break;
                case '6':
                case '\x666': value = 6; break;
                case '7':
                case '\x667': value = 7; break;
                case '8':
                case '\x668': value = 8; break;
                case '9':
                case '\x669': value = 9; break;
                case 'a':
                case 'A': value = 10; break;
                case 'b':
                case 'B': value = 11; break;
                case 'c':
                case 'C': value = 12; break;
                case 'd':
                case 'D': value = 13; break;
                case 'e':
                case 'E': value = 14; break;
                case 'f':
                case 'F': value = 15; break;
                default:
                    value = -1;
                    return false;
            }
            return true;
        }

        private static int HexValue(char ch) {
            int value;
            if (!HexValue(ch, out value)) {
                throw new ArgumentException("bad char for integer value: " + ch);
            }
            return value;
        }

        private static int CharValue(char ch, int b) {
            int val = HexValue(ch);
            if (val >= b) {
                throw new ArgumentException(String.Format("bad char for the integer value: '{0}' (base {1})", ch, b));
            }
            return val;
        }

        public static bool TryParseInteger(int sign, string text, int @base, out int ret) {
            ret = 0;
            long m = sign;
            for (int i = text.Length - 1; i >= 0; i--) {
                // avoid the exception here.  Not only is throwing it expensive,
                // but loading the resources for it is also expensive 
                long lret = (long)ret + m * CharValue(text[i], @base);
                if (Int32.MinValue <= lret && lret <= Int32.MaxValue) {
                    ret = (int)lret;
                } else {
                    return false;
                }

                m *= @base;
                if (Int32.MinValue > m || m > Int32.MaxValue) {
                    return false;
                }
            }
            return true;
        }

        public sealed class YamlBignumParser : UnsignedBigIntegerParser {
            private string _digits;
            private int _base;
            private int _position;

            public YamlBignumParser(string digits, int @base) {
                _digits = digits;
                _base = @base;
                _position = 0;
            }

            protected override int ReadDigit() {
                return CharValue(_digits[_position++], _base);
            }
        }

        public static BigInteger ParseBigInteger(int sign, string text, int @base) {
            YamlBignumParser p = new YamlBignumParser(text, @base);
            BigInteger ret = p.ParseDecimal(text.Length);
            return sign > 0 ? ret : -ret;
        }

        #endregion
    }
}
