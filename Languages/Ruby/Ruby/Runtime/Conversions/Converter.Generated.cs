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
using Microsoft.Scripting.Math;

namespace IronRuby.Runtime.Conversions {
    internal enum NumericTypeCode {
        Invalid = 0,

        // primitive:
        SByte = 1,
        Byte = 2,
        Int16 = 3,
        UInt16 = 4,
        // System::Char is treated like a single-character string and not a numeric type
        Int32 = 5,
        UInt32 = 6,
        Int64 = 7,
        UInt64 = 8,
        Single = 9,
        Double = 10,
        Decimal = 11,
        
        // extended:
        BigInteger = 12,
    }

    internal static partial class Converter {
        private const NumericTypeCode MaxPrimitive = NumericTypeCode.Decimal;
        private const NumericTypeCode MaxNumeric = NumericTypeCode.BigInteger;

        private static readonly int[] ExplicitConversions = CreateExplicitConversions();
        private static readonly NumericTypeCode[] TypeCodeMapping = CreateTypeCodeMapping();

        private static NumericTypeCode GetNumericTypeCode(Type/*!*/ type) {
            TypeCode tc = Type.GetTypeCode(type);
            if (tc == TypeCode.Object) {
                if (type == typeof(BigInteger)) {
                    return NumericTypeCode.BigInteger;
                }
            }

            return TypeCodeMapping[(int)tc];
        }

        private static NumericTypeCode[] CreateTypeCodeMapping() {
            var result = new NumericTypeCode[20];
            result[(int)TypeCode.SByte] = NumericTypeCode.SByte;
            result[(int)TypeCode.Byte] = NumericTypeCode.Byte;
            result[(int)TypeCode.Int16] = NumericTypeCode.Int16;
            result[(int)TypeCode.UInt16] = NumericTypeCode.UInt16;
            result[(int)TypeCode.Int32] = NumericTypeCode.Int32;
            result[(int)TypeCode.UInt32] = NumericTypeCode.UInt32;
            result[(int)TypeCode.Int64] = NumericTypeCode.Int64;
            result[(int)TypeCode.UInt64] = NumericTypeCode.UInt64;
            result[(int)TypeCode.Single] = NumericTypeCode.Single;
            result[(int)TypeCode.Double] = NumericTypeCode.Double;
            result[(int)TypeCode.Decimal] = NumericTypeCode.Decimal;
            return result;
        }

        internal static bool HasImplicitNumericConversion(Type/*!*/ fromType, Type/*!*/ toType) {
            NumericTypeCode toCode = GetNumericTypeCode(toType);
            NumericTypeCode fromCode = GetNumericTypeCode(fromType);
            return toCode != NumericTypeCode.Invalid
                && fromCode != NumericTypeCode.Invalid
                && (ExplicitConversions[(int)fromCode] & (1 << (int)toCode)) == 0;
        }

        internal static bool HasExplicitNumericConversion(Type/*!*/ fromType, Type/*!*/ toType) {
            return (ExplicitConversions[(int)GetNumericTypeCode(fromType)] & (1 << (int)GetNumericTypeCode(toType))) != 0;
        }
        
