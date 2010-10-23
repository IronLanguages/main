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

// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace Microsoft.IronPythonTools
{
    static class PkgCmdIDList
    {

        public const uint cmdidReplWindow       = 0x101;
        public const uint cmdidExecuteFileInRepl = 0x102;
        public const uint cmdidSendToRepl = 0x103;
        public const uint cmdidSendToDefiningModule = 0x104;
        public const uint cmdidFillParagraph = 0x105;
    };
}