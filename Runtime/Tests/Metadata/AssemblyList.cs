/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metadata {
    internal static class AssemblyList {
        public static readonly string[] FwAssemblies = new string[] {
            "Accessibility.dll",
            "AspNetMMCExt.dll",
            "CustomMarshalers.dll",
            "ISymWrapper.dll",
            "Microsoft.CSharp.dll",
            "Microsoft.Data.Entity.Build.Tasks.dll",
            "Microsoft.JScript.dll",
            "Microsoft.Transactions.Bridge.dll",
            "Microsoft.Transactions.Bridge.Dtc.dll",
            "Microsoft.VisualBasic.Activities.Compiler.dll",
            "Microsoft.VisualBasic.Compatibility.Data.dll",
            "Microsoft.VisualBasic.Compatibility.dll",
            "Microsoft.VisualBasic.dll",
            "Microsoft.VisualC.dll",
            "Microsoft.VisualC.STLCLR.dll",
            "mscorlib.dll",
            "SMDiagnostics.dll",
            "sysglobl.dll",
            "System.Activities.Core.Presentation.dll",
            "System.Activities.dll",
            "System.Activities.DurableInstancing.dll",
            "System.Activities.Presentation.dll",
            "System.AddIn.Contract.dll",
            "System.AddIn.dll",
            "System.ComponentModel.Composition.dll",
            "System.ComponentModel.DataAnnotations.dll",
            "System.configuration.dll",
            "System.Configuration.Install.dll",
            "System.Core.dll",
            "System.Data.DataSetExtensions.dll",
            "System.Data.dll",
            "System.Data.Entity.Design.dll",
            "System.Data.Entity.dll",
            "System.Data.Linq.dll",
            "System.Data.OracleClient.dll",
            "System.Data.Services.Client.dll",
            "System.Data.Services.Design.dll",
            "System.Data.Services.dll",
            "System.Data.SqlXml.dll",
            "System.Deployment.dll",
            "System.Design.dll",
            "System.Device.dll",
            "System.DirectoryServices.AccountManagement.dll",
            "System.DirectoryServices.dll",
            "System.DirectoryServices.Protocols.dll",
            "System.dll",
            "System.Drawing.Design.dll",
            "System.Drawing.dll",
            "System.Dynamic.dll",
            "System.EnterpriseServices.dll",
            "System.IdentityModel.dll",
            "System.IO.Log.dll",
            "System.Management.dll",
            "System.Management.Instrumentation.dll",
            "System.Messaging.dll",
            "System.Net.dll",
            "System.Numerics.dll",
            "System.Runtime.Caching.dll",
            "System.Runtime.DurableInstancing.dll",
            "System.Runtime.Remoting.dll",
            "System.Runtime.Serialization.dll",
            "System.Runtime.Serialization.Formatters.Soap.dll",
            "System.Security.dll",
            "System.ServiceModel.Activation.dll",
            "System.ServiceModel.Activities.dll",
            "System.ServiceModel.Channels.dll",
            "System.ServiceModel.Discovery.dll",
            "System.ServiceModel.dll",
            "System.ServiceModel.Routing.dll",
            "System.ServiceModel.ServiceMoniker40.dll",
            "System.ServiceModel.WasHosting.dll",
            "System.ServiceModel.Web.dll",
            "System.ServiceProcess.dll",
            "System.Transactions.dll",
            "System.Web.Abstractions.dll",
            "System.Web.ApplicationServices.dll",
            "System.Web.DataVisualization.Design.dll",
            "System.Web.DataVisualization.dll",
            "System.Web.dll",
            "System.Web.DynamicData.Design.dll",
            "System.Web.DynamicData.dll",
            "System.Web.Entity.Design.dll",
            "System.Web.Entity.dll",
            "System.Web.Extensions.Design.dll",
            "System.Web.Extensions.dll",
            "System.Web.Mobile.dll",
            "System.Web.RegularExpressions.dll",
            "System.Web.Routing.dll",
            "System.Web.Services.dll",
            "System.Windows.Forms.DataVisualization.Design.dll",
            "System.Windows.Forms.DataVisualization.dll",
            "System.Windows.Forms.dll",
            "System.Workflow.Activities.dll",
            "System.Workflow.ComponentModel.dll",
            "System.Workflow.Runtime.dll",
            "System.WorkflowServices.dll",
            "System.Xaml.dll",
            "System.Xaml.Hosting.dll",
            "System.XML.dll",
            "System.Xml.Linq.dll",
            "XamlBuildTask.dll",
        };

        public static readonly string[] NotInVNext = new[] {
            "Microsoft.Build.Conversion.v4.0.dll",
            "Microsoft.Build.dll",
            "Microsoft.Build.Engine.dll",
            "Microsoft.Build.Framework.dll",
            "Microsoft.Build.Utilities.v4.0.dll",
            "Microsoft.VisualBasic.Vsa.dll",
            "Microsoft.Vsa.dll",
            "Microsoft.Windows.ApplicationServer.Applications.dll",
            "Microsoft_VsaVb.dll",
            "System.IdentityModel.Selectors.dll",
        };

        public static readonly string[] NonLoadable = new[] {
            "System.EnterpriseServices.Wrapper.dll",
        };
    }
}
