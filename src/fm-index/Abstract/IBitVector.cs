namespace FmIndex.Abstract
{
    internal interface IBitVector
    {
        int Length { get; }

        bool this[int idx] { get; }

        int RankZero(int len);

        int RankOne(int len);
    }
}
