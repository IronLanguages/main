namespace Microsoft.IronPythonTools.Options {
    partial class PythonIntellisenseOptionsControl {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this._completionCommitedBy = new System.Windows.Forms.TextBox();
            this._completionCommitedByLabel = new System.Windows.Forms.Label();
            this._enterCommits = new System.Windows.Forms.CheckBox();
            this._intersectMembers = new System.Windows.Forms.CheckBox();
            this._selectionInCompletionGroupBox = new System.Windows.Forms.GroupBox();
            this._newLineAfterCompleteCompletion = new System.Windows.Forms.CheckBox();
            this._completionResultsGroupBox = new System.Windows.Forms.GroupBox();
            this._selectionInCompletionGroupBox.SuspendLayout();
            this._completionResultsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _completionCommitedBy
            // 
            this._completionCommitedBy.Location = new System.Drawing.Point(12, 32);
            this._completionCommitedBy.Name = "_completionCommitedBy";
            this._completionCommitedBy.Size = new System.Drawing.Size(371, 20);
            this._completionCommitedBy.TabIndex = 1;
            this._completionCommitedBy.TextChanged += new System.EventHandler(this._completionCommitedBy_TextChanged);
            // 
            // _completionCommitedByLabel
            // 
            this._completionCommitedByLabel.AutoSize = true;
            this._completionCommitedByLabel.Location = new System.Drawing.Point(12, 16);
            this._completionCommitedByLabel.Name = "_completionCommitedByLabel";
            this._completionCommitedByLabel.Size = new System.Drawing.Size(219, 13);
            this._completionCommitedByLabel.TabIndex = 0;
            this._completionCommitedByLabel.Text = "Committed by &typing the following characters:";
            // 
            // _enterCommits
            // 
            this._enterCommits.AutoSize = true;
            this._enterCommits.Location = new System.Drawing.Point(12, 58);
            this._enterCommits.Name = "_enterCommits";
            this._enterCommits.Size = new System.Drawing.Size(182, 17);
            this._enterCommits.TabIndex = 2;
            this._enterCommits.Text = "&Enter commits current completion";
            this._enterCommits.UseVisualStyleBackColor = true;
            this._enterCommits.CheckedChanged += new System.EventHandler(this._enterCommits_CheckedChanged);
            // 
            // _intersectMembers
            // 
            this._intersectMembers.AutoSize = true;
            this._intersectMembers.Location = new System.Drawing.Point(12, 19);
            this._intersectMembers.Name = "_intersectMembers";
            this._intersectMembers.Size = new System.Drawing.Size(272, 17);
            this._intersectMembers.TabIndex = 0;
            this._intersectMembers.Text = "Member completion displays &intersection of members";
            this._intersectMembers.UseVisualStyleBackColor = true;
            this._intersectMembers.CheckedChanged += new System.EventHandler(this._intersectMembers_CheckedChanged);
            // 
            // _selectionInCompletionGroupBox
            // 
            this._selectionInCompletionGroupBox.Controls.Add(this._completionCommitedByLabel);
            this._selectionInCompletionGroupBox.Controls.Add(this._completionCommitedBy);
            this._selectionInCompletionGroupBox.Controls.Add(this._enterCommits);
            this._selectionInCompletionGroupBox.Controls.Add(this._newLineAfterCompleteCompletion);
            this._selectionInCompletionGroupBox.Location = new System.Drawing.Point(3, 55);
            this._selectionInCompletionGroupBox.Name = "_selectionInCompletionGroupBox";
            this._selectionInCompletionGroupBox.Size = new System.Drawing.Size(389, 110);
            this._selectionInCompletionGroupBox.TabIndex = 1;
            this._selectionInCompletionGroupBox.TabStop = false;
            this._selectionInCompletionGroupBox.Text = "Selection in Completion List";
            // 
            // _newLineAfterCompleteCompletion
            // 
            this._newLineAfterCompleteCompletion.AutoSize = true;
            this._newLineAfterCompleteCompletion.Location = new System.Drawing.Point(12, 82);
            this._newLineAfterCompleteCompletion.Name = "_newLineAfterCompleteCompletion";
            this._newLineAfterCompleteCompletion.Size = new System.Drawing.Size(250, 17);
            this._newLineAfterCompleteCompletion.TabIndex = 3;
            this._newLineAfterCompleteCompletion.Text = "&Add new line on enter at end of fully typed word";
            this._newLineAfterCompleteCompletion.UseVisualStyleBackColor = true;
            this._newLineAfterCompleteCompletion.CheckedChanged += new System.EventHandler(this._newLineAfterCompleteCompletion_CheckedChanged);
            // 
            // _completionResultsGroupBox
            // 
            this._completionResultsGroupBox.Controls.Add(this._intersectMembers);
            this._completionResultsGroupBox.Location = new System.Drawing.Point(3, 3);
            this._completionResultsGroupBox.Name = "_completionResultsGroupBox";
            this._completionResultsGroupBox.Size = new System.Drawing.Size(389, 46);
            this._completionResultsGroupBox.TabIndex = 0;
            this._completionResultsGroupBox.TabStop = false;
            this._completionResultsGroupBox.Text = "Completion Results";
            // 
            // PythonIntellisenseOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._completionResultsGroupBox);
            this.Controls.Add(this._selectionInCompletionGroupBox);
            this.Name = "PythonIntellisenseOptionsControl";
            this.Size = new System.Drawing.Size(395, 317);
            this._selectionInCompletionGroupBox.ResumeLayout(false);
            this._selectionInCompletionGroupBox.PerformLayout();
            this._completionResultsGroupBox.ResumeLayout(false);
            this._completionResultsGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox _enterCommits;
        private System.Windows.Forms.CheckBox _intersectMembers;
        private System.Windows.Forms.TextBox _completionCommitedBy;
        private System.Windows.Forms.Label _completionCommitedByLabel;
        private System.Windows.Forms.GroupBox _selectionInCompletionGroupBox;
        private System.Windows.Forms.GroupBox _completionResultsGroupBox;
        private System.Windows.Forms.CheckBox _newLineAfterCompleteCompletion;
    }
}
