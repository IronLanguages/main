/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using System.Collections.Generic;

namespace Microsoft.Scripting.Debugging.CompilerServices {
    /// <summary>
    /// Used by compilers to provide additional debug information about LambdaExpression to DebugContext
    /// </summary>
    public sealed class DebugLambdaInfo {
        private IDebugCompilerSupport _langSupport;
        private string _lambdaAlias;
        private IList<MSAst.ParameterExpression> _hiddenVariables;
        private IDictionary<MSAst.ParameterExpression, string> _variableAliases;
        private object _customPayload;
        private bool _optimizeForLeafFrames;
        
        public DebugLambdaInfo(
            IDebugCompilerSupport compilerSupport,
            string lambdaAlias,
            bool optimizeForLeafFrames,
            IList<MSAst.ParameterExpression> hiddenVariables,
            IDictionary<MSAst.ParameterExpression, string> variableAliases,
            object customPayload) {
            _langSupport = compilerSupport;
            _lambdaAlias = lambdaAlias;
            _hiddenVariables = hiddenVariables;
            _variableAliases = variableAliases;
            _customPayload = customPayload;
            _optimizeForLeafFrames = optimizeForLeafFrames;
        }

        public IDebugCompilerSupport CompilerSupport {
            get { return _langSupport; }
        }

        public string LambdaAlias {
            get { return _lambdaAlias; }
        }

        public IList<MSAst.ParameterExpression> HiddenVariables {
            get { return _hiddenVariables; }
        }

        public IDictionary<MSAst.ParameterExpression, string> VariableAliases {
            get { return _variableAliases; }
        }

        public object CustomPayload {
            get { return _customPayload; }
        }

        public bool OptimizeForLeafFrames {
            get { return _optimizeForLeafFrames; }
        }
    }
}
