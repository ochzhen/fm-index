namespace FmIndex.Abstract
{
    internal interface IBitVector
    {
        int RankZero(int len);
        int RankOne(int len);

        int Length { get; }
    }
}
