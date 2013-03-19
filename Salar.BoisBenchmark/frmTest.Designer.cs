namespace Salar.BoisBenchmark
{
	partial class frmTest
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
			this.btnBenchmark = new System.Windows.Forms.Button();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.btnClear = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnBenchmark
			// 
			this.btnBenchmark.Location = new System.Drawing.Point(12, 12);
			this.btnBenchmark.Name = "btnBenchmark";
			this.btnBenchmark.Size = new System.Drawing.Size(75, 23);
			this.btnBenchmark.TabIndex = 0;
			this.btnBenchmark.Text = "Repeat";
			this.btnBenchmark.UseVisualStyleBackColor = true;
			this.btnBenchmark.Click += new System.EventHandler(this.btnBenchmark_Click);
			// 
			// txtLog
			// 
			this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtLog.Location = new System.Drawing.Point(12, 41);
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog.Size = new System.Drawing.Size(730, 436);
			this.txtLog.TabIndex = 1;
			// 
			// btnClear
			// 
			this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClear.Location = new System.Drawing.Point(708, 12);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(34, 23);
			this.btnClear.TabIndex = 2;
			this.btnClear.Text = "X";
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// frmTest
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(754, 489);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.txtLog);
			this.Controls.Add(this.btnBenchmark);
			this.Name = "frmTest";
			this.Text = "Benchmark";
			this.Load += new System.EventHandler(this.frmTest_Load);
			this.Shown += new System.EventHandler(this.frmTest_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnBenchmark;
		private System.Windows.Forms.TextBox txtLog;
		private System.Windows.Forms.Button btnClear;
	}
}