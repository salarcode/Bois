using System;
using System.IO;

namespace Salar.Bois.NetFx.Tests.Base
{
	public abstract class TestBase : IDisposable
	{
		private BoisSerializer _bois;
		private MemoryStream _mem;
		protected TestBase()
		{

		}

		public BoisSerializer Bois => _bois ?? (_bois = new BoisSerializer());

		public MemoryStream TestStream => _mem ?? (_mem = new MemoryStream());

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
