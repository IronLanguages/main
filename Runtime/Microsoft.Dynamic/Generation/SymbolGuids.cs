using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Scripting.Generation {
    internal static class SymbolGuids {
        internal static readonly Guid LanguageType_ILAssembly =
            new Guid(-1358664493, -12063, 0x11d2, 0x97, 0x7c, 0, 160, 0xc9, 180, 0xd5, 12);

        internal static readonly Guid DocumentType_Text =
            new Guid(0x5a869d0b, 0x6611, 0x11d3, 0xbd, 0x2a, 0, 0, 0xf8, 8, 0x49, 0xbd);

        internal static readonly Guid LanguageVendor_Microsoft =
            new Guid(-1723120188, -6423, 0x11d2, 0x90, 0x3f, 0, 0xc0, 0x4f, 0xa3, 2, 0xa1);
    }
}
