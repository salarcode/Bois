namespace Salar.Bois.Generator.Attributes;

/// <summary>
/// Marks a partial method as the target for generated BOIS reader implementation.
/// Applied to partial method declarations in user code to indicate the generator
/// should produce a corresponding reader implementation.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class BoisReaderAttribute : Attribute
{
	public BoisReaderAttribute()
	{
	}
}
