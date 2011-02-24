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

namespace Microsoft.IronStudio
{
    static class GuidList
    {
        public const string guidIronStudioPkgString = "F2D788A4-C316-4623-AE3A-FBE7E3E59E1A";
        public const string guidIronStudioCmdSetString = "FF3248AC-CB02-4CB4-A711-DD497D10D418";

        public static readonly Guid guidIronStudioCmdSet = new Guid(guidIronStudioCmdSetString);
    };
}