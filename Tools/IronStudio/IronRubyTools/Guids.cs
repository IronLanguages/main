/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

// Guids.cs
// MUST match guids.h
using System;

namespace Microsoft.IronRubyTools
{   
    static class GuidList
    {
        public const string guidIronRubyToolsPkgString =    "65AC248D-B48B-40D1-83D8-FC82F98952A4";
        public const string guidIronRubyToolsCmdSetString = "F9682AFE-91B4-40FC-ABD2-7B1F67A52448";
        public const string guidToolWindowPersistanceString =  "3F5345F7-B147-4B8D-A86D-0677855665B3";

        public static readonly Guid guidIronRubyToolsCmdSet = new Guid(guidIronRubyToolsCmdSetString);
    };
}