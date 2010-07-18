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
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

using Microsoft.Win32;

using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;

#if CLR2
using Microsoft.Scripting.Math;
#else
using System.Numerics;
#endif

#if !SILVERLIGHT //Registry not available in silverlight.

[assembly: PythonModule("_winreg", typeof(IronPython.Modules.PythonWinReg))]
namespace IronPython.Modules {
    public static class PythonWinReg {
        public const string __doc__ = "Provides access to the Windows registry.";

        public static PythonType error = PythonExceptions.WindowsError;

        #region Constants

        public static BigInteger HKEY_CLASSES_ROOT = 0x80000000L;
        public static BigInteger HKEY_CURRENT_USER = 0x80000001L;
        public static BigInteger HKEY_LOCAL_MACHINE = 0x80000002L;
        public static BigInteger HKEY_USERS = 0x80000003L;
        public static BigInteger HKEY_PERFORMANCE_DATA = 0x80000004L;
        public static BigInteger HKEY_CURRENT_CONFIG = 0x80000005L;
        public static BigInteger HKEY_DYN_DATA = 0x80000006L;

        public const int KEY_QUERY_VALUE = 0X1;
        public const int KEY_SET_VALUE = 0X2;
        public const int KEY_CREATE_SUB_KEY = 0X4;
        public const int KEY_ENUMERATE_SUB_KEYS = 0X8;
        public const int KEY_NOTIFY = 0X10;
        public const int KEY_CREATE_LINK = 0X20;

        public const int KEY_ALL_ACCESS = 0XF003F;
        public const int KEY_EXECUTE = 0X20019;
        public const int KEY_READ = 0X20019;
        public const int KEY_WRITE = 0X20006;

        public const int REG_CREATED_NEW_KEY = 0X1;
        public const int REG_OPENED_EXISTING_KEY = 0X2;

        public const int REG_NONE = 0X0;
        public const int REG_SZ = 0X1;
        public const int REG_EXPAND_SZ = 0X2;
        public const int REG_BINARY = 0X3;
        public const int REG_DWORD = 0X4;
        public const int REG_DWORD_LITTLE_ENDIAN = 0X4;
        public const int REG_DWORD_BIG_ENDIAN = 0X5;
        public const int REG_LINK = 0X6;
        public const int REG_MULTI_SZ = 0X7;
        public const int REG_RESOURCE_LIST = 0X8;
        public const int REG_FULL_RESOURCE_DESCRIPTOR = 0X9;
        public const int REG_RESOURCE_REQUIREMENTS_LIST = 0XA;

        public const int REG_NOTIFY_CHANGE_NAME = 0X1;
        public const int REG_NOTIFY_CHANGE_ATTRIBUTES = 0X2;
        public const int REG_NOTIFY_CHANGE_LAST_SET = 0X4;
        public const int REG_NOTIFY_CHANGE_SECURITY = 0X8;

        public const int REG_OPTION_RESERVED = 0X0;
        public const int REG_OPTION_NON_VOLATILE = 0X0;
        public const int REG_OPTION_VOLATILE = 0X1;
        public const int REG_OPTION_CREATE_LINK = 0X2;
        public const int REG_OPTION_BACKUP_RESTORE = 0X4;
        public const int REG_OPTION_OPEN_LINK = 0X8;

        public const int REG_NO_LAZY_FLUSH = 0X4;
        public const int REG_REFRESH_HIVE = 0X2;
        public const int REG_LEGAL_CHANGE_FILTER = 0XF;
        public const int REG_LEGAL_OPTION = 0XF;
        public const int REG_WHOLE_HIVE_VOLATILE = 0X1;

        #endregion

        #region Module Methods

        public static void CloseKey(HKEYType key) {
            key.Close();
        }

        public static HKEYType CreateKey(object key, string subKeyName) {
            HKEYType rootKey = GetRootKey(key);

            //if key is a system key and no subkey is specified return that.
            if (key is BigInteger && string.IsNullOrEmpty(subKeyName))
                return rootKey;

            HKEYType subKey = new HKEYType(rootKey.key.CreateSubKey(subKeyName));
            return subKey;
        }

        public static void DeleteKey(object key, string subKeyName) {
            HKEYType rootKey = GetRootKey(key);
            if (key is BigInteger && string.IsNullOrEmpty(subKeyName))
                throw new InvalidCastException("DeleteKey() argument 2 must be string, not None");

            try {
                rootKey.key.DeleteSubKey(subKeyName);
            } catch (ArgumentException e) {
                throw new ExternalException(e.Message);
            }
        }

        public static void DeleteValue(object key, string value) {
            HKEYType rootKey = GetRootKey(key);

            rootKey.key.DeleteValue(value, true);
        }

        public static string EnumKey(object key, int index) {
            HKEYType rootKey = GetRootKey(key);
            if (index >= rootKey.key.SubKeyCount) {
                throw PythonExceptions.CreateThrowable(PythonExceptions.WindowsError, PythonExceptions._WindowsError.ERROR_BAD_COMMAND, "No more data is available");
            }
            return rootKey.key.GetSubKeyNames()[index];
        }

