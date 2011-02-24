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

namespace Microsoft.IronRubyTools {

    public static class RubyConstants {
        public const string FileExtension = ".rb";        
        public const string LanguageName = "IronRuby";
        public const string IronRubyExecutable = "ir.exe";
        public const string IronRubyWindowsExecutable = "irw.exe";
        public const string ProjectImageList = "Microsoft.RubyImageList.bmp";

        public static class ProjectProperties {
            public const string DisableDebugging = "DisableDebugging";
            public const string RubyApplicationType = "RubyApplicationType";
            public const string RubyApplicationType_WebApp = "WebApp";
            public const string DefaultPort = "DefaultPort";
            public const string Launcher = "Launcher";
            public const string Launcher_Rack = "Rack";
            public const string Launcher_Spec = "Spec";
        }

        public const string TextEditorSettingsRegistryKey = LanguageName;
        public const string SnippetsIndex = @"IronRubyCodeSnippets\SnippetsIndex.xml";
        public const string SnippetsPath = @"IronRubyCodeSnippets\Snippets\";

        public const string LanguageServiceGuid = "dcf49fd2-e5bf-436d-b9f3-caf4f9029d02";
        public const string LibraryManagerGuid = "06fa4cb1-f476-4f0e-978d-fd1f8263f346";
        public const string LibraryManagerServiceGuid = "8464e8f5-80a5-45ae-839f-135d5d781264";
        public static readonly Guid RubyConsoleCmdSetGuid = new Guid("7fdd285b-0e83-42ab-b17e-59602eabbc3f");

        // Output window panes:
        public static readonly Guid ProjectOutputPaneGuid = new Guid("FAD82010-4B27-4B8D-8C0E-8F427035E53B");

        // Do not change below info without re-requesting PLK:
        public const string PLKProductVersion = "2.0";
        public const string PLKProductName = "IronRuby Language Service";
        public const string LanguageServicePackageGuid = "5e71f8f4-ae7c-4f87-9898-a646b24b9c17"; //matches PLK
        public const string ProjectSystemPackageGuid = "b76642b4-17e6-431e-9df4-09979a096ea3";
        // End of Do not change ...               

        public const string ProjectFactoryGuid = "b373b46a-b089-44d7-96ca-cd150d098bc8";
        public const string EditorFactoryGuid = "7ae31113-65bd-4b13-8ea4-c8bd4387cc02";
        public const string ProjectNodeGuid = "e8f249c4-d140-4f73-ae92-ba550cc107fb";
        public const string GeneralPropertyPageGuid = "3c8e94a6-40b3-4eb5-803e-4e0994b11222";

        
        public static readonly Guid ProjectCmdSetGuid = new Guid("2f37fdf1-7008-4d22-a856-853c74cb3d95");


        //IDs of the icons for product registration (see Resources.resx)
        public const int IconIfForSplashScreen = 300;
        public const int IconIdForAboutBox = 400;

    }
}
