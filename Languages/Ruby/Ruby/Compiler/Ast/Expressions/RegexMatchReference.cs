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

#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System.Diagnostics;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

    public partial class RegexMatchReference : Expression {
        // $&
        internal const int EntireMatch = 0;
        internal const string EntireMatchName = "&";
        
        // $~
        internal const int MatchData = -1;
        internal const string MatchDataName = "~";

        // $+
        internal const int MatchLastGroup = -2;
        internal const string MatchLastGroupName = "+";

        // $`
        internal const int PreMatch = -3;
        internal const string PreMatchName = "`";

        // $'
        internal const int PostMatch = -4;
        internal const string PostMatchName = "'";

        private readonly int _index;

        public int Index {
            get { return (_index > 0) ? _index : 0; }
        }

        internal RegexMatchReference(int index, SourceSpan location) 
            : base(location) {
            Debug.Assert(index >= PostMatch, "index");
            _index = index;
        }

        public RegexMatchReference/*!*/ CreateGroupReference(int index, SourceSpan location) {
            ContractUtils.Requires(index >= 0);
            return new RegexMatchReference(index, location);
        }

        public RegexMatchReference/*!*/ CreateLastGroupReference(SourceSpan location) {
            return new RegexMatchReference(MatchLastGroup, location);
        }

        public RegexMatchReference/*!*/ CreatePrefixReference(SourceSpan location) {
            return new RegexMatchReference(PreMatch, location);
        }

        public RegexMatchReference/*!*/ CreateSuffixReference(SourceSpan location) {
            return new RegexMatchReference(PostMatch, location);
        }

        public RegexMatchReference/*!*/ CreateMatchReference(SourceSpan location) {
            return new RegexMatchReference(MatchData, location);
        }

        // TODO: keep only full names?
        public string/*!*/ VariableName {
            get {
                switch (_index) {
                    case EntireMatch: return EntireMatchName;
                    case MatchData: return MatchDataName;
                    case MatchLastGroup: return MatchLastGroupName;
                    case PreMatch: return PreMatchName;
                    case PostMatch: return PostMatchName;
                    default: return _index.ToString();
                }
            }
        }

        public string/*!*/ FullName {
            get {
                return "$" + VariableName;
            }
        }

        // Numeric references cannot be aliased (alias _ $n doesn't work for some reason):
        internal bool CanAlias {
            get {
                return _index <= 0;
            }
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            switch (_index) {
                case MatchData: 
                    return Methods.GetCurrentMatchData.OpCall(gen.CurrentScopeVariable);
                
                case MatchLastGroup:
                    return Methods.GetCurrentMatchLastGroup.OpCall(gen.CurrentScopeVariable);
                
                case PreMatch:
                    return Methods.GetCurrentPreMatch.OpCall(gen.CurrentScopeVariable);

                case PostMatch:
                    return Methods.GetCurrentPostMatch.OpCall(gen.CurrentScopeVariable);

                default:
                    return Methods.GetCurrentMatchGroup.OpCall(gen.CurrentScopeVariable, AstUtils.Constant(_index));
            }            
        }

        internal override MSA.Expression TransformDefinedCondition(AstGenerator/*!*/ gen) {
            return Ast.NotEqual(TransformRead(gen), AstUtils.Constant(null));
        }
        
        internal override string/*!*/ GetNodeName(AstGenerator/*!*/ gen) {
            // TODO: Ruby 1.9: all return "global-variable"
            switch (_index) {
                case MatchData: return "$" + MatchDataName;
                case MatchLastGroup: return "$" + MatchLastGroupName;
                case PreMatch: return "$" + PreMatchName;
                case PostMatch: return "$" + PostMatchName;
                default: return "$" + _index.ToString();
            }
        }
    }
}
