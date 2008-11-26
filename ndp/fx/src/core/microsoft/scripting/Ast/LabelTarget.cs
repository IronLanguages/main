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

using System.Dynamic.Utils;

namespace System.Linq.Expressions {

    /// <summary>
    /// Used to denote the target of a GotoExpression
    /// </summary>
    public sealed class LabelTarget {
        private readonly Type _type;
        private readonly string _name;

        internal LabelTarget(Type type, string name) {
            _type = type;
            _name = name;
        }

        // TODO: Annotations instead of name ?
        public string Name {
            get { return _name; }
        }

        /// <summary>
        /// The type of value that is passed when jumping to the label
        /// (or System.Void if no value should be passed)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type {
            get { return _type; }
        }
    }

    public partial class Expression {
        public static LabelTarget Label() {
            return Label(typeof(void), null);
        }

        public static LabelTarget Label(string name) {
            return Label(typeof(void), name);
        }

        public static LabelTarget Label(Type type) {
            return Label(type, null);
        }

        public static LabelTarget Label(Type type, string name) {
            ContractUtils.RequiresNotNull(type, "type");
            TypeUtils.ValidateType(type);
            return new LabelTarget(type, name);
        }
    }
}
