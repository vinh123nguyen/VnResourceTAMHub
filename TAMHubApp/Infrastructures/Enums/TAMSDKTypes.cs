using System.Collections.Immutable;

namespace TAMHubApp.Infrastructures.Enums
{
    public enum TAMSDKTypes
    {
        E_ZKTECO,
        E_REALAND
    }
    public static class TAMEnumConverter
    {
        public static readonly ImmutableDictionary<int, string> ZKTecoEnumConvert = ImmutableDictionary<int, string>.Empty
        .Add(0, "I")
        .Add(1, "O")
        .Add(2, "o")
        .Add(3, "i")
        .Add(4, "I")
        .Add(5, "O");
    }
}
