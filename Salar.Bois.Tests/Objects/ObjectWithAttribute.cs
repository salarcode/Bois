using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Salar.Bois.Tests.Objects
{
	[BoisContract(true, true)]
	class ObjectWithAttribute : IBaseType
	{
		[BoisMember(1, false)]
		public string Text1 { get; set; }
		public string TextField2;

		[BoisMember(0, true)]
		public BasicTypes1.SystemLanguage Language;

		[BoisMember(true)]
		public char AcceptChar { get; set; }
		public DateTime TestDate;
		[BoisMember(2, false)]
		public Color ForeColor { get; set; }
		public Guid TestGuid;
		[BoisMember(2, false)]
		public TimeSpan PassedTimeSpan { get; set; }

		public void Initialize()
		{
			Text1 = "Well, hello!";
			TextField2 = "This is Salar.Bois";
			Language = BasicTypes1.SystemLanguage.French;
			AcceptChar = 'c';
			TestDate = DateTime.Now.AddDays(7);
			ForeColor = SystemColors.ControlText;
			TestGuid = Guid.NewGuid();
			PassedTimeSpan = new TimeSpan(2, 3, 4, 5, 200);
		}
	}
}
