using System.IO;

namespace Salar.Bois.BenchmarksBase;

public interface IBenchmark
{
	void Serialize();

	void Deserialize();
}
