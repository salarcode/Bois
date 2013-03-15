using System;
using System.Collections;
using System.Drawing;
using SharpTestsEx;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Salar.Bion.Tests
{
	public static class AssertionHelper
	{
		public static void AssetArrayEqual<T>(IList<T> expected, IList<T> actual)
		{
			actual.Count.Should().Be.EqualTo(expected.Count);

			if (typeof(T) == typeof(Color))
			{
				for (int i = 0; i < expected.Count; i++)
				{
					if (((Color)((object)expected[i])).ToArgb() != ((Color)((object)actual[i])).ToArgb())
					{
						Assert.Fail();
					}
				}
			}
			else
			{
				for (int i = 0; i < expected.Count; i++)
				{
					actual[i].Should().Be.EqualTo(expected[i]);
				}
			}
		}
		public static void AssetArrayEqual(IList expected, IList actual)
		{
			actual.Count.Should().Be.EqualTo(expected.Count);

			for (int i = 0; i < expected.Count; i++)
			{
				if (actual[i] is Color)
				{
					if (((Color)expected[i]).ToArgb() != ((Color)actual[i]).ToArgb())
					{
						Assert.Fail();
					}
					continue;
				}
				actual[i].Should().Be.EqualTo(expected[i]);
			}

		}
		public static void AssertMembersAreEqual<T>(T expected, T actual)
		{
			var type = typeof(T);
			var props =
				type.GetProperties(BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);

			foreach (PropertyInfo p in props)
			{
				object should = null, whatIs = null;
				try
				{
					should = p.GetValue(expected, null);
					whatIs = p.GetValue(actual, null);
				}
				catch { }

				if (p.PropertyType == typeof(Color))
				{
					if (((Color)should).ToArgb() != ((Color)whatIs).ToArgb())
					{
						string failMessage = string.Format("Property '{0}.{1}' of two specified objects are not equal.", type.Name, p.Name);
						Assert.Fail(failMessage);
					}
					continue;
				}

				should.Should().Be.EqualTo(whatIs);
				//Assert.AreEqual(should, whatIs,
				//				string.Format("Property '{0}.{1}' of two specified objects are not equal.", type.Name, p.Name));
			}
		}
	}
}
