namespace Assets.Scripts.Extensions
{
    public static class DoubleExtensions
    {
        public static bool IsEqualTo(this double a, double b, double margin)
        {
            return System.Math.Abs(a - b) < margin;
        }

        public static bool IsEqualTo(this double a, double b)
        {
            return System.Math.Abs(a - b) < double.Epsilon;
        }

        public static bool IsEqualTo(this float x1, float x2)
        {
            return System.Math.Abs(x1 - x2) < 0.001 * x1;
        }
    }
}