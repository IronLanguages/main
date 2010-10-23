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
using System.Diagnostics;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Math; 

namespace IronRuby.Compiler {

    /// <summary>
    /// TODO: move to DLR.
    /// </summary>
    public abstract class UnsignedBigIntegerParser {
        protected abstract int ReadDigit();

        protected UnsignedBigIntegerParser() {
        }

        public BigInteger/*!*/ ParseBinary(int digitCount) {
            return ParseBinarySuperPowers(digitCount, 1);
        }

        public BigInteger/*!*/ ParseHexadecimal(int digitCount) {
            return ParseBinarySuperPowers(digitCount, 4);
        }

        public BigInteger/*!*/ ParseDecimal(int digitCount) {
            const int DigitsPerWord = 9;
            const uint WordBase = 1000000000;
            return ParseAnyBase(digitCount, 10, WordBase, DigitsPerWord);
        }

        public BigInteger ParseOctal(int digitCount) {
            ContractUtils.Requires(digitCount > 0, "digitCount");

            const int BitsPerWord = 32;
            const int BitsPerDigit = 3;
            const int DigitsPerWord = BitsPerWord / BitsPerDigit;

            int remainingBits = (digitCount * BitsPerDigit) % (BitsPerWord * BitsPerDigit);
            int triwordCount = (digitCount * BitsPerDigit) / (BitsPerWord * BitsPerDigit);

            uint[] result = new uint[triwordCount * BitsPerDigit + (remainingBits + BitsPerWord - 1) / BitsPerWord];

            if (remainingBits <= BitsPerWord) {
                uint c = ReadBinaryWord(remainingBits / BitsPerDigit, BitsPerDigit);
                result[result.Length - 1] = c;
            } else if (remainingBits <= BitsPerWord * 2) {
                uint b = ReadBinaryWord((remainingBits - BitsPerWord) / BitsPerDigit, BitsPerDigit);
                uint bc = (uint)ReadDigit();
                uint c = ReadBinaryWord(DigitsPerWord, BitsPerDigit);
                result[result.Length - 1] = (b << 1) | (bc >> 2);
                result[result.Length - 2] = c | ((bc & 3) << 30);
            } else {
                ReadOctalTriword(result, result.Length - 1, (remainingBits - 2 * BitsPerWord) / BitsPerDigit);
            }

            for (int i = triwordCount * 3 - 1; i > 0; i -= 3) {
                ReadOctalTriword(result, i, DigitsPerWord);
            }

            return new BigInteger(+1, result);
        }

        private void ReadOctalTriword(uint[]/*!*/ result, int i, int digits) {
            const int BitsPerWord = 32;
            const int BitsPerDigit = 3;
            const int DigitsPerWord = BitsPerWord / BitsPerDigit;

            // [10 digits + 2 bits][1 bit + 10 digits + 1 bit][2 bits + 10 digits]
            uint a = ReadBinaryWord(digits, BitsPerDigit);
            uint ab = (uint)ReadDigit();
            uint b = ReadBinaryWord(DigitsPerWord, BitsPerDigit);
            uint bc = (uint)ReadDigit();
            uint c = ReadBinaryWord(DigitsPerWord, BitsPerDigit);

            result[i - 0] = (a << 2) | (ab >> 1);
            result[i - 1] = (b << 1) | ((ab & 1) << 31) | (bc >> 2);
            result[i - 2] = c | ((bc & 3) << 30);
        }

        public BigInteger/*!*/ Parse(int digitCount, int @base) {
            ContractUtils.Requires(@base > 1, "base");
            switch (@base) {
                case 2: return ParseBinary(digitCount);
                case 4: return ParseBinarySuperPowers(digitCount, 2);
                case 8: return ParseOctal(digitCount);
                case 16: return ParseHexadecimal(digitCount);
                case 10: return ParseDecimal(digitCount);
                default: 
                    return ParseDefault(digitCount, (uint)@base);
            }
        }

        internal BigInteger/*!*/ ParseDefault(int digitCount, uint @base) {
            uint wordBase = 1;
            int digitsPerWord = 0;
            while (true) {
                ulong newBase = (ulong)wordBase * @base;
                if (newBase > UInt32.MaxValue) break;
                wordBase = (uint)newBase;
                digitsPerWord++;
            }

            return ParseAnyBase(digitCount, @base, wordBase, digitsPerWord);
        }

        private BigInteger/*!*/ ParseAnyBase(int digitCount, uint @base, uint wordBase, int digitsPerWord) {
            ContractUtils.Requires(digitCount > 0, "digitCount");

            int resultSize = GetResultSize(digitCount, @base);
            int remainingDigits = digitCount % digitsPerWord;
            int wordCount = digitCount / digitsPerWord;

            uint[] result = new uint[resultSize];
            result[0] = ReadWord(remainingDigits, @base);
            int count = 1;
            for (int i = 0; i < wordCount; i++) {
                count = MultiplyAdd(result, count, wordBase, ReadWord(digitsPerWord, @base));
            }

            return new BigInteger(+1, result);
        }

        private int GetResultSize(int digitCount, uint @base) {
            int resultSize;

            try {
                resultSize = (int)System.Math.Ceiling(System.Math.Log(@base) * digitCount);
            } catch (OverflowException) {
                throw new ArgumentOutOfRangeException("Too many digits", "digitCount");
            }

            return resultSize;
        }

        private BigInteger/*!*/ ParseBinarySuperPowers(int digitCount, int bitsPerDigit) {
            ContractUtils.Requires(digitCount > 0, "digitCount");

            const int BitsPerWord = 32;

            Debug.Assert(BitsPerWord % bitsPerDigit == 0);
            int digitsPerWord = BitsPerWord / bitsPerDigit;

            int remainingDigits = digitCount % digitsPerWord;
            int wordCount = digitCount / digitsPerWord;
            uint[] result = new uint[wordCount + (remainingDigits + digitsPerWord - 1) / digitsPerWord];

            result[result.Length - 1] = ReadBinaryWord(remainingDigits, bitsPerDigit);
            for (int i = wordCount - 1; i >= 0; i--) {
                result[i] = ReadBinaryWord(digitsPerWord, bitsPerDigit);
            }

            return new BigInteger(+1, result);
        }

        // data = data * x + carry
        private int MultiplyAdd(uint[]/*!*/ data, int count, uint x, uint carry) {
            ulong m = 0;
            for (int i = 0; i < count + 1; i++) {
                m = (ulong)data[i] * x + carry;
                data[i] = (uint)(m & 0xffffffffL);
                carry = (uint)(m >> 32);
            }

            Debug.Assert(carry == 0);
            return (m > 0) ? count + 1 : count;
        }

        private uint ReadBinaryWord(int digitCount, int bitsPerDigit) {
            uint word = 0;
            while (digitCount > 0) {
                word = (word << bitsPerDigit) | (uint)ReadDigit();
                digitCount--;
            }
            return word;
        }

        private uint ReadWord(int digitCount, uint @base) {
            uint word = 0;
            while (digitCount > 0) {
                word = word * @base + (uint)ReadDigit();
                digitCount--;
            }
            return word;
        }
    }
}
