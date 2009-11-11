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
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Interpreter {
    public sealed class BranchLabel {
        internal const int UnknownIndex = Int32.MinValue;
        internal const int UnknownSize = Int32.MinValue;

        private readonly InstructionList _instructions;

        internal int _index = UnknownIndex;
        internal int _expectedStackSize = UnknownSize;

        // Offsets of forward branching instructions targetting this label 
        // that need to be updated after we emit the label.
        private List<int> _forwardBranchFixups;

        public BranchLabel(InstructionList instructions) {
            _instructions = instructions;
        }

        public void Mark() {
            ContractUtils.Requires(_index == UnknownIndex && _expectedStackSize == UnknownSize);

            _expectedStackSize = _instructions.CurrentStackDepth;
            _index = _instructions.Count;

            if (_forwardBranchFixups != null) {
                foreach (var branchIndex in _forwardBranchFixups) {
                    FixupBranch(branchIndex);
                }
                _forwardBranchFixups = null;
            }
        }

        internal void AddBranch(int branchIndex) {
            Debug.Assert((_index == UnknownIndex) == (_expectedStackSize == UnknownSize));

            if (_index == UnknownIndex) {
                if (_forwardBranchFixups == null) {
                    _forwardBranchFixups = new List<int>();
                }
                _forwardBranchFixups.Add(branchIndex);
            } else {
                FixupBranch(branchIndex);
            }
        }

        internal void FixupBranch(int branchIndex) {
            Debug.Assert(_index != UnknownIndex);
            _instructions.FixupBranch(branchIndex, _index - branchIndex, _expectedStackSize);
        }
    }


}
