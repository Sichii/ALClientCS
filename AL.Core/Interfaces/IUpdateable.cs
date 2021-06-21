namespace AL.Core.Interfaces
{
    public interface IUpdateable<in TPartial>
    {
        void Update(TPartial other);
    }
}