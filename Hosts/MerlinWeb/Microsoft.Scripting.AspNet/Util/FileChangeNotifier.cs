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
using System.IO;
using System.Diagnostics;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace Microsoft.Scripting.AspNet.Util {
    delegate void FileChangedCallback(string path);

    class FileChangeNotifier {
        internal static void Register(string path, FileChangedCallback onFileChanged) {
            new FileChangeNotifier(path, onFileChanged);
        }

        private FileChangedCallback _onFileChanged;

        private FileChangeNotifier(string path, FileChangedCallback onFileChanged) {
            _onFileChanged = onFileChanged;
            RegisterForNextNotification(path);
        }

        private void RegisterForNextNotification(string path) {

            // Rely on the ASP.NET cache for file change notifications, since FileSystemWatcher
            // doesn't work in medium trust
            HttpRuntime.Cache.Insert(path /*key*/, path /*value*/, new CacheDependency(path),
                Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable, new CacheItemRemovedCallback(OnCacheItemRemoved));
        }

        private void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason) {
            _onFileChanged(key);

            // We need to register again to get the next notification
            RegisterForNextNotification(key);
        }
    }
}
