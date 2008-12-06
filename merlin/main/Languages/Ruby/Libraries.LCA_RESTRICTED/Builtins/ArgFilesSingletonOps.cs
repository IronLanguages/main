/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
#if !SILVERLIGHT

using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;

namespace IronRuby.Builtins {

    /// <summary>
    /// ARGF singleton trait.
    /// </summary>
    [RubyConstant("ARGF")]
    [RubySingleton(BuildConfig = "!SILVERLIGHT"), Includes(typeof(Enumerable))]
    public static class ArgFilesSingletonOps {

        //fileno
        //tell
        //file
        //eof
        //to_s
        //each
        //readline
        //lineno=
        //path
        //to_i
        //getc
        //pos
        //eof?
        //gets

        [RubyMethod("filename")]
        public static MutableString/*!*/ GetCurrentFileName(RubyContext/*!*/ context, object self) {
            return context.InputProvider.CurrentFileName;
        }

        //close
        //each_byte
        //read
        //rewind
        //pos=
        //to_io
        //seek
        //skip
        //readchar
        //closed?
        //each_line
        //readlines
        //binmode
        //lineno
    }
}
#endif