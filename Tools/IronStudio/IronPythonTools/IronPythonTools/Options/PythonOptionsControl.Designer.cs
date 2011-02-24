namespace Microsoft.IronPythonTools.Options {
    partial class PythonOptionsControl {
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
            this.components = new System.ComponentModel.Container();
            this._editorGroup = new System.Windows.Forms.GroupBox();
            this._outliningOnOpen = new System.Windows.Forms.CheckBox();
            this._fillParagraphText = new System.Windows.Forms.TextBox();
            this._fillParaColumnLabel = new System.Windows.Forms.Label();
            this._interactiveGroup = new System.Windows.Forms.GroupBox();
            this._smartReplHistory = new System.Windows.Forms.CheckBox();
            this._completionModeGroup = new System.Windows.Forms.GroupBox();
            this._evalAlways = new System.Windows.Forms.RadioButton();
            this._evalNoCalls = new System.Windows.Forms.RadioButton();
            this._evalNever = new System.Windows.Forms.RadioButton();
            this._interactiveOptions = new System.Windows.Forms.Label();
            this._interactiveOptionsValue = new System.Windows.Forms.TextBox();
            this._toolTips = new System.Windows.Forms.ToolTip(this.components);
            this._editorGroup.SuspendLayout();
            this._interactiveGroup.SuspendLayout();
            this._completionModeGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // _editorGroup
            // 
            this._editorGroup.Controls.Add(this._outliningOnOpen);
            this._editorGroup.Controls.Add(this._fillParagraphText);
            this._editorGroup.Controls.Add(this._fillParaColumnLabel);
            this._editorGroup.Location = new System.Drawing.Point(3, 3);
            this._editorGroup.Name = "_editorGroup";
            this._editorGroup.Size = new System.Drawing.Size(389, 70);
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
            // _fillParagraphText
            // 
            this._fillParagraphText.Location = new System.Drawing.Point(133, 41);
            this._fillParagraphText.Name = "_fillParagraphText";
            this._fillParagraphText.Size = new System.Drawing.Size(58, 20);
            this._fillParagraphText.TabIndex = 2;
            this._fillParagraphText.TextChanged += new System.EventHandler(this._fillParagraphText_TextChanged);
            // 
            // _fillParaColumnLabel
            // 
            this._fillParaColumnLabel.AutoSize = true;
            this._fillParaColumnLabel.Location = new System.Drawing.Point(12, 44);
            this._fillParaColumnLabel.Name = "_fillParaColumnLabel";
            this._fillParaColumnLabel.Size = new System.Drawing.Size(117, 13);
            this._fillParaColumnLabel.TabIndex = 1;
            this._fillParaColumnLabel.Text = "Fill Paragraph Columns:";
            // 
            // _interactiveGroup
            // 
            this._interactiveGroup.Controls.Add(this._smartReplHistory);
            this._interactiveGroup.Controls.Add(this._interactiveOptionsValue);
            this._interactiveGroup.Controls.Add(this._interactiveOptions);
            this._interactiveGroup.Controls.Add(this._completionModeGroup);
            this._interactiveGroup.Location = new System.Drawing.Point(3, 79);
            this._interactiveGroup.Name = "_interactiveGroup";
            this._interactiveGroup.Size = new System.Drawing.Size(389, 160);
            this._interactiveGroup.TabIndex = 1;
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
            // _interactiveOptions
            // 
            this._interactiveOptions.AutoSize = true;
            this._interactiveOptions.Location = new System.Drawing.Point(12, 40);
            this._interactiveOptions.Name = "_interactiveOptions";
            this._interactiveOptions.Size = new System.Drawing.Size(46, 13);
            this._interactiveOptions.TabIndex = 1;
            this._interactiveOptions.Text = "Options:";
            // 
            // _interactiveOptionsValue
            // 
            this._interactiveOptionsValue.Location = new System.Drawing.Point(69, 40);
            this._interactiveOptionsValue.Name = "_interactiveOptionsValue";
            this._interactiveOptionsValue.Size = new System.Drawing.Size(314, 20);
            this._interactiveOptionsValue.TabIndex = 2;
            this._interactiveOptionsValue.TextChanged += new System.EventHandler(this._interactiveOptionsValue_TextChanged);
            // 
            // _completionModeGroup
            // 
            this._completionModeGroup.Controls.Add(this._evalAlways);
            this._completionModeGroup.Controls.Add(this._evalNoCalls);
            this._completionModeGroup.Controls.Add(this._evalNever);
            this._completionModeGroup.Location = new System.Drawing.Point(19, 64);
            this._completionModeGroup.Name = "_completionModeGroup";
            this._completionModeGroup.Size = new System.Drawing.Size(364, 86);
            this._completionModeGroup.TabIndex = 3;
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
            // PythonOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._editorGroup);
            this.Controls.Add(this._interactiveGroup);
            this.Name = "PythonOptionsControl";
            this.Size = new System.Drawing.Size(395, 317);
            this._editorGroup.ResumeLayout(false);
            this._editorGroup.PerformLayout();
            this._interactiveGroup.ResumeLayout(false);
            this._interactiveGroup.PerformLayout();
            this._completionModeGroup.ResumeLayout(false);
            this._completionModeGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox _editorGroup;
        private System.Windows.Forms.GroupBox _interactiveGroup;
        private System.Windows.Forms.CheckBox _outliningOnOpen;
        private System.Windows.Forms.CheckBox _smartReplHistory;
        private System.Windows.Forms.GroupBox _completionModeGroup;
        private System.Windows.Forms.RadioButton _evalAlways;
        private System.Windows.Forms.RadioButton _evalNoCalls;
        private System.Windows.Forms.RadioButton _evalNever;
        private System.Windows.Forms.Label _fillParaColumnLabel;
        private System.Windows.Forms.TextBox _fillParagraphText;
        private System.Windows.Forms.Label _interactiveOptions;
        private System.Windows.Forms.TextBox _interactiveOptionsValue;
        private System.Windows.Forms.ToolTip _toolTips;
    }
}
