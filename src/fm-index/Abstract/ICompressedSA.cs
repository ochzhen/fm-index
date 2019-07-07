namespace FmIndex.Abstract
{
    internal interface ICompressedSA
    {
        bool IsStored(int idx);

        int GetStoredPosition(int idx);
    }
}
