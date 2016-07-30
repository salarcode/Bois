using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using SalarCompactSerializer.Tests.Data;

namespace SalarCompactSerializer.DemoApp
{
	public partial class frmMain : Form
	{
		public frmMain()
		{
			InitializeComponent();
		}

		private void btnToString_Click(object sender, EventArgs e)
		{
			var obj = SampleObject2.CreateObject();
			var scs = new SalarCompactSerializer.CompactSerializer();
			scs.OrdinalNotation = chkOrdinal.Checked;
			txtData.Text = scs.Serialize(obj);

		}
		private void btnSCSObject_Click(object sender, EventArgs e)
		{
			var de = new JsonDeSerial();
			var obj = de.ToObject<SampleObject1>(txtData.Text);
			MessageBox.Show(obj.ToString(), "SampleObject1");
		}

		private void btnJSONToString_Click(object sender, EventArgs e)
		{
			var obj = SampleObject2.CreateObject();
			fastJSON.JSON.Instance.Parameters.UsingGlobalTypes = true;
			fastJSON.JSON.Instance.Parameters.UseExtensions = false;


			txtData.Text = fastJSON.JSON.Instance.ToJSON(obj);
		}

		private void btnJsonNetStr_Click(object sender, EventArgs e)
		{
			var s = new JsonSerializer();
 			s.NullValueHandling = NullValueHandling.Ignore;

			using (var w = new StringWriter())
			{
				var obj = SampleObject2.CreateObject();
				s.Serialize(w, obj);
				txtData.Text = w.ToString();


				
			}

			using (var sss=new StringReader(txtData.Text))
			using (var reader=new JsonTextReader(sss))
			{
				var oo = s.Deserialize<SampleObject2>(reader);
			}
		}
		private void btnBSON_Click(object sender, EventArgs e)
		{
			var s = new JsonSerializer();
			s.NullValueHandling = NullValueHandling.Ignore;

			using (var w = new BsonWriter(File.Create(@"E:\Programming\C#.NET\SalarCompactSerializer\SalarCompactSerializer.DemoApp\bson.dat")))
			{
				var obj = SampleObject2.CreateObject();
				s.Serialize(w, obj);
				txtData.Text = w.ToString();
			}
		}

		private void btnXML_Click(object sender, EventArgs e)
		{
			var s = new XmlSerializer(typeof(SampleObject2));

			using (var w = new StringWriter())
			{
				var obj = SampleObject2.CreateObject();
				//obj.Data =
				//	File.ReadAllBytes(
				//		@"E:\Programming\C#.NET\SalarCompactSerializer\SalarCompactSerializer\CompactSerializer.cs");
				s.Serialize(w, obj);
				txtData.Text = w.ToString();
			}
		}


	}
}
