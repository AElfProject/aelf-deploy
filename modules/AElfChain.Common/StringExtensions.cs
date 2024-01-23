using Google.Protobuf.Collections;

namespace AElf;

public static class StringExtensions
{
    public static void MergeFrom<T1, T2>(this MapField<T1, T2> field, MapField<T1, T2> others)
    {
        field.Add(others);
    }
}