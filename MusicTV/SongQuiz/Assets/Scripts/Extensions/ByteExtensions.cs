using System;

namespace Assets.Scripts.Extensions
{
    public static class ByteExtensions
    {
        public static float[] ToFloat(this byte[] array)
        {
            float[] floatArr = new float[array.Length / 4];
            for (int i = 0; i < floatArr.Length; i++)
            {
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(array, i * 4, 4);
                floatArr[i] = BitConverter.ToSingle(array, i * 4);
            }
            return floatArr;
        }
    }
}