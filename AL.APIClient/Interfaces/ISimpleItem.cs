namespace AL.APIClient.Interfaces;

/// <summary>
///     Represents the simplest information possible about an item.
/// </summary>
public interface ISimpleItem
{
    /// <summary>
    ///     The name of the item.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     The quantity of the item.
    /// </summary>
    int Quantity { get; }
}