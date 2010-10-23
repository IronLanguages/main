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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Math;
using IronRuby.Runtime;
using Microsoft.Scripting.Generation;

namespace IronRuby.Builtins {
    [RubyClass("Float", Extends = typeof(double), Inherits = typeof(Numeric)), Includes(typeof(ClrFloat), Copy = true), Includes(typeof(Precision))]
    [UndefineMethod("new", IsStatic = true)]
    public static class FloatOps {
        #region Constants

        /// <summary>
        /// Smallest Float such that 1.0+EPSILON != 1.0
        /// </summary>
        /// <remarks>System.Double.Epsilon is not actually the correct value!</remarks>
        [RubyConstant]
        public const double EPSILON = 0.0000000000000002220446049250313080847263336181640625;

        /// <summary>
        /// The smallest Float greater than zero
        /// </summary>
        /// <remarks>
        /// Note this is not double.MinValue, which is most negative Float value.
        /// </remarks>
        [RubyConstant]
        public const double MIN = 2.2250738585072014e-308;

        /// <summary>
        /// The largest possible value for Float
        /// </summary>
        [RubyConstant]
        public const double MAX = double.MaxValue;

        /// <summary>
        /// The number of digits available in the mantissa (base 10)
        /// </summary>
        [RubyConstant]
        public const int DIG = 15;

        /// <summary>
        /// The number of digits available in the mantissa (base 2)
        /// </summary>
        [RubyConstant]
        public const int MANT_DIG = 53;

        /// <summary>
        /// The maximum size of the exponent (base 10)
        /// </summary>
        [RubyConstant]
        public const double MAX_10_EXP = 308;

        /// <summary>
        /// The minimum size the the exponent (base 10)
        /// </summary>
        [RubyConstant]
        public const double MIN_10_EXP = -307;

        /// <summary>
        /// The maximum size of the exponent (base 2)
        /// </summary>
        [RubyConstant]
        public const int MAX_EXP = 1024;

        /// <summary>
        /// The minimum size of the exponent (base 2)
        /// </summary>
        [RubyConstant]
        public const int MIN_EXP = -1021;

        /// <summary>
        /// The radix or base of the mantissa
        /// </summary>
        [RubyConstant]
        public const int RADIX = 2;

        /// <summary>
        /// Rounding mode used by Float
        /// </summary>
        [RubyConstant]
        public const int ROUNDS = 1;

        #endregion
    }
}
 