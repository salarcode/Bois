namespace SalarCompactSerializer.DemoApp
{
	partial class frmMain
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
			this.btnToString = new System.Windows.Forms.Button();
			this.btnJSONToString = new System.Windows.Forms.Button();
			this.btnJsonNetStr = new System.Windows.Forms.Button();
			this.chkOrdinal = new System.Windows.Forms.CheckBox();
			this.btnSCSObject = new System.Windows.Forms.Button();
			this.btnXML = new System.Windows.Forms.Button();
			this.btnBSON = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// txtData
			// 
			this.txtData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtData.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
			this.txtData.Location = new System.Drawing.Point(123, 12);
			this.txtData.Multiline = true;
			this.txtData.Name = "txtData";
			this.txtData.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtData.Size = new System.Drawing.Size(520, 341);
			this.txtData.TabIndex = 0;
			// 
			// btnToString
			// 
			this.btnToString.Location = new System.Drawing.Point(12, 33);
			this.btnToString.Name = "btnToString";
			this.btnToString.Size = new System.Drawing.Size(105, 23);
			this.btnToString.TabIndex = 1;
			this.btnToString.Text = "SCS ToString";
			this.btnToString.UseVisualStyleBackColor = true;
			this.btnToString.Click += new System.EventHandler(this.btnToString_Click);
			// 
			// btnJSONToString
			// 
			this.btnJSONToString.Location = new System.Drawing.Point(12, 62);
			this.btnJSONToString.Name = "btnJSONToString";
			this.btnJSONToString.Size = new System.Drawing.Size(105, 23);
			this.btnJSONToString.TabIndex = 1;
			this.btnJSONToString.Text = "fastJSON ToString";
			this.btnJSONToString.UseVisualStyleBackColor = true;
			this.btnJSONToString.Click += new System.EventHandler(this.btnJSONToString_Click);
			// 
			// btnJsonNetStr
			// 
			this.btnJsonNetStr.Location = new System.Drawing.Point(12, 91);
			this.btnJsonNetStr.Name = "btnJsonNetStr";
			this.btnJsonNetStr.Size = new System.Drawing.Size(105, 23);
			this.btnJsonNetStr.TabIndex = 1;
			this.btnJsonNetStr.Text = "Json.Net ToString";
			this.btnJsonNetStr.UseVisualStyleBackColor = true;
			this.btnJsonNetStr.Click += new System.EventHandler(this.btnJsonNetStr_Click);
			// 
			// chkOrdinal
			// 
			this.chkOrdinal.AutoSize = true;
			this.chkOrdinal.Location = new System.Drawing.Point(12, 15);
			this.chkOrdinal.Name = "chkOrdinal";
			this.chkOrdinal.Size = new System.Drawing.Size(60, 17);
			this.chkOrdinal.TabIndex = 2;
			this.chkOrdinal.Text = "Ordinal";
			this.chkOrdinal.UseVisualStyleBackColor = true;
			// 
			// btnSCSObject
			// 
			this.btnSCSObject.Location = new System.Drawing.Point(12, 179);
			this.btnSCSObject.Name = "btnSCSObject";
			this.btnSCSObject.Size = new System.Drawing.Size(105, 23);
			this.btnSCSObject.TabIndex = 3;
			this.btnSCSObject.Text = "SCS Object";
			this.btnSCSObject.UseVisualStyleBackColor = true;
			this.btnSCSObject.Click += new System.EventHandler(this.btnSCSObject_Click);
			// 
			// btnXML
			// 
			this.btnXML.Location = new System.Drawing.Point(12, 150);
			this.btnXML.Name = "btnXML";
			this.btnXML.Size = new System.Drawing.Size(105, 23);
			this.btnXML.TabIndex = 1;
			this.btnXML.Text = "To XML ";
			this.btnXML.UseVisualStyleBackColor = true;
			this.btnXML.Click += new System.EventHandler(this.btnXML_Click);
			// 
			// btnBSON
			// 
			this.btnBSON.Location = new System.Drawing.Point(12, 120);
			this.btnBSON.Name = "btnBSON";
			this.btnBSON.Size = new System.Drawing.Size(105, 23);
			this.btnBSON.TabIndex = 1;
			this.btnBSON.Text = "Bson.Net ToString";
			this.btnBSON.UseVisualStyleBackColor = true;
			this.btnBSON.Click += new System.EventHandler(this.btnBSON_Click);
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(655, 365);
			this.Controls.Add(this.btnSCSObject);
			this.Controls.Add(this.chkOrdinal);
			this.Controls.Add(this.btnXML);
			this.Controls.Add(this.btnBSON);
			this.Controls.Add(this.btnJsonNetStr);
			this.Controls.Add(this.btnJSONToString);
			this.Controls.Add(this.btnToString);
			this.Controls.Add(this.txtData);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
			this.Name = "frmMain";
			this.Text = "Demo App";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtData;
		private System.Windows.Forms.Button btnToString;
		private System.Windows.Forms.Button btnJSONToString;
		private System.Windows.Forms.Button btnJsonNetStr;
		private System.Windows.Forms.CheckBox chkOrdinal;
		private System.Windows.Forms.Button btnSCSObject;
		private System.Windows.Forms.Button btnXML;
		private System.Windows.Forms.Button btnBSON;
	}
}

