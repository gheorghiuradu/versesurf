using System;
using UnityEngine;

namespace Assets.Scripts.Extensions
{
    public static class ResolutionExtensions
    {
		public static Vector2 GetAspectRatio(this Resolution resolution)
		{
			float f = (float)resolution.width / (float)resolution.height;
			int i = 0;
			while (true)
			{
				i++;
				if (System.Math.Round(f * i, 2) == Mathf.RoundToInt(f * i))
					break;
			}
			return new Vector2((float)System.Math.Round(f * i, 2), i);
		}

		public static Resolution ToResolution(this string resolution)
        {
			return new UnityEngine.Resolution
			{
				height = int.Parse(resolution.Split(new string[] { " x " }, StringSplitOptions.None)[1]),
				width = int.Parse(resolution.Split(new string[] { " x " }, StringSplitOptions.None)[0]),
				refreshRate = Screen.currentResolution.refreshRate
			};
		}
	}
}