        public static PythonTuple EnumValue(object key, int index) {
            HKEYType rootKey = GetRootKey(key);
            if (index >= rootKey.key.ValueCount) {
                throw PythonExceptions.CreateThrowable(PythonExceptions.WindowsError, PythonExceptions._WindowsError.ERROR_BAD_COMMAND, "No more data is available");
            }

            string valueName = rootKey.key.GetValueNames()[index];
            int valueKind = MapRegistryValueKind(rootKey.key.GetValueKind(valueName));

            object value = rootKey.key.GetValue(valueName);

            //Handle some special cases of registry values.
            if (valueKind == REG_MULTI_SZ)
                value = new List(value);
            else if (valueKind == REG_BINARY) {
                ASCIIEncoding encoding = new ASCIIEncoding();
                value = encoding.GetString((byte[])value);
            }
            // REG_EXPAND_SZ expands any environment variable present in the registry key.
            // CPython does the wrong thing and returns the unexpanded value. Should we put in a 
            // hack to return the wrong value that CPython does?

            return PythonTuple.MakeTuple(valueName, value, valueKind);
        }

        public static void FlushKey(object key) {
            HKEYType rootKey = GetRootKey(key);
            rootKey.key.Flush();
        }

        public static HKEYType OpenKey(object key, string subKeyName) {
            return OpenKey(key, subKeyName, 0, KEY_READ);
        }

        public static HKEYType OpenKey(object key, string subKeyName, int reserved, int mask) {
            HKEYType rootKey = GetRootKey(key);
            RegistryKey newKey = null;

            // I'm assuming that the masks that CPy uses are the same as the Win32 API one mentioned here-
            // http://msdn2.microsoft.com/en-us/library/ms724878(VS.85).aspx

            // KEY_WRITE is a combination of KEY_SET_VALUE and KEY_CREATE_SUB_KEY. We'll open with write access
            // if any of this is set.
            // KEY_READ is a combination of KEY_QUERY_VALUE, KEY_ENUMERATE_SUB_KEYS and KEY_NOTIFY. We'll open
            // with read access for all of these. 


            try {
                if ((mask & KEY_SET_VALUE) == KEY_SET_VALUE ||
                    (mask & KEY_CREATE_SUB_KEY) == KEY_CREATE_SUB_KEY) {
                    newKey = rootKey.key.OpenSubKey(subKeyName, true);
                } else if ((mask & KEY_QUERY_VALUE) == KEY_QUERY_VALUE ||
                           (mask & KEY_ENUMERATE_SUB_KEYS) == KEY_ENUMERATE_SUB_KEYS ||
                           (mask & KEY_NOTIFY) == KEY_NOTIFY) {
                    newKey = rootKey.key.OpenSubKey(subKeyName, false);
                } else {
                    throw new Win32Exception("Unexpected mode");
                }
            } catch (SecurityException) {
                throw PythonExceptions.CreateThrowable(PythonExceptions.WindowsError, PythonExceptions._WindowsError.ERROR_ACCESS_DENIED, "Access is denied");
            }


            if (newKey == null) {
                throw PythonExceptions.CreateThrowable(PythonExceptions.WindowsError, PythonExceptions._WindowsError.ERROR_FILE_NOT_FOUND, "The system cannot find the file specified");
            }

            return new HKEYType(newKey);
        }

        public static HKEYType OpenKeyEx(object key, string subKeyName) {
            return OpenKey(key, subKeyName);
        }

        public static PythonTuple QueryInfoKey(object key) {
            HKEYType rootKey = null;
            //The key can also be a handle. If it is, then retrieve it from the cache.
            if (key is int) {
                if (HKeyHandleCache.cache.ContainsKey((int)key)) {
                    if (HKeyHandleCache.cache[(int)key].IsAlive) {
                        rootKey = HKeyHandleCache.cache[(int)key].Target as HKEYType;
                    }
                }
            } else {
                rootKey = GetRootKey(key);
            }

            try {
                return PythonTuple.MakeTuple(rootKey.key.SubKeyCount, rootKey.key.ValueCount, 0);
            } catch (ObjectDisposedException e) {
                throw new ExternalException(e.Message);
            }
        }

        public static object QueryValue(object key, string subKeyName) {
            HKEYType pyKey = OpenKey(key, subKeyName);
            return pyKey.key.GetValue(null);
        }

        public static PythonTuple QueryValueEx(object key, string valueName) {
            HKEYType rootKey = GetRootKey(key);
            object value = rootKey.key.GetValue(valueName);
            int valueKind = MapRegistryValueKind(rootKey.key.GetValueKind(valueName));
            if (valueKind == REG_MULTI_SZ)
                value = new List(value);
            if (valueKind == REG_BINARY) {
                ASCIIEncoding encoding = new ASCIIEncoding();
                value = encoding.GetString((byte[])value);
            }

            return PythonTuple.MakeTuple(value, valueKind);
        }

