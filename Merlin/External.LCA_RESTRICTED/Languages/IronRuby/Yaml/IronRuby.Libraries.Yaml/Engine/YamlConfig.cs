/***** BEGIN LICENSE BLOCK *****
 * Version: CPL 1.0
 *
 * The contents of this file are subject to the Common Public
 * License Version 1.0 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.eclipse.org/legal/cpl-v10.html
 *
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 *
 * Copyright (C) 2007 Ola Bini <ola@ologix.com>
 * Copyright (c) Microsoft Corporation.
 * 
 ***** END LICENSE BLOCK *****/

using System;

namespace IronRuby.StandardLibrary.Yaml {

    public class YamlOptions {
        private int _indent;
        private bool _useHeader;
        private bool _useVersion;
        private Version _version;
        private bool _explicitStart;
        private bool _explicitEnd;
        private string _anchorFormat;
        private bool _canonical;
        private int _bestWidth;
        private bool _useBlock;
        private bool _useFlow;

        internal static readonly YamlOptions DefaultOptions = new YamlOptions();

        public int Indent { get { return _indent; } set { _indent = value; } }
        public bool UseHeader { get { return _useHeader; } set { _useHeader = value; } }
        public bool UseVersion { get { return _useVersion; } set { _useVersion = value; } }
        public Version Version { get { return _version; } set { _version = value; } }
        public bool ExplicitStart { get { return _explicitStart; } set { _explicitStart = value; } }
        public bool ExplicitEnd { get { return _explicitEnd; } set { _explicitEnd = value; } }
        public string AnchorFormat { get { return _anchorFormat; } set { _anchorFormat = value; } }
        public bool Canonical { get { return _canonical; } set { _canonical = value; } }
        public int BestWidth { get { return _bestWidth; } set { _bestWidth = value; } }
        public bool UseBlock { get { return _useBlock; } set { _useBlock = value; } }
        public bool UseFlow { get { return _useFlow; } set { _useFlow = value; } }

        public YamlOptions() {
            Indent = 2;
            Version = new Version(1, 0);
            ExplicitStart = true;
            AnchorFormat = "id{0:000}";
            BestWidth = 80;
        }
    }
}
