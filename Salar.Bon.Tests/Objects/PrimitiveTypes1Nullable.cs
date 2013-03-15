namespace Salar.Bon.Tests.Objects
{
	class PrimitiveTypes1Nullable : IBaseType
	{
		public byte? Item1_Null { get; set; }
		public short? Item2_Null { get; set; }
		public int? Item3_Null { get; set; }
		public long? Item4_Null { get; set; }
		public float? Item5_Null { get; set; }
		public decimal? Item6_Null { get; set; }
		public double? Item7_Null { get; set; }
		public bool? Item8_Null { get; set; }
		public double? Item9_Null { get; set; }
		public sbyte? Item10_Null { get; set; }
		public ushort? Item11_Null { get; set; }
		public uint? Item12_Null { get; set; }
		public string Text_Null { get; set; }
		public ulong? Item13_Null { get; set; }

		public byte? Item1 { get; set; }
		public short? Item2 { get; set; }
		public int? Item3 { get; set; }
		public long? Item4 { get; set; }
		public float? Item5 { get; set; }
		public decimal? Item6 { get; set; }
		public double? Item7 { get; set; }
		public bool? Item8 { get; set; }
		public double? Item9 { get; set; }
		public sbyte? Item10 { get; set; }
		public ushort? Item11 { get; set; }
		public uint? Item12 { get; set; }
		public string Text { get; set; }
		public ulong? Item13 { get; set; }


		public void Initialize()
		{
			Item1 = 10;
			Item2 = 202;
			Item3 = 203;
			Item4 = 204;
			Item5 = 205.091f;
			Item6 = 206.123m;
			Item7 = 207.456;
			Item8 = true;
			Item9 = 209;
			Item10 = 120;
			Item11 = 2011;
			Item12 = 2012;
			Item13 = 2013;
			Text = "Hello there !";
		}
	}
}
