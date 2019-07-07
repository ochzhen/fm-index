namespace FmIndex.Abstract
{
    internal interface IAlphabetIds
    {
        int Length { get; }

        byte Anchor { get; }

        byte this[char c] { get; }

        bool TryConvert(char c, out byte value);
    }
}
