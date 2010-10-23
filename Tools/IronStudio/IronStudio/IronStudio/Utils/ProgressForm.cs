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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Microsoft.IronStudio.Utils {
    public partial class ProgressForm : Form {
        public EventWaitHandle CancellationEvent { get; set; }
        public bool Cancelled { get; private set; }

        public ProgressForm(int maxProgress = 100) {
            InitializeComponent();
            _progressBar.Maximum = maxProgress;
        }

        private void CancelButton_Click(object sender, EventArgs e) {
            _cancelButton.Enabled = false;
            Cancelled = true;
            BackgroundWorker.CancelAsync();
            var handle = CancellationEvent;
            if (handle != null) {
                handle.Set();
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            _cancelButton.Enabled = false;
            Close();
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            _progressBar.Value = e.ProgressPercentage;
        }
    }
}
