using System;
using System.Linq;

namespace QckMox.Tests.Integration.TestHelper
{
    internal static class ArgAny
    {
        public static ref string Except(params string[] exclusions)
        {
            return ref NSubstitute.Arg.Is<string>(s =>
                    exclusions.All(e =>
                        string.Equals(s, e, StringComparison.OrdinalIgnoreCase) == false));
        }
    }
}