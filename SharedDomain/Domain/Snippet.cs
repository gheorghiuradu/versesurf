using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SharedDomain.Tests")]

namespace SharedDomain.Domain
{
    public class Snippet
    {
        internal static readonly string[] IllegalStrings = new[] { "]", "}", ")", "...", "?", "!", ".", "\"", "\'",
                                                                    "¿", "¡", ",", "(" , "{", "[" };

        public string Question { get; }
        public string Answer { get; }
        public string FullText { get; }

        public Snippet(string text, Mode mode = Mode.LastWord)
        {
            var toReplace = this.GetBlankWords(text, mode);

            this.FullText = text;
            this.Question = this.ReplaceLastOccurrence(text, toReplace, "_____");
            this.Answer = toReplace;
        }

        public Snippet(string text)
        {
            if (!text.Contains("{"))
            {
                throw new System.InvalidOperationException("Cannot create auto-snippet if text does not contain {}");
            }

            var toReplace = text.Split('{', '}')[1];
            this.FullText = text;
            this.Question = this.ReplaceLastOccurrence(text, $"{{{toReplace}}}", "_____");
            this.Answer = toReplace;
        }

        private string GetBlankWords(string text, Mode mode)
        {
            var blanks = text.Substring(text.LastIndexOf(' ') + 1);
            blanks = this.TrimIllegalStrings(blanks);

            if (mode == Mode.LastTwoWords)
            {
                var words = text.Split(' ');
                if (words.Length > 2)
                {
                    var wordBeforeLast = words[words.Length - 2];
                    if (wordBeforeLast.Length <= 3)
                    {
                        wordBeforeLast = this.TrimIllegalStrings(wordBeforeLast);
                        blanks = $"{wordBeforeLast} {blanks}";
                    }
                }
            }

            return blanks;
        }

        private string TrimIllegalStrings(string word)
        {
            while (IllegalStrings.Any(illegal => word.Contains(illegal)))
            {
                foreach (var illegalString in IllegalStrings)
                {
                    if (word.EndsWith(illegalString))
                    {
                        word = word.TrimEnd(illegalString.ToCharArray());
                    }
                    if (word.StartsWith(illegalString))
                    {
                        word = word.TrimStart(illegalString.ToCharArray());
                    }
                }
            }

            return word;
        }

        private string ReplaceLastOccurrence(string source, string find, string replace)
        {
            var place = source.LastIndexOf(find);

            if (place == -1)
                return source;

            return source.Remove(place, find.Length).Insert(place, replace);
        }

        public enum Mode
        {
            LastWord,
            LastTwoWords
        }
    }
}