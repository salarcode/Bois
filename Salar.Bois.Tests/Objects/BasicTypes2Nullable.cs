using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Salar.Bois.Tests.Objects
{
	class BasicTypes2Nullable : IBaseType
	{
		public enum SystemLanguage
		{
			English,
			French,
			Germany,
			Turkey
		}

		public SystemLanguage? EndLanguage1_Null { get; set; }
		public SystemLanguage? EndLanguage2_Null { get; set; }
		public string Text { get; set; }
		public SystemLanguage EndLanguage { get; set; }
		public void Initialize()
		{
			Text = "Well, hello!";
			EndLanguage1_Null = null;
			EndLanguage2_Null = SystemLanguage.French;
			EndLanguage = SystemLanguage.Germany;
		}
	}
}
