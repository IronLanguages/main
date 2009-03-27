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


namespace IronPython.Compiler {
    /// <summary>
    /// Specifies the compilation mode which will be used during the AST transformation
    /// </summary>
    enum CompilationMode {
        None,
        /// <summary>
        /// Compilation will proceed in a manner in which the resulting AST can be serialized to disk.
        /// </summary>
        ToDisk,
        /// <summary>
        /// Compilation will use a type and declare static fields for globals.  The resulting type
        /// is uncollectible and therefore extended use of this will cause memory leaks.
        /// </summary>
        Uncollectable,
        /// <summary>
        /// Compilation will use an array for globals.  The resulting code will be fully collectible
        /// and once all references are released will be collected.
        /// </summary>
        Collectable,
        /// <summary>
        /// Compilation will force all global accesses to do a full lookup.  This will also happen for
        /// any unbound local references.  This is the slowest form of code generation and is only
        /// used for exec/eval code where we can run against an arbitrary dictionary.
        /// </summary>
        Lookup
    }
}
