namespace FmIndex.Abstract
{
    internal interface IAlphabetIds
    {
        int Length { get; }

        char Anchor { get; }

        char this[char c] { get; }

        bool TryConvert(char c, out char value);
    }
}