        public static void SetValue(object key, string subKeyName, int type, string value) {
            HKEYType pyKey = CreateKey(key, subKeyName);
            pyKey.key.SetValue(null, value);
        }

        public static void SetValueEx(object key, string valueName, int reserved, int type, object value) {
            HKEYType rootKey = GetRootKey(key);
            RegistryValueKind regKind = (RegistryValueKind)type;

            if (regKind == RegistryValueKind.MultiString) {
                int size = ((List)value)._size;
                string[] strArray = new string[size];
                Array.Copy(((List)value)._data, strArray, size);
                rootKey.key.SetValue(valueName, strArray, regKind);
            } else if (regKind == RegistryValueKind.Binary) {
                byte[] byteArr = null;
                if (value is string) {
                    string strValue = value as string;
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byteArr = encoding.GetBytes(strValue);
                }
                rootKey.key.SetValue(valueName, byteArr, regKind);
            } else {
                rootKey.key.SetValue(valueName, value, regKind);
            }

        }

        public static HKEYType ConnectRegistry(string computerName, BigInteger key) {
            if (string.IsNullOrEmpty(computerName))
                computerName = string.Empty;

            RegistryKey newKey;
            try {
                newKey = RegistryKey.OpenRemoteBaseKey(MapSystemKey(key), computerName);
            }catch(IOException ioe) {
                throw PythonExceptions.CreateThrowable(PythonExceptions.WindowsError, PythonExceptions._WindowsError.ERROR_BAD_NETPATH, ioe.Message);
            } catch (Exception e) {
                throw new ExternalException(e.Message);
            }
            return new HKEYType(newKey);
        }
        #endregion

        #region Helpers
        private static HKEYType GetRootKey(object key) {
            HKEYType rootKey;
            rootKey = key as HKEYType;
            if (rootKey == null) {
                if (key is BigInteger) {
                    rootKey = new HKEYType(RegistryKey.OpenRemoteBaseKey(MapSystemKey((BigInteger)key), string.Empty));
                } else {
                    throw new InvalidCastException("The object is not a PyHKEY object");
                }
            }
            return rootKey;
        }

        private static RegistryHive MapSystemKey(BigInteger hKey) {
            if (hKey == HKEY_CLASSES_ROOT)
                return RegistryHive.ClassesRoot;
            else if (hKey == HKEY_CURRENT_CONFIG)
                return RegistryHive.CurrentConfig;
            else if (hKey == HKEY_CURRENT_USER)
                return RegistryHive.CurrentUser;
            else if (hKey == HKEY_DYN_DATA)
                return RegistryHive.DynData;
            else if (hKey == HKEY_LOCAL_MACHINE)
                return RegistryHive.LocalMachine;
            else if (hKey == HKEY_PERFORMANCE_DATA)
                return RegistryHive.PerformanceData;
            else if (hKey == HKEY_USERS)
                return RegistryHive.Users;
            else
                throw new ValueErrorException("Unknown system key");
        }

        private static int MapRegistryValueKind(RegistryValueKind registryValueKind) {
            switch (registryValueKind) {
                case RegistryValueKind.Binary:
                    return REG_BINARY;
                case RegistryValueKind.DWord:
                    return REG_DWORD;
                case RegistryValueKind.ExpandString:
                    return REG_EXPAND_SZ;
                case RegistryValueKind.MultiString:
                    return REG_MULTI_SZ;
                case RegistryValueKind.QWord:
                    return REG_DWORD;                  //?
                case RegistryValueKind.String:
                    return REG_SZ;
                case RegistryValueKind.Unknown:
                    return REG_NONE;                   //?
                default:
                    return REG_NONE;
            }
        }
        #endregion


        [PythonType]
        public class HKEYType : IDisposable {
            internal RegistryKey key;
            internal HKEYType(RegistryKey key) {
                this.key = key;
                HKeyHandleCache.cache[key.GetHashCode()] = new WeakReference(this);
            }

            public void Close() {
                key.Close();
            }

            public int Detach() {
                return 0; //Can't keep handle after the object is destroyed.
            }

            public int handle {
                get {
                    return key.GetHashCode();
                }
            }

            public static implicit operator int(HKEYType hKey) {
                return hKey.handle;
            }

            #region IDisposable Members
            void IDisposable.Dispose() {
                Close();
            }

            #endregion
        }
    }

    //CPython exposes the native handle for the registry keys as well. Since there is no .NET API to
    //expose the native handle, we return the hashcode of the key as the "handle". To track these handles 
    //and return the right RegistryKey we maintain this cache of the generated handles.
    internal static class HKeyHandleCache {
        internal static Dictionary<int, WeakReference> cache = new Dictionary<int, WeakReference>();

    }

}

#endif