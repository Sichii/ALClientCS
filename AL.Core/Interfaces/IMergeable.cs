namespace AL.Core.Interfaces
{
    public interface IMergeable<in T>
    {
        void Merge(T other);
    }
}