using ReflectionMagic;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;

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

		public BinaryReader Reader=> _reader ?? (_reader = new BinaryReader(TestStream));

		public void ResetBois()
		{
			TestStream.Position = 0;
		}

		public void ResetStream()
		{
			TestStream.Position = 0;
		}


		public void Dispose()
		{
			_mem?.Dispose();
			_mem = null;
		}
	}
}
