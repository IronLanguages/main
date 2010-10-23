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
using IronRuby.Compiler.Generation;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using System.Diagnostics;
using IronRuby.Runtime.Calls;

namespace IronRuby.Runtime {
    [ReflectionCached, CLSCompliant(false)]
    public sealed class ConstantSiteCache {
        [Emitted]
        public static readonly WeakReference WeakNull = new WeakReference(null);

        [Emitted]
        public static readonly WeakReference WeakMissingConstant = new WeakReference(StrongMissingConstant);
        private static readonly object StrongMissingConstant = new object();

        //
        // Thread safety:
        // We need these fields to be volatile to ensure the right order of writes in RubyOps.GetUnqualifiedConstant.
        //

        [Emitted]
        public volatile int Version;

        [Emitted]
        public volatile object Value; // not null after initialized for the first time

        internal void Update(object newValue, int newVersion) {
            // Thread safety:
            // We need to write the version after we write the value. Otherwise the first time we initialize the cache with a value
            // another thread could read the value right after the version was set but before the Value field was initialized.
            Value = newValue;
            Version = newVersion;
        }
    }

    [ReflectionCached, CLSCompliant(false)]
    public sealed class IsDefinedConstantSiteCache {
        [Emitted]
        public volatile int Version;

        [Emitted]
        public volatile bool Value;

        internal void Update(bool newValue, int newVersion) {
            // Thread safety:
            // We need to write the version after we write the value. Otherwise the first time we initialize the cache with a value
            // another thread could read the value right after the version was set but before the Value field was initialized.
            Value = newValue;
            Version = newVersion;
        }
    }

    internal sealed class VersionAndModule {
        internal static readonly VersionAndModule Default = new VersionAndModule(0, 0);

        internal readonly int Version;
        internal readonly int ModuleId;

        internal VersionAndModule(int version, int moduleId) {
            Version = version;
            ModuleId = moduleId;
        }
    }

    public sealed class ExpressionQualifiedConstantSiteCache {
        internal volatile VersionAndModule/*!*/ Condition = VersionAndModule.Default;
        internal volatile object/*!*/ Value;

        internal void Update(object newValue, int newVersion, RubyModule newModule) {
            // Thread safety:
            // We need to write the version after we write the value. Otherwise the first time we initialize the cache with a value
            // another thread could read the value right after the version was set but before the Value field was initialized.
            Value = newValue;
            Condition = new VersionAndModule(newVersion, newModule.Id);
        }
    }

    public sealed class ExpressionQualifiedIsDefinedConstantSiteCache {
        internal volatile VersionAndModule/*!*/ Condition = VersionAndModule.Default;
        internal volatile bool Value;

        internal void Update(bool newValue, int newVersion, RubyModule newModule) {
            // Thread safety:
            // We need to write the version after we write the value. Otherwise the first time we initialize the cache with a value
            // another thread could read the value right after the version was set but before the Value field was initialized.
            Value = newValue;
            Condition = new VersionAndModule(newVersion, newModule.Id);
        }
    }
}
