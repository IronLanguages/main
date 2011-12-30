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
#if FEATURE_FULL_CONSOLE

using System;
using System.IO;
using Microsoft.Scripting.Utils;
using System.Threading;

namespace Microsoft.Scripting.Hosting.Shell {

    public class BasicConsole : IConsole, IDisposable {
        private TextWriter _output;
        private TextWriter _errorOutput;
        private AutoResetEvent _ctrlCEvent;
        private Thread _creatingThread;

        public TextWriter Output {
            get { return _output; }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _output = value;
            }
        }

        public TextWriter ErrorOutput {
            get { return _errorOutput; }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _errorOutput = value;
            }
        }

        protected AutoResetEvent CtrlCEvent {
            get { return _ctrlCEvent; }
            set { _ctrlCEvent = value; }
        }

        protected Thread CreatingThread {
            get { return _creatingThread; }
            set { _creatingThread = value; }
        }

        public ConsoleCancelEventHandler ConsoleCancelEventHandler { get; set; }
        private ConsoleColor _promptColor;
        private ConsoleColor _outColor;
        private ConsoleColor _errorColor;
        private ConsoleColor _warningColor;

        public BasicConsole(bool colorful) {            
            _output = System.Console.Out;
            _errorOutput = System.Console.Error;
            SetupColors(colorful);

            _creatingThread = Thread.CurrentThread;            

            // Create the default handler
            this.ConsoleCancelEventHandler = delegate(object sender, ConsoleCancelEventArgs e) {
                if (e.SpecialKey == ConsoleSpecialKey.ControlC) {
                    e.Cancel = true;
                    _ctrlCEvent.Set();
                    _creatingThread.Abort(new KeyboardInterruptException(""));
                }
            };

            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e) {
                // Dispatch the registered handler
                ConsoleCancelEventHandler handler = this.ConsoleCancelEventHandler;
                if (handler != null) {
                    this.ConsoleCancelEventHandler(sender, e);
                }
            };

            _ctrlCEvent = new AutoResetEvent(false);
        }

        private void SetupColors(bool colorful) {

            if (colorful) {
                _promptColor = PickColor(ConsoleColor.Gray, ConsoleColor.White);
                _outColor = PickColor(ConsoleColor.Cyan, ConsoleColor.White);
                _errorColor = PickColor(ConsoleColor.Red, ConsoleColor.White);
                _warningColor = PickColor(ConsoleColor.Yellow, ConsoleColor.White);
            } else {
                _promptColor = _outColor = _errorColor = _warningColor = Console.ForegroundColor;
            }
        }

        private static ConsoleColor PickColor(ConsoleColor best, ConsoleColor other) {
            best = IsDark(Console.BackgroundColor) ? MakeLight(best) : MakeDark(best);
            other = IsDark(Console.BackgroundColor) ? MakeLight(other) : MakeDark(other);

            if (Console.BackgroundColor != best) {
                return best;
            }

            return other;
        }

        private static bool IsDark(ConsoleColor color) {
            // The dark colours are < 8 and the light are > 8,
            // but the two grays are a bit special
            return color < ConsoleColor.Gray || color == ConsoleColor.DarkGray;
        }

        private static ConsoleColor MakeLight(ConsoleColor color) {
            // DarkGray would stay dark gray, which would be hard to read on a dark background
            if (color == ConsoleColor.DarkGray)
                return ConsoleColor.White;

            // The light colours all have their 8 bit set
            return (ConsoleColor)(((int)color) | 0xF);
        }

        private static ConsoleColor MakeDark(ConsoleColor color) {
            // Gray would stay gray, which would be hard to read on a light background
            if (color == ConsoleColor.Gray)
                return ConsoleColor.Black;

            // The dark colours all have their 8 bit unset
            return (ConsoleColor)(((int)color) & ~0xF);
        }

        protected void WriteColor(TextWriter output, string str, ConsoleColor c) {
            ConsoleColor origColor = Console.ForegroundColor;
            Console.ForegroundColor = c;
      
            output.Write(str);
            output.Flush();

            Console.ForegroundColor = origColor;
        }

        #region IConsole Members

        public virtual string ReadLine(int autoIndentSize) {
            Write("".PadLeft(autoIndentSize), Style.Prompt);

            string res = Console.In.ReadLine();
            if (res == null) {
                // we have a race - the Ctrl-C event is delivered
                // after ReadLine returns.  We need to wait for a little
                // bit to see which one we got.  This will cause a slight
                // delay when shutting down the process via ctrl-z, but it's
                // not really perceptible.  In the ctrl-C case we will return
                // as soon as the event is signaled.
                if (_ctrlCEvent != null && _ctrlCEvent.WaitOne(100, false)) {
                    // received ctrl-C
                    return "";
                } else {
                    // received ctrl-Z
                    return null;
                }
            }
            return "".PadLeft(autoIndentSize) + res;
        }

        public virtual void Write(string text, Style style) {
            switch (style) {
                case Style.Prompt: WriteColor(_output, text, _promptColor); break;
                case Style.Out: WriteColor(_output, text, _outColor); break;
                case Style.Error: WriteColor(_errorOutput, text, _errorColor); break;
                case Style.Warning: WriteColor(_errorOutput, text, _warningColor); break;
            }
        }

        public void WriteLine(string text, Style style) {
            Write(text + Environment.NewLine, style);
        }

        public void WriteLine() {
            Write(Environment.NewLine, Style.Out);
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            if (_ctrlCEvent != null) {
                _ctrlCEvent.Close();
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

#endif