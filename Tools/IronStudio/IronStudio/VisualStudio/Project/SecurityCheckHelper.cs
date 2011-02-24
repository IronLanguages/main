using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.BuildEngine;

namespace Microsoft.VisualStudio.Project {
    public class SecurityCheckHelper {
        public string[] GetNonImportedUsingTasks(ProjectShim project) {
            List<string> list = new List<string>();
            foreach (UsingTask task in project.UsingTasks) {
                if (!task.IsImported) {
                    list.Add(task.TaskName);
                }
            }
            return list.ToArray();
        }

        public string[] GetDirectlyImportedProjects(ProjectShim project) {
            List<string> list = new List<string>();
            foreach (Import import in project.Imports) {
                if (!import.IsImported) {
                    list.Add(import.EvaluatedProjectPath);
                }
            }
            return list.ToArray();
        }

        internal bool IsProjectSafe(string dangerousItemsPropertyName, string defaultDangerousItems, ProjectShim mainProject, ProjectShim userProject, SecurityCheckPass pass, out string reasonFailed, out bool isUserFile) {
            reasonFailed = string.Empty;
            isUserFile = false;
            string[] nonImportedItems = null;
            string[] nonImportedTargetNames = null;
            switch (pass) {
                case SecurityCheckPass.Targets:
                    if (mainProject != null) {
                        nonImportedItems = GetNonImportedTargetNames(mainProject);
                    }
                    if (userProject != null) {
                        nonImportedTargetNames = GetNonImportedTargetNames(userProject);
                    }
                    break;

                case SecurityCheckPass.Properties:
                    if (mainProject != null) {
                        nonImportedItems = GetNonImportedPropertyNames(mainProject);
                    }
                    if (userProject != null) {
                        nonImportedTargetNames = GetNonImportedPropertyNames(userProject);
                    }
                    break;

                case SecurityCheckPass.Items:
                    if (mainProject != null) {
                        nonImportedItems = GetNonImportedItemNames(mainProject);
                    }
                    if (userProject != null) {
                        nonImportedTargetNames = GetNonImportedItemNames(userProject);
                    }
                    break;

                default:
                    return false;
            }
            Dictionary<string, string> dangerousItems = CreateDangerousItemHashtable(defaultDangerousItems + mainProject.GetEvaluatedProperty(dangerousItemsPropertyName));
            bool flag = IsProjectSafeHelper(nonImportedItems, dangerousItems, out reasonFailed);
            if (!flag) {
                isUserFile = false;
                return false;
            }
            bool flag2 = IsProjectSafeHelper(nonImportedTargetNames, dangerousItems, out reasonFailed);
            if (!flag2) {
                isUserFile = true;
                return false;
            }
            return (flag && flag2);
        }

        internal static string[] GetNonImportedItemNames(ProjectShim project) {
            Dictionary<string, string> hashtable = new Dictionary<string, string>();
            foreach (BuildItemGroupShim shim in project.ItemGroups) {
                if (!shim.IsImported) {
                    foreach (BuildItemShim shim2 in shim) {
                        hashtable[shim2.Name] = string.Empty;
                    }
                    continue;
                }
            }
            return hashtable.Keys.ToArray();
        }

 
        internal static string[] GetNonImportedTargetNames(ProjectShim project) {
            List<string> list = new List<string>();
            foreach (TargetShim shim in project.Targets) {
                if (!shim.IsImported) {
                    list.Add(shim.Name);
                }
            }
            return list.ToArray();
        }

        internal static string[] GetNonImportedPropertyNames(ProjectShim project) {
            Dictionary<string, string> hashtable = new Dictionary<string, string>();
            foreach (BuildPropertyGroupShim shim in project.PropertyGroups) {
                if (!shim.IsImported) {
                    foreach (BuildPropertyShim shim2 in shim) {
                        hashtable[shim2.Name] = string.Empty;
                    }
                    continue;
                }
            }
            return hashtable.Keys.ToArray();
        }

        internal static Dictionary<string, string> CreateDangerousItemHashtable(string dangerousItems) {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(dangerousItems)) {
                foreach (string str in dangerousItems.Split(new char[] { ';' })) {
                    string str2 = str.Trim();
                    if (str2.Length > 0) {
                        dictionary[str2] = string.Empty;
                    }
                }
            }
            return dictionary;
        }

        internal static bool IsProjectSafeHelper(string[] nonImportedItems, Dictionary<string, string> dangerousItems, out string reasonFailed) {
            if (nonImportedItems != null) {
                foreach (string str in nonImportedItems) {
                    if (!string.IsNullOrEmpty(str) && (dangerousItems.ContainsKey(str) || (str[0] == '_'))) {
                        reasonFailed = str;
                        return false;
                    }
                }
            }
            reasonFailed = string.Empty;
            return true;
        }

    }
}