        private static int[] CreateExplicitConversions() {
            int[] result = new int[(int)MaxNumeric + 1];

#if GENERATOR
            ExplicitConversions = {
                "SByte"      => [         "Byte",          "UInt16",          "UInt32",          "UInt64",                                            ],
                "Byte"       => ["SByte",                                                                                                             ],
                "Int16"      => ["SByte", "Byte",          "UInt16",          "UInt32",          "UInt64",                                            ],
                "UInt16"     => ["SByte", "Byte", "Int16",                                                                                            ],
                "Int32"      => ["SByte", "Byte", "Int16", "UInt16",          "UInt32",          "UInt64",                                            ],
                "UInt32"     => ["SByte", "Byte", "Int16", "UInt16", "Int32",                                                                         ],
                "Int64"      => ["SByte", "Byte", "Int16", "UInt16", "Int32", "UInt32",          "UInt64",                                            ],
                "UInt64"     => ["SByte", "Byte", "Int16", "UInt16", "Int32", "UInt32", "Int64",                                                      ],
                "Single"     => ["SByte", "Byte", "Int16", "UInt16", "Int32", "UInt32", "Int64", "UInt64",                     "Decimal", "BigInteger"],
                "Double"     => ["SByte", "Byte", "Int16", "UInt16", "Int32", "UInt32", "Int64", "UInt64", "Single",           "Decimal", "BigInteger"],
                "Decimal"    => ["SByte", "Byte", "Int16", "UInt16", "Int32", "UInt32", "Int64", "UInt64", "Single", "Double",                        ],
                "BigInteger" => ["SByte", "Byte", "Int16", "UInt16", "Int32", "UInt32", "Int64", "UInt64", "Single", "Double", "Decimal",             ],
            }
            
            def generate
                ExplicitConversions.each do |from, toTypes|
                    bits = []
                    toTypes.each do |to|    
                        bits << "(1 << ((int)NumericTypeCode.#{to}))"
                    end
                    puts "result[(int)NumericTypeCode.#{from}] = #{bits.join(' | ')};"
                end
            end
#endif
            #region Generated by Converter.Generator.rb

            result[(int)NumericTypeCode.SByte] = (1 << ((int)NumericTypeCode.Byte)) | (1 << ((int)NumericTypeCode.UInt16)) | (1 << ((int)NumericTypeCode.UInt32)) | (1 << ((int)NumericTypeCode.UInt64));
            result[(int)NumericTypeCode.Byte] = (1 << ((int)NumericTypeCode.SByte));
            result[(int)NumericTypeCode.Int16] = (1 << ((int)NumericTypeCode.SByte)) | (1 << ((int)NumericTypeCode.Byte)) | (1 << ((int)NumericTypeCode.UInt16)) | (1 << ((int)NumericTypeCode.UInt32)) | (1 << ((int)NumericTypeCode.UInt64));
            result[(int)NumericTypeCode.UInt16] = (1 << ((int)NumericTypeCode.SByte)) | (1 << ((int)NumericTypeCode.Byte)) | (1 << ((int)NumericTypeCode.Int16));
            result[(int)NumericTypeCode.Int32] = (1 << ((int)NumericTypeCode.SByte)) | (1 << ((int)NumericTypeCode.Byte)) | (1 << ((int)NumericTypeCode.Int16)) | (1 << ((int)NumericTypeCode.UInt16)) | (1 << ((int)NumericTypeCode.UInt32)) | (1 << ((int)NumericTypeCode.UInt64));
            result[(int)NumericTypeCode.UInt32] = (1 << ((int)NumericTypeCode.SByte)) | (1 << ((int)NumericTypeCode.Byte)) | (1 << ((int)NumericTypeCode.Int16)) | (1 << ((int)NumericTypeCode.UInt16)) | (1 << ((int)NumericTypeCode.Int32));
            result[(int)NumericTypeCode.Int64] = (1 << ((int)NumericTypeCode.SByte)) | (1 << ((int)NumericTypeCode.Byte)) | (1 << ((int)NumericTypeCode.Int16)) | (1 << ((int)NumericTypeCode.UInt16)) | (1 << ((int)NumericTypeCode.Int32)) | (1 << ((int)NumericTypeCode.UInt32)) | (1 << ((int)NumericTypeCode.UInt64));
            result[(int)NumericTypeCode.UInt64] = (1 << ((int)NumericTypeCode.SByte)) | (1 << ((int)NumericTypeCode.Byte)) | (1 << ((int)NumericTypeCode.Int16)) | (1 << ((int)NumericTypeCode.UInt16)) | (1 << ((int)NumericTypeCode.Int32)) | (1 << ((int)NumericTypeCode.UInt32)) | (1 << ((int)NumericTypeCode.Int64));
            result[(int)NumericTypeCode.Single] = (1 << ((int)NumericTypeCode.SByte)) | (1 << ((int)NumericTypeCode.Byte)) | (1 << ((int)NumericTypeCode.Int16)) | (1 << ((int)NumericTypeCode.UInt16)) | (1 << ((int)NumericTypeCode.Int32)) | (1 << ((int)NumericTypeCode.UInt32)) | (1 << ((int)NumericTypeCode.Int64)) | (1 << ((int)NumericTypeCode.UInt64)) | (1 << ((int)NumericTypeCode.Decimal)) | (1 << ((int)NumericTypeCode.BigInteger));
            result[(int)NumericTypeCode.Double] = (1 << ((int)NumericTypeCode.SByte)) | (1 << ((int)NumericTypeCode.Byte)) | (1 << ((int)NumericTypeCode.Int16)) | (1 << ((int)NumericTypeCode.UInt16)) | (1 << ((int)NumericTypeCode.Int32)) | (1 << ((int)NumericTypeCode.UInt32)) | (1 << ((int)NumericTypeCode.Int64)) | (1 << ((int)NumericTypeCode.UInt64)) | (1 << ((int)NumericTypeCode.Single)) | (1 << ((int)NumericTypeCode.Decimal)) | (1 << ((int)NumericTypeCode.BigInteger));
            result[(int)NumericTypeCode.Decimal] = (1 << ((int)NumericTypeCode.SByte)) | (1 << ((int)NumericTypeCode.Byte)) | (1 << ((int)NumericTypeCode.Int16)) | (1 << ((int)NumericTypeCode.UInt16)) | (1 << ((int)NumericTypeCode.Int32)) | (1 << ((int)NumericTypeCode.UInt32)) | (1 << ((int)NumericTypeCode.Int64)) | (1 << ((int)NumericTypeCode.UInt64)) | (1 << ((int)NumericTypeCode.Single)) | (1 << ((int)NumericTypeCode.Double));
            result[(int)NumericTypeCode.BigInteger] = (1 << ((int)NumericTypeCode.SByte)) | (1 << ((int)NumericTypeCode.Byte)) | (1 << ((int)NumericTypeCode.Int16)) | (1 << ((int)NumericTypeCode.UInt16)) | (1 << ((int)NumericTypeCode.Int32)) | (1 << ((int)NumericTypeCode.UInt32)) | (1 << ((int)NumericTypeCode.Int64)) | (1 << ((int)NumericTypeCode.UInt64)) | (1 << ((int)NumericTypeCode.Single)) | (1 << ((int)NumericTypeCode.Double)) | (1 << ((int)NumericTypeCode.Decimal));

            #endregion

            return result;
        }
    }
}
