namespace Microsoft.IronRubyTools.Options {
    partial class RubyOptionsControl {
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
            this._editorGroup = new System.Windows.Forms.GroupBox();
            this._outliningOnOpen = new System.Windows.Forms.CheckBox();
            this._autoIndent = new System.Windows.Forms.CheckBox();
            this._intellisenseGroup = new System.Windows.Forms.GroupBox();
            this._enterCommits = new System.Windows.Forms.CheckBox();
            this._intersectMembers = new System.Windows.Forms.CheckBox();
            this._interactiveGroup = new System.Windows.Forms.GroupBox();
            this._smartReplHistory = new System.Windows.Forms.CheckBox();
            this._completionModeGroup = new System.Windows.Forms.GroupBox();
            this._evalAlways = new System.Windows.Forms.RadioButton();
            this._evalNoCalls = new System.Windows.Forms.RadioButton();
            this._evalNever = new System.Windows.Forms.RadioButton();
            this._editorGroup.SuspendLayout();
            this._intellisenseGroup.SuspendLayout();
            this._interactiveGroup.SuspendLayout();
            this._completionModeGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // _editorGroup
            // 
            this._editorGroup.Controls.Add(this._outliningOnOpen);
            this._editorGroup.Controls.Add(this._autoIndent);
            this._editorGroup.Location = new System.Drawing.Point(3, 3);
            this._editorGroup.Name = "_editorGroup";
            this._editorGroup.Size = new System.Drawing.Size(468, 67);
            this._editorGroup.TabIndex = 0;
            this._editorGroup.TabStop = false;
            this._editorGroup.Text = "Editor";
            // 
            // _outliningOnOpen
            // 
            this._outliningOnOpen.AutoSize = true;
            this._outliningOnOpen.Location = new System.Drawing.Point(12, 20);
            this._outliningOnOpen.Name = "_outliningOnOpen";
            this._outliningOnOpen.Size = new System.Drawing.Size(199, 17);
            this._outliningOnOpen.TabIndex = 0;
            this._outliningOnOpen.Text = "Enter &outlining mode when files open";
            this._outliningOnOpen.UseVisualStyleBackColor = true;
            this._outliningOnOpen.CheckedChanged += new System.EventHandler(this._outliningOnOpen_CheckedChanged);
            // 
            // _autoIndent
            // 
            this._autoIndent.AutoSize = true;
            this._autoIndent.Location = new System.Drawing.Point(12, 39);
            this._autoIndent.Name = "_autoIndent";
            this._autoIndent.Size = new System.Drawing.Size(115, 17);
            this._autoIndent.TabIndex = 10;
            this._autoIndent.Text = "Enable &auto indent";
            this._autoIndent.UseVisualStyleBackColor = true;
            this._autoIndent.CheckedChanged += new System.EventHandler(this._autoIndent_CheckedChanged);
            // 
            // _intellisenseGroup
            // 
            this._intellisenseGroup.Controls.Add(this._enterCommits);
            this._intellisenseGroup.Controls.Add(this._intersectMembers);
            this._intellisenseGroup.Location = new System.Drawing.Point(3, 76);
            this._intellisenseGroup.Name = "_intellisenseGroup";
            this._intellisenseGroup.Size = new System.Drawing.Size(468, 61);
            this._intellisenseGroup.TabIndex = 10;
            this._intellisenseGroup.TabStop = false;
            this._intellisenseGroup.Text = "Intellisense";
            this._intellisenseGroup.Visible = false;
            // 
            // _enterCommits
            // 
            this._enterCommits.AutoSize = true;
            this._enterCommits.Location = new System.Drawing.Point(12, 20);
            this._enterCommits.Name = "_enterCommits";
            this._enterCommits.Size = new System.Drawing.Size(182, 17);
            this._enterCommits.TabIndex = 0;
            this._enterCommits.Text = "&Enter commits current completion";
            this._enterCommits.UseVisualStyleBackColor = true;
            this._enterCommits.CheckedChanged += new System.EventHandler(this._enterCommits_CheckedChanged);
            // 
            // _intersectMembers
            // 
            this._intersectMembers.AutoSize = true;
            this._intersectMembers.Location = new System.Drawing.Point(12, 40);
            this._intersectMembers.Name = "_intersectMembers";
            this._intersectMembers.Size = new System.Drawing.Size(272, 17);
            this._intersectMembers.TabIndex = 10;
            this._intersectMembers.Text = "Member completion displays &intersection of members";
            this._intersectMembers.UseVisualStyleBackColor = true;
            this._intersectMembers.CheckedChanged += new System.EventHandler(this._intersectMembers_CheckedChanged);
            // 
            // _interactiveGroup
            // 
            this._interactiveGroup.Controls.Add(this._smartReplHistory);
            this._interactiveGroup.Controls.Add(this._completionModeGroup);
            this._interactiveGroup.Location = new System.Drawing.Point(6, 143);
            this._interactiveGroup.Name = "_interactiveGroup";
            this._interactiveGroup.Size = new System.Drawing.Size(468, 142);
            this._interactiveGroup.TabIndex = 20;
            this._interactiveGroup.TabStop = false;
            this._interactiveGroup.Text = "Interactive Window";
            // 
            // _smartReplHistory
            // 
            this._smartReplHistory.AutoSize = true;
            this._smartReplHistory.Location = new System.Drawing.Point(12, 20);
            this._smartReplHistory.Name = "_smartReplHistory";
            this._smartReplHistory.Size = new System.Drawing.Size(210, 17);
            this._smartReplHistory.TabIndex = 0;
            this._smartReplHistory.Text = "Up/Down Arrow Keys use smart &history";
            this._smartReplHistory.UseVisualStyleBackColor = true;
            this._smartReplHistory.CheckedChanged += new System.EventHandler(this._smartReplHistory_CheckedChanged);
            // 
            // _completionModeGroup
            // 
            this._completionModeGroup.Controls.Add(this._evalAlways);
            this._completionModeGroup.Controls.Add(this._evalNoCalls);
            this._completionModeGroup.Controls.Add(this._evalNever);
            this._completionModeGroup.Location = new System.Drawing.Point(19, 44);
            this._completionModeGroup.Name = "_completionModeGroup";
            this._completionModeGroup.Size = new System.Drawing.Size(434, 86);
            this._completionModeGroup.TabIndex = 10;
            this._completionModeGroup.TabStop = false;
            this._completionModeGroup.Text = "Completion Mode";
            // 
            // _evalAlways
            // 
            this._evalAlways.AutoSize = true;
            this._evalAlways.Location = new System.Drawing.Point(12, 58);
            this._evalAlways.Name = "_evalAlways";
            this._evalAlways.Size = new System.Drawing.Size(160, 17);
            this._evalAlways.TabIndex = 20;
            this._evalAlways.TabStop = true;
            this._evalAlways.Text = "&Always evaluate expressions";
            this._evalAlways.UseVisualStyleBackColor = true;
            this._evalAlways.CheckedChanged += new System.EventHandler(this._evalAlways_CheckedChanged);
            // 
            // _evalNoCalls
            // 
            this._evalNoCalls.AutoSize = true;
            this._evalNoCalls.Location = new System.Drawing.Point(12, 39);
            this._evalNoCalls.Name = "_evalNoCalls";
            this._evalNoCalls.Size = new System.Drawing.Size(232, 17);
            this._evalNoCalls.TabIndex = 10;
            this._evalNoCalls.TabStop = true;
            this._evalNoCalls.Text = "Never evaluate expressions containing &calls";
            this._evalNoCalls.UseVisualStyleBackColor = true;
            this._evalNoCalls.CheckedChanged += new System.EventHandler(this._evalNoCalls_CheckedChanged);
            // 
            // _evalNever
            // 
            this._evalNever.AutoSize = true;
            this._evalNever.Location = new System.Drawing.Point(12, 20);
            this._evalNever.Name = "_evalNever";
            this._evalNever.Size = new System.Drawing.Size(156, 17);
            this._evalNever.TabIndex = 0;
            this._evalNever.TabStop = true;
            this._evalNever.Text = "&Never evaluate expressions";
            this._evalNever.UseVisualStyleBackColor = true;
            this._evalNever.CheckedChanged += new System.EventHandler(this._evalNever_CheckedChanged);
            // 
            // RubyOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._editorGroup);
            this.Controls.Add(this._intellisenseGroup);
            this.Controls.Add(this._interactiveGroup);
            this.Name = "RubyOptionsControl";
            this.Size = new System.Drawing.Size(474, 294);
            this._editorGroup.ResumeLayout(false);
            this._editorGroup.PerformLayout();
            this._intellisenseGroup.ResumeLayout(false);
            this._intellisenseGroup.PerformLayout();
            this._interactiveGroup.ResumeLayout(false);
            this._interactiveGroup.PerformLayout();
            this._completionModeGroup.ResumeLayout(false);
            this._completionModeGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox _editorGroup;
        private System.Windows.Forms.GroupBox _intellisenseGroup;
        private System.Windows.Forms.GroupBox _interactiveGroup;
        private System.Windows.Forms.CheckBox _outliningOnOpen;
        private System.Windows.Forms.CheckBox _autoIndent;
        private System.Windows.Forms.CheckBox _enterCommits;
        private System.Windows.Forms.CheckBox _intersectMembers;
        private System.Windows.Forms.CheckBox _smartReplHistory;
        private System.Windows.Forms.GroupBox _completionModeGroup;
        private System.Windows.Forms.RadioButton _evalAlways;
        private System.Windows.Forms.RadioButton _evalNoCalls;
        private System.Windows.Forms.RadioButton _evalNever;
    }
}
