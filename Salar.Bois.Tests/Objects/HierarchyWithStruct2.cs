using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salar.Bois.Tests.Objects
{
	public class HierarchyWithStruct2 : IBaseType
	{
		public const int ResponseHeaderVersion = 1;
 		public int Ver { get; set; }
 		public string CEnc { get; set; }
 		public long CLen { get; set; }
 		public string CType { get; set; }
 		public string ChSet { get; set; }
 		public int SCode { get; set; }
 		public string SDesc { get; set; }

 		public List<KeyValueString> HDR { get; set; }

		public HierarchyWithStruct2()
		{
			HDR = new List<KeyValueString>();
			Ver = -1;
		}

		public void Initialize()
		{
			Ver = 12;
			CEnc = "Encoding";
			CLen = 1024;
			CType = "image/jpeg";
			ChSet = "";
			SCode = 200;
			SDesc = "OK";
			HDR.Add(new KeyValueString("MLast-Modified", "Thu, 22 Nov 2012 13:20:36 GMT"));
			HDR.Add(new KeyValueString("Date", "Thu, 02 May 2013 15:08:13 GMT"));
			HDR.Add(new KeyValueString("Cache-Control", ""));
			HDR.Add(new KeyValueString("max-age=2592000", ""));
			HDR.Add(new KeyValueString("Expires", "Sat, 01 Jun 2013 15:08:13 GMT"));
		}
	}

	public struct KeyValueString
	{
		public string Key;
		public string Value;

		public KeyValueString(string key, string value)
		{
			Key = key;
			Value = value;
		}
		public override string ToString()
		{
			return Key + ": " + Value;
		}

	}

}
