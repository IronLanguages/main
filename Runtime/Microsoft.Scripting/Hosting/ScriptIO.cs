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
using System.Security.Permissions;
using System.Text;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {
    /// <summary>
    /// Provides host-redirectable IO streams used by DLR languages for default IO.
    /// </summary>
    public sealed class ScriptIO
#if !SILVERLIGHT
        : MarshalByRefObject
#endif
    {

        private readonly SharedIO _io;

        public Stream InputStream { get { return _io.InputStream; } }
        public Stream OutputStream { get { return _io.OutputStream; } }
        public Stream ErrorStream { get { return _io.ErrorStream; } }

        public TextReader InputReader { get { return _io.InputReader; } }
        public TextWriter OutputWriter { get { return _io.OutputWriter; } }
        public TextWriter ErrorWriter { get { return _io.ErrorWriter; } }

        public Encoding InputEncoding { get { return _io.InputEncoding; } }
        public Encoding OutputEncoding { get { return _io.OutputEncoding; } }
        public Encoding ErrorEncoding { get { return _io.ErrorEncoding; } }

        internal SharedIO SharedIO { get { return _io; } }

        internal ScriptIO(SharedIO io) {
            Assert.NotNull(io);
            _io = io;
        }

        /// <summary>
        /// Used if the host stores the output as binary data.
        /// </summary>
        /// <param name="stream">Binary stream to write data to.</param>
        /// <param name="encoding">Encoding used to convert textual data written to the output by the script.</param>
        public void SetOutput(Stream stream, Encoding encoding) {
            ContractUtils.RequiresNotNull(stream, "stream");
            ContractUtils.RequiresNotNull(encoding, "encoding");
            _io.SetOutput(stream, new StreamWriter(stream, encoding));
        }

        /// <summary>
        /// Used if the host handles both kinds of data (textual and binary) by itself.
        /// </summary>
        public void SetOutput(Stream stream, TextWriter writer) {
            ContractUtils.RequiresNotNull(stream, "stream");
            ContractUtils.RequiresNotNull(writer, "writer");
            _io.SetOutput(stream, writer);
        }

        public void SetErrorOutput(Stream stream, Encoding encoding) {
            ContractUtils.RequiresNotNull(stream, "stream");
            ContractUtils.RequiresNotNull(encoding, "encoding");
            _io.SetErrorOutput(stream, new StreamWriter(stream, encoding));
        }

        public void SetErrorOutput(Stream stream, TextWriter writer) {
            ContractUtils.RequiresNotNull(stream, "stream");
            ContractUtils.RequiresNotNull(writer, "writer");
            _io.SetErrorOutput(stream, writer);
        }

        public void SetInput(Stream stream, Encoding encoding) {
            ContractUtils.RequiresNotNull(stream, "stream");
            ContractUtils.RequiresNotNull(encoding, "encoding");
            _io.SetInput(stream, new StreamReader(stream, encoding), encoding);
        }

        public void SetInput(Stream stream, TextReader reader, Encoding encoding) {
            ContractUtils.RequiresNotNull(stream, "stream");
            ContractUtils.RequiresNotNull(reader, "writer");
            ContractUtils.RequiresNotNull(encoding, "encoding");
            _io.SetInput(stream, reader, encoding);
        }

        public void RedirectToConsole() {
            _io.RedirectToConsole();
        }

#if !SILVERLIGHT
        // TODO: Figure out what is the right lifetime
        public override object InitializeLifetimeService() {
            return null;
        }
#endif
    }
}
