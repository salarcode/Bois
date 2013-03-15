using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Salar.Bion.Tests.Objects
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
			Language = SystemLanguage.French;
			AcceptChar = 'c';
			TestDate = DateTime.Now.AddDays(7);
			ForeColor = SystemColors.ControlText;
			TestGuid = Guid.NewGuid();
			PassedTimeSpan = new TimeSpan(2, 3, 4, 5, 200);
			TestVersion = new Version(2, 3, 4, 600);
			DbValueNull = DBNull.Value;
			UiLanguage = SystemLanguage.Germany;
		}
	}
}
