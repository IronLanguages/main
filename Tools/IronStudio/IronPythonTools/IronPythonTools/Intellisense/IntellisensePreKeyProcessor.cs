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

using System.Windows.Input;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.IronPythonTools.Intellisense {
    internal class IntellisensePreKeyProcessor : KeyProcessor {
        private ITextView _wpfTextView;

        private event KeyEventHandler _preProcessKeyDownEvent;
        private event KeyEventHandler _preProcessKeyUpEvent;

        public IntellisensePreKeyProcessor(IWpfTextView wpfTextView) {
            _wpfTextView = wpfTextView;
            _wpfTextView.Properties.GetProperty<IntellisenseController>(typeof(IntellisenseController)).Attach(this);
        }

        public override bool IsInterestedInHandledEvents {
            get {
                return true;
            }
        }

        public override void KeyDown(KeyEventArgs args) {
            this.FireKeyDown(args);
            base.KeyDown(args);
        }

        public override void KeyUp(KeyEventArgs args) {
            this.FireKeyUp(args);
            base.KeyUp(args);
        }

        public override void TextInput(TextCompositionEventArgs args) {
            this.FireTextInput(args);
            base.TextInput(args);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Public Surface

        internal event KeyEventHandler PreprocessKeyDown {
            add { _preProcessKeyDownEvent += value; }
            remove { _preProcessKeyDownEvent -= value; }
        }

        internal event KeyEventHandler PreprocessKeyUp {
            add { _preProcessKeyUpEvent += value; }
            remove { _preProcessKeyUpEvent -= value; }
        }

        internal event TextCompositionEventHandler PreprocessTextInput;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Implementation

        private void FireKeyDown(KeyEventArgs args) {
            KeyEventHandler handler = _preProcessKeyDownEvent;
            if (handler != null) {
                handler(_wpfTextView, args);
            }
        }

        private void FireKeyUp(KeyEventArgs args) {
            KeyEventHandler handler = _preProcessKeyUpEvent;
            if (handler != null) {
                handler(_wpfTextView, args);
            }
        }

        private void FireTextInput(TextCompositionEventArgs args) {
            TextCompositionEventHandler handler = PreprocessTextInput;
            if (handler != null) {
                handler(_wpfTextView, args);
            }
        }

        #endregion
    }
}
