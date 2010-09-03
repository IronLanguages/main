/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

using Microsoft.Scripting;

class WsdlProvider : IWebServiceProvider {
    #region IWebServiceProvider Members

    string IWebServiceProvider.Name {
        get { return "Wsdl"; }
    }

    bool IWebServiceProvider.MatchUrl(string url) {
        return url.EndsWith("wsdl", StringComparison.OrdinalIgnoreCase) ||
               url.EndsWith(".asmx", StringComparison.OrdinalIgnoreCase);
    }

    object IWebServiceProvider.LoadWebService(string url) {
        if (url.EndsWith(".asmx", StringComparison.OrdinalIgnoreCase)) {
            url += "?WSDL";
        }

        byte[] data = DynamicWebServiceHelpers.GetBytesForUrl(url);
        return CreateWebServiceFromWsdl(data);
    }

    #endregion

    #region Helpers

    object CreateWebServiceFromWsdl(byte[] wsdl) {
        // generate CodeDom from WSDL
        ServiceDescription sd = ServiceDescription.Read(new MemoryStream(wsdl));
        ServiceDescriptionImporter importer = new ServiceDescriptionImporter();
        importer.ServiceDescriptions.Add(sd);
        CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
        CodeNamespace codeNamespace = new CodeNamespace("");
        codeCompileUnit.Namespaces.Add(codeNamespace);
        importer.CodeGenerationOptions = CodeGenerationOptions.GenerateNewAsync | CodeGenerationOptions.GenerateOldAsync;
        importer.Import(codeNamespace, codeCompileUnit);

        // update web service proxy CodeDom tree to add dynamic support
        string wsProxyTypeName = FindProxyTypeAndAugmentCodeDom(codeNamespace);
        // compile CodeDom tree into an Assembly
        CodeDomProvider provider = CodeDomProvider.CreateProvider("CS");
        CompilerParameters compilerParams = new CompilerParameters();
        compilerParams.GenerateInMemory = true;
        compilerParams.IncludeDebugInformation = false;
        compilerParams.ReferencedAssemblies.Add(typeof(Microsoft.Scripting.Hosting.ScriptRuntime).Assembly.Location); //DLR
        CompilerResults results = provider.CompileAssemblyFromDom(compilerParams, codeCompileUnit);
        Assembly generatedAssembly = results.CompiledAssembly;

        // find the type derived from SoapHttpClientProtocol
        Type wsProxyType = generatedAssembly.GetType(wsProxyTypeName);

        if (wsProxyType == null) {
            throw new InvalidOperationException("Web service proxy type not generated.");
        }

        // create an instance of the web proxy type
        return Activator.CreateInstance(wsProxyType);
    }

    string FindProxyTypeAndAugmentCodeDom(CodeNamespace codeNamespace) {
        // add new type containing Type properties for each type
        // in the web service proxy assembly (kind of a namespace)
        string nsName = string.Format("ws_namespace_{0:x}", Guid.NewGuid().GetHashCode());
        CodeTypeDeclaration nsType = new CodeTypeDeclaration(nsName);

        CodeTypeDeclaration wsType = null; // the web service type (only one)

        foreach (CodeTypeDeclaration t in codeNamespace.Types) {
            string name = t.Name;

            // find the one derived from SoapHttpClientProtocol
            foreach (CodeTypeReference baseType in t.BaseTypes) {
                if (baseType.BaseType == typeof(SoapHttpClientProtocol).FullName) {
                    if (wsType != null) {
                        throw new InvalidDataException("Found more than one web service proxy type.");
                    }

                    wsType = t;
                }
            }

            // add the corresponding property to the namespace type
            CodeMemberProperty p = new CodeMemberProperty();
            p.Attributes &= ~MemberAttributes.AccessMask;
            p.Attributes |= MemberAttributes.Public;
            p.Name = name; // same as type name
            p.Type = new CodeTypeReference(typeof(Type));
            p.GetStatements.Add(new CodeMethodReturnStatement(new CodeTypeOfExpression(name)));
            nsType.Members.Add(p);
        }

        if (wsType == null) {
            // must have exactly one ws proxy
            throw new InvalidDataException("Web service proxy type not found.");
        }

        codeNamespace.Types.Add(nsType);

        // add ServiceNamespace property of the above type to the proxy type
        CodeMemberField nsField = new CodeMemberField(nsName, "_serviceNamespace");
        nsField.Attributes &= ~MemberAttributes.AccessMask;
        nsField.Attributes |= MemberAttributes.Private;
        nsField.InitExpression = new CodeObjectCreateExpression(nsName);
        wsType.Members.Add(nsField);

        CodeMemberProperty nsProp = new CodeMemberProperty();
        nsProp.Attributes &= ~MemberAttributes.AccessMask;
        nsProp.Attributes |= MemberAttributes.Public;
        nsProp.Name = "ServiceNamespace";
        nsProp.Type = new CodeTypeReference(nsName);
        nsProp.GetStatements.Add(new CodeMethodReturnStatement(
            new CodePropertyReferenceExpression(
                new CodeThisReferenceExpression(),
                "_serviceNamespace")));
        wsType.Members.Add(nsProp);

        // return the proxy type name
        return wsType.Name;
    }

    #endregion
}
