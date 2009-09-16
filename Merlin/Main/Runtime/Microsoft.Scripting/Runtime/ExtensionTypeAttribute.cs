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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Marks a class in the assembly as being an extension type for another type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
    public sealed class ExtensionTypeAttribute : Attribute {
        private readonly Type _extensionType;
        private readonly Type _extends;

        /// <summary>
        /// Marks a type in the assembly as being an extension type for another type.
        /// </summary>
        /// <param name="extends">The type which is being extended</param>
        /// <param name="extensionType">The type which provides the extension members.</param>
        public ExtensionTypeAttribute(Type extends, Type extensionType) {
            if (extends == null) {
                throw new ArgumentNullException("extends");
            }
            if (extensionType != null && !extensionType.IsPublic && !extensionType.IsNestedPublic) {
                throw Error.ExtensionMustBePublic(extensionType.FullName);
            }

            _extends = extends;
            _extensionType = extensionType;
        }

        /// <summary>
        /// The type which contains extension members which are added to the type being extended.
        /// </summary>
        public Type ExtensionType {
            get {
                return _extensionType;
            }
        }

        /// <summary>
        /// The type which is being extended by the extension type.
        /// </summary>
        public Type Extends {
            get {
                return _extends;
            }
        }
    }

}
