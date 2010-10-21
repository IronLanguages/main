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

// Guids.cs
// MUST match guids.h
using System;

namespace Microsoft.IronPythonTools
{
    static class GuidList
    {
        public const string guidIronPythonToolsPkgString =    "6dbd7c1e-1f1b-496d-ac7c-c55dae66c783";
        public const string guidIronPythonToolsCmdSetString = "bdfa79d2-2cd2-474a-a82a-ce8694116825";

        public static readonly Guid guidIronPythonToolsCmdSet = new Guid(guidIronPythonToolsCmdSetString);
    };
}