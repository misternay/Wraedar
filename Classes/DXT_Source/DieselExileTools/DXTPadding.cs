using System;
using System.Numerics;

namespace DieselExileTools;

public readonly struct DXTPadding
{
	public readonly int Left;

	public readonly int Top;

	public readonly int Right;

	public readonly int Bottom;

	public int Horizontal => Left + Right;

	public int Vertical => Top + Bottom;

	public DXTPadding(int left, int top, int right, int bottom)
	{
		Left = left;
		Top = top;
		Right = right;
		Bottom = bottom;
	}

	public DXTPadding(int all)
		: this(all, all, all, all)
	{
	}

	public DXTPadding(int horizontal, int vertical)
		: this(horizontal, vertical, horizontal, vertical)
	{
	}

	public bool Equals(DXTPadding other)
	{
		if (Left == other.Left && Top == other.Top && Right == other.Right)
		{
			return Bottom == other.Bottom;
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		if (obj is DXTPadding other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Left, Top, Right, Bottom);
	}

	public static bool operator ==(DXTPadding a, DXTPadding b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(DXTPadding a, DXTPadding b)
	{
		return !a.Equals(b);
	}

	public void Deconstruct(out int left, out int top, out int right, out int bottom)
	{
		left = Left;
		top = Top;
		right = Right;
		bottom = Bottom;
	}

	public Vector4 ToVector4()
	{
		return new Vector4(Left, Top, Right, Bottom);
	}

	public override string ToString()
	{
		return $"Padding(L:{Left}, T:{Top}, R:{Right}, B:{Bottom})";
	}
}
