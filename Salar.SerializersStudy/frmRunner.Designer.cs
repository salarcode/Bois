namespace Salar.SerializersStudy
{
	partial class frmRunner
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
			this.btnRepeat = new System.Windows.Forms.Button();
			this.txtResult = new System.Windows.Forms.TextBox();
			this.btnClear = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnRepeat
			// 
			this.btnRepeat.Location = new System.Drawing.Point(12, 12);
			this.btnRepeat.Name = "btnRepeat";
			this.btnRepeat.Size = new System.Drawing.Size(75, 23);
			this.btnRepeat.TabIndex = 0;
			this.btnRepeat.Text = "Repeat";
			this.btnRepeat.UseVisualStyleBackColor = true;
			this.btnRepeat.Click += new System.EventHandler(this.btnRepeat_Click);
			// 
			// txtResult
			// 
			this.txtResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtResult.Location = new System.Drawing.Point(12, 41);
			this.txtResult.Multiline = true;
			this.txtResult.Name = "txtResult";
			this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtResult.Size = new System.Drawing.Size(714, 399);
			this.txtResult.TabIndex = 1;
			// 
			// btnClear
			// 
			this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClear.Location = new System.Drawing.Point(692, 12);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(34, 23);
			this.btnClear.TabIndex = 3;
			this.btnClear.Text = "X";
			this.btnClear.UseVisualStyleBackColor = true;
			// 
			// frmRunner
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(738, 452);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.txtResult);
			this.Controls.Add(this.btnRepeat);
			this.Name = "frmRunner";
			this.Text = "Serializers Study";
			this.Load += new System.EventHandler(this.frmRunner_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnRepeat;
		private System.Windows.Forms.TextBox txtResult;
		private System.Windows.Forms.Button btnClear;
	}
}

