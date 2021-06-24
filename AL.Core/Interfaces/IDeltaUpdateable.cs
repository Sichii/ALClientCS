using Newtonsoft.Json;

namespace AL.Core.Interfaces
{
    public interface IDeltaUpdateable : IMutable
    {
        [JsonIgnore]
        long Delta { get; set; }
        void Update(long delta);
    }
}