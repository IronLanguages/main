/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.IronStudio.Repl;
using Microsoft.PyAnalysis;
using Microsoft.Scripting.Hosting;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.IronPythonTools.Intellisense {
    internal class NormalCompletionAnalysis : CompletionAnalysis {
        private readonly int _paramIndex;
        private readonly ITextSnapshot _snapshot;
        private readonly bool _intersectMembers, _hideAdvancedMembers;

        internal NormalCompletionAnalysis(string text, int pos, ITextSnapshot snapshot, ITrackingSpan span, ITextBuffer textBuffer, int paramIndex, bool intersectMembers = true, bool hideAdvancedMembers = false)
            : base(text, pos, span, textBuffer) {
            _paramIndex = paramIndex;
            _snapshot = snapshot;
            _intersectMembers = intersectMembers;
            _hideAdvancedMembers = hideAdvancedMembers;
        }

        public override CompletionSet GetCompletions(IGlyphService glyphService) {
            var start1 = _stopwatch.ElapsedMilliseconds;

            MemberResult[] members = null;
            IReplEvaluator eval;
            IDlrEvaluator dlrEval;
            if (_snapshot.TextBuffer.Properties.TryGetProperty<IReplEvaluator>(typeof(IReplEvaluator), out eval) &&
                (dlrEval = eval as IDlrEvaluator) != null) {
                string text = Text;
                if(Text.EndsWith(".")) {
                    text = Text.Substring(0, Text.Length - 1);
                }
                var memberNames = dlrEval.GetMemberNames(text);

                if (memberNames != null && memberNames.Count > 0) {
                    members = new MemberResult[memberNames.Count];
                    int i = 0;
                    foreach(var member in memberNames) {
                        members[i++] = new MemberResult(member.Name, GetMemberType(member));
                    }
                }
            }

            if (members == null) {
                var analysis = GetAnalysisEntry();
                if (analysis != null) {
                    members = analysis.GetMembers(
                        Text, 
                        _snapshot.GetLineNumberFromPosition(_pos) + 1, 
                        _intersectMembers).ToArray();
                } else {
                    members = new MemberResult[0];
                }
            }

            members = DoFilterCompletions(members);
            Array.Sort(members, ModuleSort);

            var end = _stopwatch.ElapsedMilliseconds;

            if (/*Logging &&*/ (end - start1) > TooMuchTime) {
                Trace.WriteLine(String.Format("{0} lookup time {1} for {2} members", this, end - start1, members.Length));
            }

            var start = _stopwatch.ElapsedMilliseconds;
            
            var result = new PythonCompletionSet(
                Text,
                Text,
                _snapshot.CreateTrackingSpan(_pos, 0, SpanTrackingMode.EdgeInclusive),
                TransformMembers(glyphService, members),
                new Completion[0]);

            end = _stopwatch.ElapsedMilliseconds;

            if (/*Logging &&*/ (end - start1) > TooMuchTime) {
                Trace.WriteLine(String.Format("{0} completion set time {1} total time {2}", this, end - start, end - start1));
            }

            return result;
        }

        private IEnumerable<Completion> TransformMembers(IGlyphService glyphService, MemberResult[] members) {
            return members.Select(m => PythonCompletion(glyphService, m));
        }

        private MemberResult[] DoFilterCompletions(MemberResult[] members) {
            if (_hideAdvancedMembers) {
                members = FilterCompletions(members, Text, (completion, filter) => completion.StartsWith(filter) && (!completion.StartsWith("__") || ! completion.EndsWith("__")));
            } else {
                members = FilterCompletions(members, Text, (x, y) => x.StartsWith(y));
            }
            return members;
        }

        private ResultType GetMemberType(MemberDoc member) {
            switch (member.Kind) {
                case MemberKind.Class: return ResultType.Class;
                case MemberKind.Constant: return ResultType.Constant;
                case MemberKind.Delegate: return ResultType.Delegate;
                case MemberKind.Enum: return ResultType.Enum;
                case MemberKind.EnumMember: return ResultType.EnumInstance;
                case MemberKind.Event: return ResultType.Event;
                case MemberKind.Field: return ResultType.Field;
                case MemberKind.Function: return ResultType.Function;
                case MemberKind.Instance: return ResultType.Instance;
                case MemberKind.Method: return ResultType.Method;
                case MemberKind.Module: return ResultType.Module;
                case MemberKind.Namespace: return ResultType.Namespace;
                case MemberKind.Property: return ResultType.Property;
                default:
                    return ResultType.Unknown;
            }
        }

        internal static MemberResult[] FilterCompletions(MemberResult[] completions, string text, Func<string, string, bool> filterFunc) {
            var cut = text.LastIndexOfAny(new[] { '.', ']', ')' });
            var filter = (cut == -1) ? text : text.Substring(cut + 1);

            var result = new List<MemberResult>(completions.Length);
            foreach (var comp in completions) {
                if (filterFunc(comp.Name, filter)) {
                    result.Add(new MemberResult(comp.Name, comp.Name.Substring(filter.Length), comp.Namespaces));
                }
            }
            return result.ToArray();
        }

        internal static int ModuleSort(MemberResult x, MemberResult y) {
            return MemberSortComparison(x.Name, y.Name);
        }

        /// <summary>
        /// Sorts members for displaying in completion list.  The member sort
        /// moves all __x__ functions to the end of the list.  Members which
        /// start with a single underscore (private members) are sorted as if 
        /// they did not start with an underscore.
        /// </summary> 
        internal static int MemberSortComparison(string xName, string yName) {
            bool xUnder = xName.StartsWith("__") && xName.EndsWith("__");
            bool yUnder = yName.StartsWith("__") && yName.EndsWith("__");
            int xStart = 0, yStart = 0;
            if (!xUnder && xName.StartsWith("_")) {
                xStart = 1;
            }
            if (!yUnder && yName.StartsWith("_")) {
                yStart = 1;
            }
            if (xUnder == yUnder) {
                // Compare the actual strings
                return String.Compare(xName, xStart, yName, yStart, Math.Max(xName.Length, yName.Length), StringComparison.OrdinalIgnoreCase);
            }
            // The one that starts with an underscore comes later
            return xUnder ? 1 : -1;
        }

    }
}
