/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Dynamic;
using System.Dynamic.Binders;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions.Calls {
    /// <summary>
    /// ParameterWrapper represents the logical view of a parameter. For eg. the byref-reduced signature
    /// of a method with byref parameters will be represented using a ParameterWrapper of the underlying
    /// element type, since the logical view of the byref-reduced signature is that the argument will be
    /// passed by value (and the updated value is included in the return value).
    /// 
    /// Contrast this with ArgBuilder which represents the real physical argument passed to the method.
    /// </summary>
    public sealed class ParameterWrapper {
        private readonly Type _type;
        private readonly bool _prohibitNull, _isParams, _isParamsDict;
        private readonly ActionBinder _binder;
        private readonly SymbolId _name;

        // Type and other properties may differ from the values on the info; info could also be unspecified.
        private readonly ParameterInfo _info;

        /// <summary>
        /// ParameterInfo is not available.
        /// </summary>
        public ParameterWrapper(ActionBinder binder, Type type, SymbolId name, bool prohibitNull)
            : this(binder, null, type, name, prohibitNull, false, false) {
        }

        public ParameterWrapper(ActionBinder binder, ParameterInfo info, Type type, SymbolId name, bool prohibitNull, bool isParams, bool isParamsDict) {
            ContractUtils.RequiresNotNull(binder, "binder");
            ContractUtils.RequiresNotNull(type, "type");
            
            _type = type;
            _prohibitNull = prohibitNull;
            _binder = binder;
            _info = info;
            _name = name;
            _isParams = isParams;
            _isParamsDict = isParamsDict;
        }

        public ParameterWrapper(ActionBinder binder, ParameterInfo info)
            : this(binder, info, info.ParameterType, SymbolId.Empty, false, false, false) {

            _prohibitNull = CompilerHelpers.ProhibitsNull(info);
            _isParams = CompilerHelpers.IsParamArray(info);
            _isParamsDict = BinderHelpers.IsParamDictionary(info);
            if (_isParams || _isParamsDict) {
                // params arrays & dictionaries don't allow assignment by keyword
                _name = SymbolTable.StringToId("<unknown>");
            } else {
                _name = SymbolTable.StringToId(info.Name ?? "<unknown>");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type {
            get { return _type; }
        }

        public bool ProhibitNull {
            get { return _prohibitNull; }
        }

        public ParameterInfo ParameterInfo {
            get { return _info; }
        }

        public bool HasConversionFrom(Type fromType, NarrowingLevel level) {
            return _binder.CanConvertFrom(fromType, this, level);
        }

        // TODO: remove
        internal static Candidate GetPreferredParameters(IList<ParameterWrapper> parameters1, IList<ParameterWrapper> parameters2, Type[] actualTypes) {
            Debug.Assert(parameters1.Count == parameters2.Count);
            Debug.Assert(parameters1.Count == actualTypes.Length);

            Candidate result = Candidate.Equivalent;
            for (int i = 0; i < actualTypes.Length; i++) {
                Candidate preferred = GetPreferredParameter(parameters1[i], parameters2[i], actualTypes[i]);
                
                switch (result) {
                    case Candidate.Equivalent:
                        result = preferred; 
                        break;

                    case Candidate.One:
                        if (preferred == Candidate.Two) return Candidate.Ambiguous;
                        break;

                    case Candidate.Two:
                        if (preferred == Candidate.One) return Candidate.Ambiguous;
                        break;

                    case Candidate.Ambiguous:
                        if (preferred != Candidate.Equivalent) {
                            result = preferred;
                        }
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            return result;
        }

        public static Candidate GetPreferredParameters(IList<ParameterWrapper> parameters1, IList<ParameterWrapper> parameters2, MetaObject[] actualTypes) {
            Debug.Assert(parameters1.Count == parameters2.Count);
            Debug.Assert(parameters1.Count == actualTypes.Length);

            Candidate result = Candidate.Equivalent;
            for (int i = 0; i < actualTypes.Length; i++) {
                Candidate preferred = GetPreferredParameter(parameters1[i], parameters2[i], actualTypes[i]);

                switch (result) {
                    case Candidate.Equivalent:
                        result = preferred; 
                        break;

                    case Candidate.One:
                        if (preferred == Candidate.Two) return Candidate.Ambiguous;
                        break;

                    case Candidate.Two:
                        if (preferred == Candidate.One) return Candidate.Ambiguous;
                        break;

                    case Candidate.Ambiguous:
                        if (preferred != Candidate.Equivalent) {
                            result = preferred;
                        }
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            return result;
        }

        private static Candidate GetPreferredParameter(ParameterWrapper candidateOne, ParameterWrapper candidateTwo) {
            Assert.NotNull(candidateOne, candidateTwo);

            if (candidateOne._binder.ParametersEquivalent(candidateOne, candidateTwo)) {
                return Candidate.Equivalent;
            }

            Type t1 = candidateOne.Type;
            Type t2 = candidateTwo.Type;

            if (candidateOne._binder.CanConvertFrom(t2, candidateOne, NarrowingLevel.None)) {
                if (candidateOne._binder.CanConvertFrom(t1, candidateTwo, NarrowingLevel.None)) {
                    return Candidate.Ambiguous;
                } else {
                    return Candidate.Two;
                }
            }

            if (candidateOne._binder.CanConvertFrom(t1, candidateTwo, NarrowingLevel.None)) {
                return Candidate.One;
            }

            // Special additional rules to order numeric value types
            Candidate preferred = candidateOne._binder.PreferConvert(t1, t2);
            if (preferred.Chosen()) {
                return preferred;
            }

            preferred = candidateOne._binder.PreferConvert(t2, t1).TheOther();
            if (preferred.Chosen()) {
                return preferred;
            }

            return Candidate.Ambiguous;
        }

        private static Candidate GetPreferredParameter(ParameterWrapper candidateOne, ParameterWrapper candidateTwo, Type actualType) {
            Assert.NotNull(candidateOne, candidateTwo, actualType);

            if (candidateOne._binder.ParametersEquivalent(candidateOne, candidateTwo)) {
                return Candidate.Equivalent;
            }

            for (NarrowingLevel curLevel = NarrowingLevel.None; curLevel <= NarrowingLevel.All; curLevel++) {
                Candidate candidate = candidateOne._binder.SelectBestConversionFor(actualType, candidateOne, candidateTwo, curLevel);
                if (candidate.Chosen()) {
                    return candidate;
                }
            }

            return GetPreferredParameter(candidateOne, candidateTwo);
        }

        private static Candidate GetPreferredParameter(ParameterWrapper candidateOne, ParameterWrapper candidateTwo, MetaObject actualType) {
            Assert.NotNull(candidateOne, candidateTwo, actualType);

            if (candidateOne._binder.ParametersEquivalent(candidateOne, candidateTwo)) {
                return Candidate.Equivalent;
            }

            for (NarrowingLevel curLevel = NarrowingLevel.None; curLevel <= NarrowingLevel.All; curLevel++) {
                Candidate candidate = candidateOne._binder.SelectBestConversionFor(actualType.LimitType, candidateOne, candidateTwo, curLevel);
                if (candidate.Chosen()) {
                    return candidate;
                }
            }

            return GetPreferredParameter(candidateOne, candidateTwo);
        }

        public SymbolId Name {
            get {
                return _name;
            }
        }

        public bool IsParamsArray {
            get {
                return _isParams;
            }
        }

        public bool IsParamsDict {
            get {
                return _isParamsDict;
            }
        }

        public string ToSignatureString() {
            return Type.Name;
        }
    }

}
