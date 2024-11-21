#region
using System.Collections;
using System.Collections.Generic;
using AL.Client.Interfaces;
using AL.SocketClient.Model;
#endregion

namespace AL.Client.Model;

/// <seealso cref="ICompoundableGrouping{T}" />
/// <inheritdoc cref="ICompoundableGrouping{T}" />
public sealed record CompoundableGrouping<T> : ICompoundableGrouping<T> where T: IIndexer<Item>
{
    internal IReadOnlyList<T> Items { get; init; } = new List<T>();
    public int Level { get; init; }
    public string Name { get; init; } = null!;
    public int Count => Items.Count;
    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public T this[int index] => Items[index];
}