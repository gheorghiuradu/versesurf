namespace LicensingService
{
    internal static class StringExtensions
    {
        internal static string ReverseWords(this string str)
        {
            var i = str.Length - 1;
            int start, end = i + 1;
            var result = "";

            while (i >= 0)
            {
                if (str[i] == ' ')
                {
                    start = i + 1;
                    while (start != end)
                        result += str[start++];

                    result += ' ';

                    end = i;
                }
                i--;
            }

            start = 0;
            while (start != end)
                result += str[start++];

            return result;
        }
    }
}