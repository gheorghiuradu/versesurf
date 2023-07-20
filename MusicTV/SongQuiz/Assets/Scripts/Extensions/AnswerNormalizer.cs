using SharedDomain;

namespace Assets.Scripts.Extensions
{
    public static class AnswerNormalizer
    {
        private static string NormalizeAnswer(string answer, string correctAnswer)
        {
            var firstChar = correctAnswer[0];
            var toReplace = answer[0];
            if (char.IsUpper(firstChar))
            {
                return $"{toReplace.ToString().ToUpper()}{answer.Substring(1, answer.Length - 1)}";
            }
            else
            {
                return $"{toReplace.ToString().ToLower()}{answer.Substring(1, answer.Length - 1)}";
            }
        }

        public static void Normalize(this Answer answer, string correctAnswer)
            => answer.Name = NormalizeAnswer(answer.Name, correctAnswer);

        public static void Normalize(this SpeedAnswer answer, string correctAnswer)
            => answer.Name = NormalizeAnswer(answer.Name, correctAnswer);
    }
}