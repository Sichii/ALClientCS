namespace AL.Core.Extensions
{
    internal static class ArrayExtensions
    {
        internal static T[] RemoveAt<T>(this T[] arr, int index)
        {
            var newArr = new T[arr.Length - 1];

            for (var i = 0; i < index; i++)
                newArr[i] = arr[i];

            for (var i = index + 1; i < arr.Length;)
                newArr[i] = arr[++i];

            return newArr;
        }
    }
}