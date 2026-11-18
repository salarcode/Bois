using System;

/* 
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 * 
 * https://github.com/salarcode/Bois
 * Mozilla Public License v2
 */
namespace Salar.Bois.CodeGen;

/// <summary>
/// Types for <see cref="BoisReservedAttribute"/>.
/// </summary>
public static class BoisSkip
{
    /// <summary>
    /// Use <c>typeof(BoisSkip.Contract)</c> in <see cref="BoisReservedAttribute"/> when the removed member was a
    /// versioned object. The reader skips it by its size, so the old class can be deleted.
    /// </summary>
    public sealed class Contract
    {
        private Contract() { }
    }
}
