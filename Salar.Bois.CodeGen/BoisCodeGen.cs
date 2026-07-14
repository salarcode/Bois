using System;
using System.Collections.Generic;
using System.Text;

namespace Salar.Bois.CodeGen;

public static class BoisCodeGen
{
    public static Encoding DefaultEncoding
    {
        get;
        set => field = value ?? throw new ArgumentNullException(nameof(value));
    } = Encoding.UTF8;
}
