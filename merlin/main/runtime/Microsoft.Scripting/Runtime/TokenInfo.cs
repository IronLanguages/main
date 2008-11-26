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
using System.Dynamic;
using Microsoft.Contracts; 

namespace Microsoft.Scripting {

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")] // TODO: fix
    [Serializable]
    public struct TokenInfo : IEquatable<TokenInfo> {
        private TokenCategory _category;
        private TokenTriggers _trigger;
        private SourceSpan _span;
        
        public TokenCategory Category {
            get { return _category; }
            set { _category = value; }
        }

        public TokenTriggers Trigger {
            get { return _trigger; }
            set { _trigger = value; }
        }

        public SourceSpan SourceSpan {
            get { return _span; }
            set { _span = value; }
        }

        public TokenInfo(SourceSpan span, TokenCategory category, TokenTriggers trigger) {
            _category = category;
            _trigger = trigger;
            _span = span;
        }

        #region IEquatable<TokenInfo> Members

        [StateIndependent]
        public bool Equals(TokenInfo other) {
            return _category == other._category && _trigger == other._trigger && _span == other._span;
        }

        #endregion
    }
}
