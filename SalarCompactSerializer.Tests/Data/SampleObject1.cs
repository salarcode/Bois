using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SalarCompactSerializer.Tests.Data
{

	public class SampleObject1
	{
		static bool exotic = false;
		static bool dsser = false;
		static DataSet ds = new DataSet();

		public enum Gender
		{
			Male,
			Female
		}

		public SampleObject1()
		{
			items = new List<baseclass>();
			date = DateTime.Now;
			multilineString = @"
				AJKLjaskljLA
				   ahjksjkAHJKS سلام فارسی
				   AJKHSKJhaksjhAHSJKa
				   AJKSHajkhsjkHKSJKash
				   ASJKhasjkKASJKahsjk
            ";
			isNew = true;
			booleanValue = true;
			ordinaryDouble = 0.001;
			gender = Gender.Female;
			intarray = new int[5] { 1, 2, 3, 4, 5 };
		}
		public bool booleanValue { get; set; }
		public DateTime date { get; set; }
		public string multilineString { get; set; }
		public List<baseclass> items { get; set; }
		public decimal ordinaryDecimal { get; set; }
		public double ordinaryDouble { get; set; }
		public bool isNew { get; set; }
		public string laststring { get; set; }
		public Gender gender { get; set; }

		public DataSet dataset { get; set; }
		public Dictionary<string, baseclass> stringDictionary { get; set; }
		public Dictionary<baseclass, baseclass> objectDictionary { get; set; }
		public Dictionary<int, baseclass> intDictionary { get; set; }
		public Guid? nullableGuid { get; set; }
		public decimal? nullableDecimal { get; set; }
		public double? nullableDouble { get; set; }
		public Hashtable hash { get; set; }
		public baseclass[] arrayType { get; set; }
		public byte[] bytes { get; set; }
		public int[] intarray { get; set; }

		public static SampleObject1 CreateObject()
		{
			var c = new SampleObject1();

			c.booleanValue = true;
			c.ordinaryDecimal = 3;

			if (exotic)
			{
				c.nullableGuid = Guid.NewGuid();
				c.hash = new Hashtable();
				c.bytes = new byte[1024];
				c.stringDictionary = new Dictionary<string, baseclass>();
				c.objectDictionary = new Dictionary<baseclass, baseclass>();
				c.intDictionary = new Dictionary<int, baseclass>();
				c.nullableDouble = 100.003;

				if (dsser)
					c.dataset = ds;
				c.nullableDecimal = 3.14M;

				c.hash.Add(new class1("0", "hello", Guid.NewGuid()), new class2("1", "code", "desc"));
				c.hash.Add(new class2("0", "hello", "pppp"), new class1("1", "code", Guid.NewGuid()));

				c.stringDictionary.Add("name1", new class2("1", "code", "desc"));
				c.stringDictionary.Add("name2", new class1("1", "code", Guid.NewGuid()));

				c.intDictionary.Add(1, new class2("1", "code", "desc"));
				c.intDictionary.Add(2, new class1("1", "code", Guid.NewGuid()));

				c.objectDictionary.Add(new class1("0", "hello", Guid.NewGuid()), new class2("1", "code", "desc"));
				c.objectDictionary.Add(new class2("0", "hello", "pppp"), new class1("1", "code", Guid.NewGuid()));

				c.arrayType = new baseclass[2];
				c.arrayType[0] = new class1();
				c.arrayType[1] = new class2();
			}


			c.items.Add(new class1("1", "1", Guid.NewGuid()));
			c.items.Add(new class2("2", "2", "desc1"));
			c.items.Add(new class1("3", "3", Guid.NewGuid()));
			c.items.Add(new class2("4", "4", "desc2"));

			c.laststring = "" + DateTime.Now;

			return c;
		}
	}



	public class baseclass
	{
		public string Name { get; set; }
		public string Code { get; set; }
	}

	public class class1 : baseclass
	{
		public class1() { }
		public class1(string name, string code, Guid g)
		{
			Name = name;
			Code = code;
			guid = g;
		}
		public Guid guid { get; set; }
	}

	public class class2 : baseclass
	{
		public class2() { }
		public class2(string name, string code, string desc)
		{
			Name = name;
			Code = code;
			description = desc;
		}
		public string description { get; set; }
	}

}


