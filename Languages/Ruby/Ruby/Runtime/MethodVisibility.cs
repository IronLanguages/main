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

using System;
using System.Collections.Generic;
using System.Text;

namespace IronRuby.Runtime {
    [Flags]
    public enum RubyMethodAttributes {
        None = 0,

        Public = 1,
        Private = 2,
        Protected = 4,
        DefaultVisibility = Public,
        VisibilityMask = Public | Private | Protected,

        /// <summary>
        /// Method does nothing.
        /// </summary>
        Empty = 8,

        MemberFlagsMask = VisibilityMask | Empty,

        /// <summary>
        /// Method is defined in the type's instance method table.
        /// </summary>
        Instance = 16,

        /// <summary>
        /// Method is defined in the type's static method table.
        /// </summary>
        Singleton = 32,

        /// <summary>
        /// Do not trigger method_added when the method is defined.
        /// </summary>
        NoEvent = 64,

        PublicInstance = Public | Instance,
        PrivateInstance = Private | Instance,
        ProtectedInstance = Protected | Instance,

        PublicSingleton = Public | Singleton,
        PrivateSingleton = Private | Singleton,
        ProtectedSingleton = Protected | Singleton,
        
        /// <summary>
        /// Set by module_function. Subsequently defined methods are private instance and public singleton. 
        /// </summary>
        ModuleFunction = Public | Instance | Singleton,

        Default = PublicInstance,
    }

    [Flags]
    public enum RubyMemberFlags {
        Invalid = 0,
        
        // visibility:
        Public = RubyMethodAttributes.Public,
        Private = RubyMethodAttributes.Private,
        Protected = RubyMethodAttributes.Protected,
        VisibilityMask = Public | Private | Protected,

        // method is empty:
        Empty = RubyMethodAttributes.Empty,
    }

    public enum RubyMethodVisibility {
        None = 0,
        Public = RubyMethodAttributes.Public,
        Private = RubyMethodAttributes.Private,
        Protected = RubyMethodAttributes.Protected
    }
}
