/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Diagnostics;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using MSA = System.Linq.Expressions;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

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
        internal const int MatchPrefix = -3;
        internal const string MatchPrefixName = "`";

        // $'
        internal const int MatchSuffix = -4;
        internal const string MatchSuffixName = "'";

        private readonly int _index;

        public int Index {
            get { return (_index > 0) ? _index : 0; }
        }

        internal RegexMatchReference(int index, SourceSpan location) 
            : base(location) {
            Debug.Assert(index >= MatchSuffix, "index");
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
            return new RegexMatchReference(MatchPrefix, location);
        }

        public RegexMatchReference/*!*/ CreateSuffixReference(SourceSpan location) {
            return new RegexMatchReference(MatchSuffix, location);
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
                    case MatchPrefix: return MatchPrefixName;
                    case MatchSuffix: return MatchSuffixName;
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
                
                case MatchPrefix:
                    return Methods.GetCurrentMatchPrefix.OpCall(gen.CurrentScopeVariable);

                case MatchSuffix:
                    return Methods.GetCurrentMatchSuffix.OpCall(gen.CurrentScopeVariable);

                default:
                    return Methods.GetCurrentMatchGroup.OpCall(gen.CurrentScopeVariable, Ast.Constant(_index));
            }            
        }

        internal override MSA.Expression TransformDefinedCondition(AstGenerator/*!*/ gen) {
            return Ast.NotEqual(TransformRead(gen), Ast.Constant(null));
        }
        
        internal override string/*!*/ GetNodeName(AstGenerator/*!*/ gen) {
            // TODO: Ruby 1.9: all return "global-variable"
            switch (_index) {
                case MatchData: return "$" + MatchDataName;
                case MatchLastGroup: return "$" + MatchLastGroupName;
                case MatchPrefix: return "$" + MatchPrefixName;
                case MatchSuffix: return "$" + MatchSuffixName;
                default: return "$" + _index.ToString();
            }
        }
    }
}
