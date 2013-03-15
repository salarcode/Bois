using System;
using System.Drawing;

namespace Salar.Bon.Tests.Objects
{
	class BasicTypes1Nullable : IBaseType
	{
		public enum SystemLanguage
		{
			English,
			French,
			Germany,
			Turkey
		}

		public DBNull DbValueNull_Null { get; set; }
		public string Text_Null { get; set; }
		public SystemLanguage? Language_Null { get; set; }
		public char? AcceptChar_Null { get; set; }
		public DateTime? TestDate_Null { get; set; }
		public Color? ForeColor_Null { get; set; }
		public Guid? TestGuid_Null { get; set; }
		public TimeSpan? PassedTimeSpan_Null { get; set; }
		public Version TestVersion_Null { get; set; }
		public SystemLanguage? EndLanguage_Null { get; set; }

		public DBNull DbValueNull { get; set; }
		public string Text { get; set; }
		public SystemLanguage Language { get; set; }
		public char AcceptChar { get; set; }
		public DateTime TestDate { get; set; }
		public Color ForeColor { get; set; }
		public Guid TestGuid { get; set; }
		public TimeSpan PassedTimeSpan { get; set; }
		public Version TestVersion { get; set; }
		public SystemLanguage EndLanguage { get; set; }
		public void Initialize()
		{
			Text = "Well, hello!";
			Language = SystemLanguage.French;
			AcceptChar = 'c';
			TestDate = DateTime.Now.AddDays(7);
			ForeColor = SystemColors.ControlText;
			TestGuid = Guid.NewGuid();
			PassedTimeSpan = new TimeSpan(2, 3, 4, 5, 200);
			TestVersion = new Version(2, 3, 4, 600);
			DbValueNull = DBNull.Value;
			EndLanguage = SystemLanguage.Germany;
		}
	}
}
