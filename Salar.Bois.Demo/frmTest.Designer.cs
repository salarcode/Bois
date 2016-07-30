namespace CompactBinarySerializer.Demo
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
			this.txtData = new System.Windows.Forms.TextBox();
			this.txtFile = new System.Windows.Forms.TextBox();
			this.btnToBinary = new System.Windows.Forms.Button();
			this.btnFromBinary = new System.Windows.Forms.Button();
			this.btnCacheAll = new System.Windows.Forms.Button();
			this.btnBuff = new System.Windows.Forms.Button();
			this.btnBenchmark = new System.Windows.Forms.Button();
			this.btnClr = new System.Windows.Forms.Button();
			this.btnBonBecnhmark = new System.Windows.Forms.Button();
			this.btnReadBenchmark = new System.Windows.Forms.Button();
			this.btnWriteBenchmark = new System.Windows.Forms.Button();
			this.btnBin = new System.Windows.Forms.Button();
			this.btnStruxt = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// txtData
			// 
			this.txtData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtData.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
			this.txtData.Location = new System.Drawing.Point(12, 72);
			this.txtData.Multiline = true;
			this.txtData.Name = "txtData";
			this.txtData.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtData.Size = new System.Drawing.Size(674, 372);
			this.txtData.TabIndex = 1;
			// 
			// txtFile
			// 
			this.txtFile.Location = new System.Drawing.Point(82, 46);
			this.txtFile.Name = "txtFile";
			this.txtFile.Size = new System.Drawing.Size(142, 20);
			this.txtFile.TabIndex = 2;
			this.txtFile.Text = "SampleData.bin";
			// 
			// btnToBinary
			// 
			this.btnToBinary.Location = new System.Drawing.Point(12, 12);
			this.btnToBinary.Name = "btnToBinary";
			this.btnToBinary.Size = new System.Drawing.Size(103, 20);
			this.btnToBinary.TabIndex = 3;
			this.btnToBinary.Text = "Bin Serialize";
			this.btnToBinary.UseVisualStyleBackColor = true;
			this.btnToBinary.Click += new System.EventHandler(this.btnToBinary_Click);
			// 
			// btnFromBinary
			// 
			this.btnFromBinary.Location = new System.Drawing.Point(121, 12);
			this.btnFromBinary.Name = "btnFromBinary";
			this.btnFromBinary.Size = new System.Drawing.Size(103, 20);
			this.btnFromBinary.TabIndex = 3;
			this.btnFromBinary.Text = "Bin Deserialize";
			this.btnFromBinary.UseVisualStyleBackColor = true;
			this.btnFromBinary.Click += new System.EventHandler(this.btnFromBinary_Click);
			// 
			// btnCacheAll
			// 
			this.btnCacheAll.Location = new System.Drawing.Point(250, 12);
			this.btnCacheAll.Name = "btnCacheAll";
			this.btnCacheAll.Size = new System.Drawing.Size(103, 20);
			this.btnCacheAll.TabIndex = 3;
			this.btnCacheAll.Text = "Cache\'em all";
			this.btnCacheAll.UseVisualStyleBackColor = true;
			this.btnCacheAll.Click += new System.EventHandler(this.btnCacheAll_Click);
			// 
			// btnBuff
			// 
			this.btnBuff.Location = new System.Drawing.Point(359, 12);
			this.btnBuff.Name = "btnBuff";
			this.btnBuff.Size = new System.Drawing.Size(103, 20);
			this.btnBuff.TabIndex = 4;
			this.btnBuff.Text = "To Protocol Buffers";
			this.btnBuff.UseVisualStyleBackColor = true;
			this.btnBuff.Click += new System.EventHandler(this.btnBuff_Click);
			// 
			// btnBenchmark
			// 
			this.btnBenchmark.Location = new System.Drawing.Point(582, 12);
			this.btnBenchmark.Name = "btnBenchmark";
			this.btnBenchmark.Size = new System.Drawing.Size(103, 20);
			this.btnBenchmark.TabIndex = 5;
			this.btnBenchmark.Text = "Speed Benchmark";
			this.btnBenchmark.UseVisualStyleBackColor = true;
			this.btnBenchmark.Click += new System.EventHandler(this.btnBenchmark_Click);
			// 
			// btnClr
			// 
			this.btnClr.Location = new System.Drawing.Point(12, 46);
			this.btnClr.Name = "btnClr";
			this.btnClr.Size = new System.Drawing.Size(29, 20);
			this.btnClr.TabIndex = 6;
			this.btnClr.Text = "X";
			this.btnClr.UseVisualStyleBackColor = true;
			this.btnClr.Click += new System.EventHandler(this.btnClr_Click);
			// 
			// btnBonBecnhmark
			// 
			this.btnBonBecnhmark.Location = new System.Drawing.Point(473, 12);
			this.btnBonBecnhmark.Name = "btnBonBecnhmark";
			this.btnBonBecnhmark.Size = new System.Drawing.Size(103, 20);
			this.btnBonBecnhmark.TabIndex = 5;
			this.btnBonBecnhmark.Text = "BonBecnhmark";
			this.btnBonBecnhmark.UseVisualStyleBackColor = true;
			this.btnBonBecnhmark.Click += new System.EventHandler(this.btnBonBecnhmark_Click);
			// 
			// btnReadBenchmark
			// 
			this.btnReadBenchmark.Location = new System.Drawing.Point(473, 46);
			this.btnReadBenchmark.Name = "btnReadBenchmark";
			this.btnReadBenchmark.Size = new System.Drawing.Size(103, 20);
			this.btnReadBenchmark.TabIndex = 5;
			this.btnReadBenchmark.Text = "ReadBenchmark";
			this.btnReadBenchmark.UseVisualStyleBackColor = true;
			this.btnReadBenchmark.Click += new System.EventHandler(this.btnReadBenchmark_Click);
			// 
			// btnWriteBenchmark
			// 
			this.btnWriteBenchmark.Location = new System.Drawing.Point(582, 46);
			this.btnWriteBenchmark.Name = "btnWriteBenchmark";
			this.btnWriteBenchmark.Size = new System.Drawing.Size(103, 20);
			this.btnWriteBenchmark.TabIndex = 5;
			this.btnWriteBenchmark.Text = "WriteBenchmark";
			this.btnWriteBenchmark.UseVisualStyleBackColor = true;
			this.btnWriteBenchmark.Click += new System.EventHandler(this.btnWriteBenchmark_Click);
			// 
			// btnBin
			// 
			this.btnBin.Location = new System.Drawing.Point(47, 46);
			this.btnBin.Name = "btnBin";
			this.btnBin.Size = new System.Drawing.Size(29, 20);
			this.btnBin.TabIndex = 7;
			this.btnBin.Text = "0";
			this.btnBin.UseVisualStyleBackColor = true;
			this.btnBin.Click += new System.EventHandler(this.btnBin_Click);
			// 
			// btnStruxt
			// 
			this.btnStruxt.Location = new System.Drawing.Point(230, 46);
			this.btnStruxt.Name = "btnStruxt";
			this.btnStruxt.Size = new System.Drawing.Size(103, 20);
			this.btnStruxt.TabIndex = 3;
			this.btnStruxt.Text = "StructBenchmark";
			this.btnStruxt.UseVisualStyleBackColor = true;
			this.btnStruxt.Click += new System.EventHandler(this.btnStruxt_Click);
			// 
			// frmTest
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(698, 456);
			this.Controls.Add(this.btnBin);
			this.Controls.Add(this.btnClr);
			this.Controls.Add(this.btnWriteBenchmark);
			this.Controls.Add(this.btnReadBenchmark);
			this.Controls.Add(this.btnBonBecnhmark);
			this.Controls.Add(this.btnBenchmark);
			this.Controls.Add(this.btnBuff);
			this.Controls.Add(this.btnCacheAll);
			this.Controls.Add(this.btnFromBinary);
			this.Controls.Add(this.btnStruxt);
			this.Controls.Add(this.btnToBinary);
			this.Controls.Add(this.txtFile);
			this.Controls.Add(this.txtData);
			this.Name = "frmTest";
			this.Text = "Binary";
			this.Load += new System.EventHandler(this.frmTest_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtData;
		private System.Windows.Forms.TextBox txtFile;
		private System.Windows.Forms.Button btnToBinary;
		private System.Windows.Forms.Button btnFromBinary;
		private System.Windows.Forms.Button btnCacheAll;
		private System.Windows.Forms.Button btnBuff;
		private System.Windows.Forms.Button btnBenchmark;
		private System.Windows.Forms.Button btnClr;
		private System.Windows.Forms.Button btnBonBecnhmark;
		private System.Windows.Forms.Button btnReadBenchmark;
		private System.Windows.Forms.Button btnWriteBenchmark;
		private System.Windows.Forms.Button btnBin;
		private System.Windows.Forms.Button btnStruxt;
	}
}

