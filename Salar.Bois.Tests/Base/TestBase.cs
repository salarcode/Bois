using Newtonsoft.Json;
using ReflectionMagic;
using Salar.BinaryBuffers;
using Salar.BinaryBuffers.Compatibility;

namespace Salar.Bois.NetFx.Tests.Base;

public abstract class TestBase : IDisposable
{
	private dynamic _boisDynamic;
	private BoisSerializer _bois;
	private MemoryStream _mem;
	private BufferWriterBase _writer;
	private BufferReaderBase _reader;

	protected TestBase()
	{

	}

	public BoisSerializer Bois => _bois ??= new BoisSerializer();

	public dynamic BoisDynamic => _boisDynamic ??= _bois.AsDynamic();

	public MemoryStream TestStream => _mem ??= new MemoryStream();

	public BufferWriterBase Writer => _writer ??= new StreamBufferWriter(TestStream);

	public BufferReaderBase Reader => _reader ??= new StreamBufferReader(TestStream);

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
			throw new InvalidDataException("Failed to serialize to json, the expected value.", ex);
		}
		string actualStr;
		try
		{
			actualStr = JsonConvert.SerializeObject(actual);
		}
		catch (Exception ex)
		{
			throw new InvalidDataException("Failed to serialize to json, the actual value.", ex);
		}
		Assert.Equal(expectedStr, actualStr);
	}


	public void Dispose()
	{
		_mem?.Dispose();
		_mem = null;
	}
}
