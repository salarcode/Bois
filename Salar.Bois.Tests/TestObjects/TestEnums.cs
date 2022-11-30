using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salar.Bois.NetFx.Tests.TestObjects
{
	public enum TestEnumsNormal
	{
		Ten = 10,
		Eleven,
		Twelve
	}

	public enum TestsEnumUInt : uint
	{
		UInt1 = 1,
		UInt2 = 2
	}

	public enum TestsEnumByte : byte
	{
		Byte1 = 1,
		Byte2 = 2
	}

	public enum TestsEnumShort : short
	{
		Short1 = 1,
		Short2 = 2
	}

	public enum TestsEnumInt64 : long
	{
		Long1 = 1L,
		long2 = 2L
	}
}
