namespace Microsoft.IronPythonTools.Project {
    partial class PythonGeneralyPropertyPageControl {
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
            this._applicationGroup = new System.Windows.Forms.GroupBox();
            this._workingDirLabel = new System.Windows.Forms.Label();
            this._workingDirectory = new System.Windows.Forms.TextBox();
            this._windowsApplication = new System.Windows.Forms.CheckBox();
            this._startupFile = new System.Windows.Forms.TextBox();
            this._startupFileLabel = new System.Windows.Forms.Label();
            this._debugGroup = new System.Windows.Forms.GroupBox();
            this._arguments = new System.Windows.Forms.TextBox();
            this._argumentsLabel = new System.Windows.Forms.Label();
            this._searchPaths = new System.Windows.Forms.TextBox();
            this._searchPathLabel = new System.Windows.Forms.Label();
            this._interpreterPath = new System.Windows.Forms.TextBox();
            this._interpreterPathLabel = new System.Windows.Forms.Label();
            this._debugStdLib = new System.Windows.Forms.CheckBox();
            this._applicationGroup.SuspendLayout();
            this._debugGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // _applicationGroup
            // 
            this._applicationGroup.Controls.Add(this._workingDirLabel);
            this._applicationGroup.Controls.Add(this._workingDirectory);
            this._applicationGroup.Controls.Add(this._windowsApplication);
            this._applicationGroup.Controls.Add(this._startupFile);
            this._applicationGroup.Controls.Add(this._startupFileLabel);
            this._applicationGroup.Location = new System.Drawing.Point(4, 4);
            this._applicationGroup.Name = "_applicationGroup";
            this._applicationGroup.Size = new System.Drawing.Size(437, 112);
            this._applicationGroup.TabIndex = 0;
            this._applicationGroup.TabStop = false;
            this._applicationGroup.Text = "Application";
            // 
            // _workingDirLabel
            // 
            this._workingDirLabel.AutoSize = true;
            this._workingDirLabel.Location = new System.Drawing.Point(4, 54);
            this._workingDirLabel.Name = "_workingDirLabel";
            this._workingDirLabel.Size = new System.Drawing.Size(92, 13);
            this._workingDirLabel.TabIndex = 10;
            this._workingDirLabel.Text = "Working Directory";
            // 
            // _workingDirectory
            // 
            this._workingDirectory.Location = new System.Drawing.Point(139, 51);
            this._workingDirectory.Name = "_workingDirectory";
            this._workingDirectory.Size = new System.Drawing.Size(286, 20);
            this._workingDirectory.TabIndex = 1;
            this._workingDirectory.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _windowsApplication
            // 
            this._windowsApplication.AutoSize = true;
            this._windowsApplication.Location = new System.Drawing.Point(7, 79);
            this._windowsApplication.Name = "_windowsApplication";
            this._windowsApplication.Size = new System.Drawing.Size(125, 17);
            this._windowsApplication.TabIndex = 2;
            this._windowsApplication.Text = "Windows Application";
            this._windowsApplication.UseVisualStyleBackColor = true;
            this._windowsApplication.CheckedChanged += new System.EventHandler(this.Changed);
            // 
            // _startupFile
            // 
            this._startupFile.Location = new System.Drawing.Point(139, 25);
            this._startupFile.Name = "_startupFile";
            this._startupFile.Size = new System.Drawing.Size(286, 20);
            this._startupFile.TabIndex = 0;
            this._startupFile.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _startupFileLabel
            // 
            this._startupFileLabel.AutoSize = true;
            this._startupFileLabel.Location = new System.Drawing.Point(4, 28);
            this._startupFileLabel.Name = "_startupFileLabel";
            this._startupFileLabel.Size = new System.Drawing.Size(60, 13);
            this._startupFileLabel.TabIndex = 0;
            this._startupFileLabel.Text = "Startup File";
            // 
            // _debugGroup
            // 
            this._debugGroup.Controls.Add(this._debugStdLib);
            this._debugGroup.Controls.Add(this._arguments);
            this._debugGroup.Controls.Add(this._argumentsLabel);
            this._debugGroup.Controls.Add(this._searchPaths);
            this._debugGroup.Controls.Add(this._searchPathLabel);
            this._debugGroup.Controls.Add(this._interpreterPath);
            this._debugGroup.Controls.Add(this._interpreterPathLabel);
            this._debugGroup.Location = new System.Drawing.Point(4, 122);
            this._debugGroup.Name = "_debugGroup";
            this._debugGroup.Size = new System.Drawing.Size(437, 131);
            this._debugGroup.TabIndex = 15;
            this._debugGroup.TabStop = false;
            this._debugGroup.Text = "Debug";
            // 
            // _arguments
            // 
            this._arguments.Location = new System.Drawing.Point(139, 48);
            this._arguments.Name = "_arguments";
            this._arguments.Size = new System.Drawing.Size(286, 20);
            this._arguments.TabIndex = 1;
            this._arguments.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _argumentsLabel
            // 
            this._argumentsLabel.AutoSize = true;
            this._argumentsLabel.Location = new System.Drawing.Point(4, 51);
            this._argumentsLabel.Name = "_argumentsLabel";
            this._argumentsLabel.Size = new System.Drawing.Size(130, 13);
            this._argumentsLabel.TabIndex = 19;
            this._argumentsLabel.Text = "Command Line Arguments";
            // 
            // _searchPaths
            // 
            this._searchPaths.Location = new System.Drawing.Point(139, 22);
            this._searchPaths.Name = "_searchPaths";
            this._searchPaths.Size = new System.Drawing.Size(286, 20);
            this._searchPaths.TabIndex = 0;
            this._searchPaths.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _searchPathLabel
            // 
            this._searchPathLabel.AutoSize = true;
            this._searchPathLabel.Location = new System.Drawing.Point(4, 25);
            this._searchPathLabel.Name = "_searchPathLabel";
            this._searchPathLabel.Size = new System.Drawing.Size(71, 13);
            this._searchPathLabel.TabIndex = 17;
            this._searchPathLabel.Text = "Search Paths";
            // 
            // _interpreterPath
            // 
            this._interpreterPath.Location = new System.Drawing.Point(139, 74);
            this._interpreterPath.Name = "_interpreterPath";
            this._interpreterPath.Size = new System.Drawing.Size(286, 20);
            this._interpreterPath.TabIndex = 2;
            this._interpreterPath.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _interpreterPathLabel
            // 
            this._interpreterPathLabel.AutoSize = true;
            this._interpreterPathLabel.Location = new System.Drawing.Point(4, 77);
            this._interpreterPathLabel.Name = "_interpreterPathLabel";
            this._interpreterPathLabel.Size = new System.Drawing.Size(80, 13);
            this._interpreterPathLabel.TabIndex = 15;
            this._interpreterPathLabel.Text = "Interpreter Path";
            // 
            // _debugStdLib
            // 
            this._debugStdLib.AutoSize = true;
            this._debugStdLib.Location = new System.Drawing.Point(7, 98);
            this._debugStdLib.Name = "_debugStdLib";
            this._debugStdLib.Size = new System.Drawing.Size(138, 17);
            this._debugStdLib.TabIndex = 20;
            this._debugStdLib.Text = "Debug Standard Library";
            this._debugStdLib.UseVisualStyleBackColor = true;
            this._debugStdLib.CheckedChanged += Changed;
            // 
            // PythonGeneralyPropertyPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._debugGroup);
            this.Controls.Add(this._applicationGroup);
            this.Name = "PythonGeneralyPropertyPageControl";
            this.Size = new System.Drawing.Size(457, 285);
            this._applicationGroup.ResumeLayout(false);
            this._applicationGroup.PerformLayout();
            this._debugGroup.ResumeLayout(false);
            this._debugGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox _applicationGroup;
        private System.Windows.Forms.Label _startupFileLabel;
        private System.Windows.Forms.TextBox _startupFile;
        private System.Windows.Forms.CheckBox _windowsApplication;
        private System.Windows.Forms.TextBox _workingDirectory;
        private System.Windows.Forms.Label _workingDirLabel;
        private System.Windows.Forms.GroupBox _debugGroup;
        private System.Windows.Forms.TextBox _arguments;
        private System.Windows.Forms.Label _argumentsLabel;
        private System.Windows.Forms.TextBox _searchPaths;
        private System.Windows.Forms.Label _searchPathLabel;
        private System.Windows.Forms.TextBox _interpreterPath;
        private System.Windows.Forms.Label _interpreterPathLabel;
        private System.Windows.Forms.CheckBox _debugStdLib;
    }
}
