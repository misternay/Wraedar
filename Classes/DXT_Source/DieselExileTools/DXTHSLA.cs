using System;
using System.Drawing;

namespace DieselExileTools;

public struct DXTHSLA
{
	private float _h;

	private float _s;

	private float _l;

	private float _a;

	private const float Tolerance = 0.003921569f;

	public float H
	{
		get
		{
			return _h;
		}
		set
		{
			_h = Math.Clamp(value, 0f, 360f);
		}
	}

	public float S
	{
		get
		{
			return _s;
		}
		set
		{
			_s = Math.Clamp(value, 0f, 1f);
		}
	}

	public float L
	{
		get
		{
			return _l;
		}
		set
		{
			_l = Math.Clamp(value, 0f, 1f);
		}
	}

	public float A
	{
		get
		{
			return _a;
		}
		set
		{
			_a = Math.Clamp(value, 0f, 1f);
		}
	}

	public float SPercent
	{
		get
		{
			return _s * 100f;
		}
		set
		{
			S = value / 100f;
		}
	}

	public float LPercent
	{
		get
		{
			return _l * 100f;
		}
		set
		{
			L = value / 100f;
		}
	}

	public float APercent
	{
		get
		{
			return _a * 100f;
		}
		set
		{
			A = value / 100f;
		}
	}

	public DXTHSLA(float h, float s, float l, float a = 1f)
	{
		_h = (h % 360f + 360f) % 360f;
		_s = Math.Clamp(s, 0f, 1f);
		_l = Math.Clamp(l, 0f, 1f);
		_a = Math.Clamp(a, 0f, 1f);
	}

	public static bool operator ==(DXTHSLA left, DXTHSLA right)
	{
		if (Math.Abs(left.H - right.H) < 0.003921569f && Math.Abs(left.S - right.S) < 0.003921569f && Math.Abs(left.L - right.L) < 0.003921569f)
		{
			return Math.Abs(left.A - right.A) < 0.003921569f;
		}
		return false;
	}

	public static bool operator !=(DXTHSLA left, DXTHSLA right)
	{
		return !(left == right);
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is DXTHSLA dXTHSLA)
		{
			return this == dXTHSLA;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(H, S, L, A);
	}

	public static DXTHSLA FromPercent(float h, float sPercent, float lPercent, float aPercent = 100f)
	{
		return new DXTHSLA(h, sPercent / 100f, lPercent / 100f, aPercent / 100f);
	}

	public static DXTHSLA FromRGBA(Color color)
	{
		float num = (float)(int)color.R / 255f;
		float num2 = (float)(int)color.G / 255f;
		float num3 = (float)(int)color.B / 255f;
		float a = (float)(int)color.A / 255f;
		float num4 = Math.Max(num, Math.Max(num2, num3));
		float num5 = Math.Min(num, Math.Min(num2, num3));
		float num6 = num4 - num5;
		float num7 = ((num6 == 0f) ? 0f : ((num4 == num) ? (60f * ((num2 - num3) / num6 % 6f)) : ((num4 != num2) ? (60f * ((num - num2) / num6 + 4f)) : (60f * ((num3 - num) / num6 + 2f)))));
		if (num7 < 0f)
		{
			num7 += 360f;
		}
		float num8 = (num4 + num5) / 2f;
		float s = ((num6 != 0f) ? (num6 / (1f - Math.Abs(2f * num8 - 1f))) : 0f);
		return new DXTHSLA(num7, s, num8, a);
	}

	public readonly Color ToRGBA()
	{
		float num = (1f - Math.Abs(2f * L - 1f)) * S;
		float num2 = H / 60f;
		float num3 = num * (1f - Math.Abs(num2 % 2f - 1f));
		float num4 = L - num / 2f;
		float num5;
		float num6;
		float num7;
		if (num2 >= 0f && num2 < 1f)
		{
			num5 = num;
			num6 = num3;
			num7 = 0f;
		}
		else if (num2 >= 1f && num2 < 2f)
		{
			num5 = num3;
			num6 = num;
			num7 = 0f;
		}
		else if (num2 >= 2f && num2 < 3f)
		{
			num5 = 0f;
			num6 = num;
			num7 = num3;
		}
		else if (num2 >= 3f && num2 < 4f)
		{
			num5 = 0f;
			num6 = num3;
			num7 = num;
		}
		else if (num2 >= 4f && num2 < 5f)
		{
			num5 = num3;
			num6 = 0f;
			num7 = num;
		}
		else if (num2 >= 5f && num2 < 6f)
		{
			num5 = num;
			num6 = 0f;
			num7 = num3;
		}
		else
		{
			num5 = 0f;
			num6 = 0f;
			num7 = 0f;
		}
		byte red = (byte)Math.Round((num5 + num4) * 255f);
		byte green = (byte)Math.Round((num6 + num4) * 255f);
		byte blue = (byte)Math.Round((num7 + num4) * 255f);
		return Color.FromArgb((byte)Math.Round(A * 255f), red, green, blue);
	}

	public readonly uint ToImGui()
	{
		Color color = ToRGBA();
		return (uint)(color.R | (color.G << 8) | (color.B << 16) | (color.A << 24));
	}

	public (float H, float S, float L, float A) ToPercent()
	{
		return (H: H, S: S * 100f, L: L * 100f, A: A * 100f);
	}

	public (int H, int S, int L, int A) ToPercentRounded()
	{
		return (H: (int)MathF.Round(H), S: (int)MathF.Round(S * 100f), L: (int)MathF.Round(L * 100f), A: (int)MathF.Round(A * 100f));
	}
}
