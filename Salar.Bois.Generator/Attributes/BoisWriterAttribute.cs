namespace Salar.Bois.Generator.Attributes;

/// <summary>
/// Marks a partial method as the target for generated BOIS writer implementation.
/// Applied to partial method declarations in user code to indicate the generator
/// should produce a corresponding writer implementation.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class BoisWriterAttribute : Attribute
{
	public BoisWriterAttribute()
	{
	}
}
