using Newtonsoft.Json;
using ReflectionMagic;
using System;
using System.IO;
using Xunit;

namespace Salar.Bois.NetFx.Tests.Base
{
	public abstract class TestBase : IDisposable
	{
		private dynamic _boisDynamic;
		private BoisSerializer _bois;
		private MemoryStream _mem;
		private BinaryWriter _writer;
		private BinaryReader _reader;

		protected TestBase()
		{

		}

		public BoisSerializer Bois => _bois ?? (_bois = new BoisSerializer());

		public dynamic BoisDynamic => _boisDynamic ?? (_boisDynamic = _bois.AsDynamic());

		public MemoryStream TestStream => _mem ?? (_mem = new MemoryStream());

		public BinaryWriter Writer => _writer ?? (_writer = new BinaryWriter(TestStream));

		public BinaryReader Reader => _reader ?? (_reader = new BinaryReader(TestStream));

		public void ResetBois()
		{
			TestStream.Position = 0;
		}

		public void ResetStream()
		{
			TestStream.Position = 0;
		}


		public void SerializeAreEqual<T>(T expected, T actual)
		{
			string expectedStr;
			try
			{
				expectedStr = JsonConvert.SerializeObject(expected);
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to serialize to json, the expected value.", ex);
			}
			string actualStr;
			try
			{
				actualStr = JsonConvert.SerializeObject(actual);
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to serialize to json, the actual value.", ex);
			}
			Assert.Equal(expectedStr, actualStr);
		}


		public void Dispose()
		{
			_mem?.Dispose();
			_mem = null;
		}
	}
}
