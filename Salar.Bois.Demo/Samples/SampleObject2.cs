using System;
using System.Drawing;

namespace CompactBinarySerializer.Demo.Samples
{
	[Serializable]
	public class SampleObject2
	{
		public string Text { get; set; }
		public DateTime Dt { get; set; }
		public Color Clr { get; set; }
		public byte[] Data { get; set; }
		public Language Lng { get; set; }
		public string TextField;
		public string[] StrArray;
		public ChildOfYou[] Childs { get; set; }
		public Version VerInfo;

		public static SampleObject2 CreateObject()
		{
			var obj = new SampleObject2()
					   {
						   TextField = "Hi",
						   Lng = Language.Csharp,
						   Dt = DateTime.Now,
						   Text = "Hello ticks: " + DateTime.Now.Ticks.ToString(),
						   Clr = SystemColors.ActiveBorder,
						   Data = new byte[] { 66, 20, 30, 50, 90, 122, 50, 22, 0, 0, 0, 16, 19, 177 },
						   StrArray = new string[] { "T1", "Test2", "Something is not ok!", "Done" },
						   VerInfo = new Version("10.6.1099.0")
					   };
			obj.Childs = new ChildOfYou[]
				           {
					           new ChildOfYou()
						           {
							           Title = "CerateDynamic(Type ArrayType,Int32 Length)",
									   Dates =   new DateTime[] { DateTime.Now, DateTime.Now.AddDays(-45) }
						           }, 
							   new ChildOfYou()
								   {
							           Title = "Get Element?",
									   Dates = null
								   }, 
							   new ChildOfYou()
								   {
							           Title = null,
									   Dates =   new DateTime[] { DateTime.Now.AddYears(-6), DateTime.Now.AddDays(-14) }
								   }, 
				           };
			return obj;
		}
	}

	public enum Language
	{
		Cpp,
		C,
		Csharp,
		Javascript,
		Php
	}
	[Serializable]
	public class ChildOfYou
	{
		public string Title { get; set; }//= "wrote in message";
		public DateTime[] Dates;//= new DateTime[] { DateTime.Now, DateTime.Now.AddDays(-45) };
	}
}
