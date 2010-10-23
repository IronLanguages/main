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


using System;
using System.Runtime.InteropServices;
using Microsoft.IronStudio.Project;


namespace Microsoft.IronRubyTools.Project {
    /// <summary>
    /// "General" tab in project properties.
    /// </summary>
    /// <remarks>While currently empty this class has to be in this
    /// assembly and has to have unique guid.
    /// In the future if we need to add new properties to this page
    /// this is the place where it can be done.</remarks>
    [Guid(RubyConstants.GeneralPropertyPageGuid)]
    public class RubyGeneralPropertyPage : CommonGeneralPropertyPage {

    }
}
