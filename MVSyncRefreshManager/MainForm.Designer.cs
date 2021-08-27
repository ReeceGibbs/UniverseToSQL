
namespace MVSyncRefreshManager
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dumpButton = new System.Windows.Forms.Button();
            this.fetchFilesButton = new System.Windows.Forms.Button();
            this.bulkInsertButton = new System.Windows.Forms.Button();
            this.eventLog = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // dumpButton
            // 
            this.dumpButton.Location = new System.Drawing.Point(12, 12);
            this.dumpButton.Name = "dumpButton";
            this.dumpButton.Size = new System.Drawing.Size(282, 23);
            this.dumpButton.TabIndex = 0;
            this.dumpButton.Text = "Begin Data Dump";
            this.dumpButton.UseVisualStyleBackColor = true;
            this.dumpButton.Click += new System.EventHandler(this.dumpButton_Click);
            // 
            // fetchFilesButton
            // 
            this.fetchFilesButton.Enabled = false;
            this.fetchFilesButton.Location = new System.Drawing.Point(12, 41);
            this.fetchFilesButton.Name = "fetchFilesButton";
            this.fetchFilesButton.Size = new System.Drawing.Size(282, 23);
            this.fetchFilesButton.TabIndex = 1;
            this.fetchFilesButton.Text = "Fetch Remote Files";
            this.fetchFilesButton.UseVisualStyleBackColor = true;
            this.fetchFilesButton.Click += new System.EventHandler(this.fetchFilesButton_Click);
            // 
            // bulkInsertButton
            // 
            this.bulkInsertButton.Enabled = false;
            this.bulkInsertButton.Location = new System.Drawing.Point(12, 70);
            this.bulkInsertButton.Name = "bulkInsertButton";
            this.bulkInsertButton.Size = new System.Drawing.Size(282, 23);
            this.bulkInsertButton.TabIndex = 2;
            this.bulkInsertButton.Text = "Run Bulk Insert";
            this.bulkInsertButton.UseVisualStyleBackColor = true;
            this.bulkInsertButton.Click += new System.EventHandler(this.bulkInsertButton_Click);
            // 
            // eventLog
            // 
            this.eventLog.Location = new System.Drawing.Point(12, 99);
            this.eventLog.Name = "eventLog";
            this.eventLog.ReadOnly = true;
            this.eventLog.Size = new System.Drawing.Size(282, 339);
            this.eventLog.TabIndex = 3;
            this.eventLog.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(306, 450);
            this.Controls.Add(this.eventLog);
            this.Controls.Add(this.bulkInsertButton);
            this.Controls.Add(this.fetchFilesButton);
            this.Controls.Add(this.dumpButton);
            this.Name = "MainForm";
            this.Text = "MVSyncRefreshManager";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button dumpButton;
        private System.Windows.Forms.Button fetchFilesButton;
        private System.Windows.Forms.Button bulkInsertButton;
        private System.Windows.Forms.RichTextBox eventLog;
    }
}