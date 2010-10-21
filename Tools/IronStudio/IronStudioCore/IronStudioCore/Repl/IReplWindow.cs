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
using System.Windows;

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronStudio.Repl {
    /// <summary>
    /// An implementation of a Read Eval Print Loop Window for iteratively developing code.
    /// </summary>
    public interface IReplWindow {
        /// <summary>
        /// Gets the ITextCaret in which the REPL window is executing.
        /// </summary>
        ITextCaret Caret {
            get;
        }

        /// <summary>
        /// WPF Content of the Repl Window
        /// </summary>
        FrameworkElement Content {
            get;
        }

        /// <summary>
        /// Content type in the Repl Window
        /// </summary>
        IContentType ContentType {
            get;
        }

        /// <summary>
        /// Gets the IWpfTextView in which the REPL window is executing.
        /// </summary>
        IWpfTextView CurrentView {
            get;
        }

        /// <summary>
        /// The language evaluator used in Repl Window
        /// </summary>
        IReplEvaluator Evaluator {
            get;
        }

        /// <summary>
        /// Gets or sets whether output from scripts should be echoed to the window.
        /// </summary>
        bool ShowOutput {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the up/down arrows support navigating history when at the end of the current input.
        /// </summary>
        bool UseSmartUpDown {
            get;
            set;
        }

        /// <summary>
        /// Title of the Repl Window
        /// </summary>
        string Title {
            get;
        }

        /// <summary>
        /// Clears the REPL window screen.
        /// </summary>
        void ClearScreen();

        void Focus();

        /// <summary>
        /// Pastes the specified text in as if the user had typed it.
        /// </summary>
        /// <param name="text"></param>
        void PasteText(string text);

        /// <summary>
        /// Resets the execution context clearing all variables.
        /// </summary>
        void Reset();

        void AbortCommand();

        /// <summary>
        /// Writes a line into the output buffer as if it was outputted by the program.
        /// </summary>
        /// <param name="text"></param>
        void WriteLine(string text);
    }
}
