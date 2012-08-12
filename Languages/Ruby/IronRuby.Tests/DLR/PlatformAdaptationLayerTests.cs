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
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting;

namespace IronRuby.Tests {
    public partial class Tests {
        private class WinPAL : PlatformAdaptationLayer {
            public override bool IsSingleRootFileSystem {
                get { return false; }
            }
        }

        public void IsAbsolutePath_Windows() {
            var pal = new WinPAL();

            Assert(!pal.IsAbsolutePath(null));
            Assert(!pal.IsAbsolutePath(""));
            Assert(!pal.IsAbsolutePath("C"));
            Assert(!pal.IsAbsolutePath(@"\"));
            Assert(!pal.IsAbsolutePath(@"/"));
            Assert(!pal.IsAbsolutePath(@":"));
            Assert(!pal.IsAbsolutePath(@"\:"));
            Assert(!pal.IsAbsolutePath(@"/:"));
            Assert(!pal.IsAbsolutePath(@"XX:"));

            // current working directory relative
            Assert(!pal.IsAbsolutePath(@"."));
            Assert(!pal.IsAbsolutePath(@".\"));
            Assert(!pal.IsAbsolutePath(@"./"));
            Assert(!pal.IsAbsolutePath(@".."));
            Assert(!pal.IsAbsolutePath(@"..\"));
            Assert(!pal.IsAbsolutePath(@"../"));

            // Drive relative
            Assert(!pal.IsAbsolutePath(@"C:"));

            // Drive
            Assert(pal.IsAbsolutePath(@"C:\"));
            Assert(pal.IsAbsolutePath(@"C:/"));
            Assert(pal.IsAbsolutePath(@"C:\path"));

            // UNC
            Assert(pal.IsAbsolutePath(@"\/"));
            Assert(pal.IsAbsolutePath(@"\\"));
            Assert(pal.IsAbsolutePath(@"/\"));
            Assert(pal.IsAbsolutePath(@"//"));

            // relative to the current user:
            Assert(!pal.IsAbsolutePath(@"~\path"));
        }

        private class UnixPAL : PlatformAdaptationLayer {
            public override bool IsSingleRootFileSystem {
                get { return true; }
            }
        }

        public void IsAbsolutePath_Unix()
        {
            var pal = new UnixPAL();

            Assert(!pal.IsAbsolutePath(null));
            Assert(!pal.IsAbsolutePath(""));
            Assert(!pal.IsAbsolutePath("C"));
            Assert(!pal.IsAbsolutePath(@":"));
            Assert(!pal.IsAbsolutePath(@"XX:"));

            // root 
            Assert(pal.IsAbsolutePath(@"\"));
            Assert(pal.IsAbsolutePath(@"/"));
            Assert(pal.IsAbsolutePath(@"\:"));
            Assert(pal.IsAbsolutePath(@"/:"));
            Assert(pal.IsAbsolutePath(@"\/"));
            Assert(pal.IsAbsolutePath(@"\\"));
            Assert(pal.IsAbsolutePath(@"/\"));
            Assert(pal.IsAbsolutePath(@"//"));

            // current working directory relative
            Assert(!pal.IsAbsolutePath(@"."));
            Assert(!pal.IsAbsolutePath(@".\"));
            Assert(!pal.IsAbsolutePath(@"./"));
            Assert(!pal.IsAbsolutePath(@".."));
            Assert(!pal.IsAbsolutePath(@"..\"));
            Assert(!pal.IsAbsolutePath(@"../"));

            // Drive relative
            Assert(!pal.IsAbsolutePath(@"C:"));

            // Drive
            Assert(!pal.IsAbsolutePath(@"C:\"));
            Assert(!pal.IsAbsolutePath(@"C:/"));
            Assert(!pal.IsAbsolutePath(@"C:\path"));

            // relative to the current user:
            Assert(!pal.IsAbsolutePath(@"~\path"));
        }
    }
}
