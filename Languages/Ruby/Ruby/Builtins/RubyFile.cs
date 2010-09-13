/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.IO;
using IronRuby.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {
    public class RubyFile : RubyIO {
        public string Path { get; set; }

        public RubyFile(RubyContext/*!*/ context)
            : base(context) {
            Path = null;
        }

        public RubyFile(RubyContext/*!*/ context, string/*!*/ path, IOMode mode)
            : base(context, OpenFileStream(context, path, mode), mode) {
            Path = path;
        }

        public RubyFile(RubyContext/*!*/ context, Stream/*!*/ stream, int descriptor, IOMode mode)
            : base(context, stream, descriptor, mode) {
            Path = null;
        }

        public static Stream/*!*/ OpenFileStream(RubyContext/*!*/ context, string/*!*/ path, IOMode mode) {
            ContractUtils.RequiresNotNull(path, "path");
            FileAccess access = mode.ToFileAccess();

            FileMode fileMode;
  
            if ((mode & IOMode.CreateIfNotExists) != 0) {
                if ((mode & IOMode.ErrorIfExists) != 0) {
                    access |= FileAccess.Write;
                    fileMode = FileMode.CreateNew;
                } else {
                    fileMode = FileMode.OpenOrCreate;
                }
            } else {
                fileMode = FileMode.Open;
            }

            if ((mode & IOMode.Truncate) != 0 && (access & FileAccess.Write) == 0) {
                throw RubyExceptions.CreateEINVAL("cannot truncate a file opened for reading only");
            }

            if ((mode & IOMode.WriteAppends) != 0 && (access & FileAccess.Write) == 0) {
                throw RubyExceptions.CreateEINVAL("cannot append to a file opened for reading only");
            }

            if (String.IsNullOrEmpty(path)) {
                throw RubyExceptions.CreateEINVAL();
            }

            Stream stream;
            if (path == "NUL") {
                stream = Stream.Null;
            } else {
                try {
                    stream = context.DomainManager.Platform.OpenInputFileStream(path, fileMode, access, FileShare.ReadWrite);
                } catch (FileNotFoundException) {
                    throw RubyExceptions.CreateENOENT(String.Format("No such file or directory - {0}", path));
                } catch (DirectoryNotFoundException e) {
                    throw RubyExceptions.CreateENOENT(e.Message, e);
                } catch (PathTooLongException e) {
                    throw RubyExceptions.CreateENOENT(e.Message, e);
                } catch (IOException) {
                    if ((mode & IOMode.ErrorIfExists) != 0) {
                        throw RubyExceptions.CreateEEXIST(path);
                    } else {
                        throw;
                    }
                } catch (ArgumentException e) {
                    throw RubyExceptions.CreateEINVAL(e.Message, e);
                }
            }

            if ((mode & IOMode.Truncate) != 0) {
                stream.SetLength(0);
            }

            return stream;
        }
    }
}