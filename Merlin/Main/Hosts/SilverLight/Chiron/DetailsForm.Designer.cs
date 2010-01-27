namespace Chiron {
    partial class DetailsForm {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this._stopButton = new System.Windows.Forms.Button();
            this._urlLabel = new System.Windows.Forms.Label();
            this._port = new System.Windows.Forms.Label();
            this._path = new System.Windows.Forms.Label();
            this._urlValue = new System.Windows.Forms.Label();
            this._portValue = new System.Windows.Forms.Label();
            this._pathValue = new System.Windows.Forms.Label();
            this._chiron = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _stopButton
            // 
            this._stopButton.Location = new System.Drawing.Point(343, 145);
            this._stopButton.Name = "_stopButton";
            this._stopButton.Size = new System.Drawing.Size(75, 23);
            this._stopButton.TabIndex = 0;
            this._stopButton.Text = "Stop Server";
            this._stopButton.UseVisualStyleBackColor = true;
            this._stopButton.Click += new System.EventHandler(this.StopClicked);
            // 
            // _urlLabel
            // 
            this._urlLabel.AutoSize = true;
            this._urlLabel.Location = new System.Drawing.Point(14, 51);
            this._urlLabel.Name = "_urlLabel";
            this._urlLabel.Size = new System.Drawing.Size(32, 13);
            this._urlLabel.TabIndex = 1;
            this._urlLabel.Text = "URL:";
            // 
            // _port
            // 
            this._port.AutoSize = true;
            this._port.Location = new System.Drawing.Point(14, 75);
            this._port.Name = "_port";
            this._port.Size = new System.Drawing.Size(29, 13);
            this._port.TabIndex = 2;
            this._port.Text = "Port:";
            // 
            // _path
            // 
            this._path.AutoSize = true;
            this._path.Location = new System.Drawing.Point(14, 97);
            this._path.Name = "_path";
            this._path.Size = new System.Drawing.Size(58, 13);
            this._path.TabIndex = 3;
            this._path.Text = "Root Path:";
            // 
            // _urlValue
            // 
            this._urlValue.AutoSize = true;
            this._urlValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._urlValue.ForeColor = System.Drawing.Color.Blue;
            this._urlValue.Location = new System.Drawing.Point(148, 51);
            this._urlValue.Name = "_urlValue";
            this._urlValue.Size = new System.Drawing.Size(107, 13);
            this._urlValue.TabIndex = 4;
            this._urlValue.Text = "http://localhost:8080";
            this._urlValue.Click += new System.EventHandler(this.UrlClicked);
            // 
            // _portValue
            // 
            this._portValue.AutoSize = true;
            this._portValue.Location = new System.Drawing.Point(148, 75);
            this._portValue.Name = "_portValue";
            this._portValue.Size = new System.Drawing.Size(31, 13);
            this._portValue.TabIndex = 5;
            this._portValue.Text = "8080";
            // 
            // _pathValue
            // 
            this._pathValue.AutoSize = true;
            this._pathValue.Location = new System.Drawing.Point(148, 97);
            this._pathValue.Name = "_pathValue";
            this._pathValue.Size = new System.Drawing.Size(105, 13);
            this._pathValue.TabIndex = 6;
            this._pathValue.Text = "C:\\Inetpub\\wwwroot";
            // 
            // _chiron
            // 
            this._chiron.AutoSize = true;
            this._chiron.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._chiron.Location = new System.Drawing.Point(12, 9);
            this._chiron.Name = "_chiron";
            this._chiron.Size = new System.Drawing.Size(249, 25);
            this._chiron.TabIndex = 7;
            this._chiron.Text = "Chiron Development Server";
            // 
            // DetailsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 180);
            this.Controls.Add(this._chiron);
            this.Controls.Add(this._pathValue);
            this.Controls.Add(this._portValue);
            this.Controls.Add(this._urlValue);
            this.Controls.Add(this._path);
            this.Controls.Add(this._port);
            this.Controls.Add(this._urlLabel);
            this.Controls.Add(this._stopButton);
            this.Name = "DetailsForm";
            this.Text = "Chiron Development Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _stopButton;
        private System.Windows.Forms.Label _urlLabel;
        private System.Windows.Forms.Label _port;
        private System.Windows.Forms.Label _path;
        private System.Windows.Forms.Label _urlValue;
        private System.Windows.Forms.Label _portValue;
        private System.Windows.Forms.Label _pathValue;
        private System.Windows.Forms.Label _chiron;
    }
}