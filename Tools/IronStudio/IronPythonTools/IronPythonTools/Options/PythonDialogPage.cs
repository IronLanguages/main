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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.IronPythonTools.Options {
    /// <summary>
    /// Base class used for saving/loading of settings.  The settings are stored in VSRegistryRoot\IronPython\Options\Category\SettingName
    /// where Category is provided in the constructor and SettingName is provided to each call of the Save*/Load* APIs.
    /// 
    /// The primary purpose of this class is so that we can be in control of providing reasonable default values.
    /// </summary>
    class PythonDialogPage : DialogPage {
        private readonly string _category;
        private const string _optionsKey = "Options";

        public PythonDialogPage(string category) {
            _category = category;
        }

        public void SaveBool(string name, bool value) {
            SaveString(name, value.ToString());
        }

        public void SaveInt(string name, int value) {
            SaveString(name, value.ToString());
        }

        public void SaveString(string name, string value) {
            using (var ironPython = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings, true).CreateSubKey("IronPython")) {
                using (var optionsKey = ironPython.CreateSubKey(_optionsKey)) {
                    using (var categoryKey = optionsKey.CreateSubKey(_category)) {
                        categoryKey.SetValue(name, value, Win32.RegistryValueKind.String);
                    }
                }
            }
        }

        public void SaveEnum<T>(string name, T value) where T : struct {
            SaveString(name, value.ToString());
        }

        public int? LoadInt(string name) {
            string res = LoadString(name);
            if (res == null) {
                return null;
            }
            return Convert.ToInt32(res);
        }

        public bool? LoadBool(string name) {
            string res = LoadString(name);
            if (res == null) {
                return null;
            }
            return Convert.ToBoolean(res);
        }

        public string LoadString(string name) {
            using (var ironPython = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings, true).CreateSubKey("IronPython")) {
                using (var optionsKey = ironPython.CreateSubKey(_optionsKey)) {
                    using (var categoryKey = optionsKey.CreateSubKey(_category)) {
                        return categoryKey.GetValue(name) as string;
                    }
                }
            }
        }

        public T? LoadEnum<T>(string name) where T : struct {
            string res = LoadString(name);
            if (res == null) {
                return null;
            }

            T enumRes;
            if (Enum.TryParse<T>(res, out enumRes)) {
                return enumRes;
            }
            return null;
        }
    }
}
