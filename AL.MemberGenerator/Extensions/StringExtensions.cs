// nullable is not enabled in this project, so nullability can only be expressed via JetBrains attributes
// ReSharper disable AnnotateNotNullTypeMember
// ReSharper disable AnnotateNotNullParameter
// ReSharper disable AnnotateCanBeNullParameter

namespace AL.MemberGenerator.Extensions;

public static class StringExtensions
{
    public static string ToCodeFormat(this string str)
    {
        static IEnumerable<char> InternalIterate(string source)
        {
            var capitolize = false;
            var first = true;

            foreach (var @char in source)
                if (first)
                {
                    first = false;

                    if (char.IsDigit(@char))
                    {
                        capitolize = true;

                        yield return '_';
                        yield return @char;
                    } else
                        yield return char.ToUpper(@char);
                } else if (@char == '_')
                    capitolize = true;
                else if (char.IsDigit(@char))
                {
                    capitolize = true;

                    yield return @char;
                } else if (capitolize)
                {
                    yield return char.ToUpper(@char);

                    capitolize = false;
                } else
                    yield return @char;
        }

        return new string(
            InternalIterate(str)
                .ToArray());
    }

    public static string ToUpperFirstLetter(this string source)
    {
        if (string.IsNullOrEmpty(source))
            return string.Empty;

        // convert to char array of the string
        var letters = source.ToCharArray();

        // upper case the first char
        letters[0] = char.ToUpper(letters[0]);

        // return the array made of the new char array
        return new string(letters);
    }
}