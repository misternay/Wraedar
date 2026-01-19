using System;
using System.Drawing;
using System.Numerics;
using System.Text.RegularExpressions;

namespace DieselExileTools;

public static class Extensions
{
	private const float Offset = 5.4347825f;

	public static Vector2 GridToWorld(this Vector2 v)
	{
		return new Vector2(v.X / 0.092f + 5.4347825f, v.Y / 0.092f + 5.4347825f);
	}

	public static Vector2 WorldToGrid(this Vector3 v)
	{
		return new Vector2((float)Math.Floor(v.X * 0.092f), (float)Math.Floor(v.Y * 0.092f));
	}

	public static Vector2 WorldToGrid(this Vector2 v)
	{
		return new Vector2((float)Math.Floor(v.X * 0.092f), (float)Math.Floor(v.Y * 0.092f));
	}

	public static Vector4 ToVector4(this Color c)
	{
		return new Vector4((float)(int)c.R / 255f, (float)(int)c.G / 255f, (float)(int)c.B / 255f, (float)(int)c.A / 255f);
	}

	public static uint ToImGui(this Color c)
	{
		return (uint)((c.A << 24) | (c.B << 16) | (c.G << 8) | c.R);
	}

	public static DXTHSLA ToHSLA(this Color c)
	{
		return DXTHSLA.FromRGBA(c);
	}

	public static string ToColorCode(this Color c)
	{
		return $"|c{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";
	}

	public static string ToHEX(this Color color)
	{
		if (color.A != byte.MaxValue)
		{
			return $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
		}
		return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
	}

	public static Color Darken(this Color color, float amount)
	{
		DXTHSLA dXTHSLA = color.ToHSLA();
		dXTHSLA.L = Math.Clamp(dXTHSLA.L - amount, 0f, 1f);
		return dXTHSLA.ToRGBA();
	}

	public static Color Lighten(this Color color, float amount)
	{
		DXTHSLA dXTHSLA = color.ToHSLA();
		dXTHSLA.L = Math.Clamp(dXTHSLA.L + amount, 0f, 1f);
		return dXTHSLA.ToRGBA();
	}

	public static Color Saturate(this Color color, float amount)
	{
		DXTHSLA dXTHSLA = color.ToHSLA();
		dXTHSLA.S = Math.Clamp(dXTHSLA.S + amount, 0f, 1f);
		return dXTHSLA.ToRGBA();
	}

	public static Color Desaturate(this Color color, float amount)
	{
		DXTHSLA dXTHSLA = color.ToHSLA();
		dXTHSLA.S = Math.Clamp(dXTHSLA.S - amount, 0f, 1f);
		return dXTHSLA.ToRGBA();
	}

	public static Color WithAlpha(this Color color, float alpha)
	{
		DXTHSLA dXTHSLA = color.ToHSLA();
		dXTHSLA.A = Math.Clamp(alpha, 0f, 1f);
		return dXTHSLA.ToRGBA();
	}

	public static Color Invert(this Color color)
	{
		return Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B);
	}

	public static Color Grayscale(this Color color)
	{
		int num = (int)((double)(int)color.R * 0.3 + (double)(int)color.G * 0.59 + (double)(int)color.B * 0.11);
		return Color.FromArgb(color.A, num, num, num);
	}

	public static bool Like(this string str, string pattern)
	{
		return pattern.ToLikeRegex().IsMatch(str);
	}

	public static Regex ToLikeRegex(this string pattern)
	{
		return new Regex("^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
	}
}
