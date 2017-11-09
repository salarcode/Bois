using System;
using System.Drawing;

namespace Salar.Bois.Tests.Objects
{
	class BasicTypes1 : IBaseType
	{
		public enum SystemLanguage
		{
			English,
			French,
			Germany,
			Turkey
		}

		public DBNull DbValueNull { get; set; }
		public string Text { get; set; }
		public string Text2 { get; set; }
		public string Text3 { get; set; }
		public string Text4 { get; set; }
		public string Text5 { get; set; }
		public SystemLanguage Language { get; set; }
		public char AcceptChar { get; set; }
		public DateTime TestDate { get; set; }
		public Color ForeColor { get; set; }
		public Guid TestGuid { get; set; }
		public TimeSpan PassedTimeSpan { get; set; }
		public Version TestVersion { get; set; }
		public SystemLanguage UiLanguage { get; set; }

		public void Initialize()
		{
			Text = "Well, hello!";
			Text2 = "This is Salar.Bois";
			Text3 = "";
			Text4 = null;
			Text5 = "A binary serializer";
			Language = SystemLanguage.French;
			AcceptChar = 'c';
			TestDate = DateTime.Now.AddDays(7);
			ForeColor = Color.MidnightBlue;
			TestGuid = Guid.NewGuid();
			PassedTimeSpan = new TimeSpan(2, 3, 4, 5, 200);
			TestVersion = new Version(2, 3, 4, 600);
			DbValueNull = DBNull.Value;
			UiLanguage = SystemLanguage.Germany;
		}
	}
}
