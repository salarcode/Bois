using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Salar.SerializersStudy
{
	public partial class frmRunner : Form
	{
		public frmRunner()
		{
			InitializeComponent();
		}


		void RunTheTest()
		{
			var result = StudyRunner.RunBenchmark();

			txtResult.Text = string.Join(Environment.NewLine, result);
		}

		private void btnRepeat_Click(object sender, EventArgs e)
		{
			RunTheTest();
		}

		private void frmRunner_Load(object sender, EventArgs e)
		{
			RunTheTest();
		}
	}
}
