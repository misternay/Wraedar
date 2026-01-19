using System;
using System.Globalization;
using System.Numerics;

namespace DieselExileTools;

public struct DXTVector2i : IEquatable<DXTVector2i>
{
	public int X;

	public int Y;

	public static DXTVector2i Zero { get; } = new DXTVector2i(0, 0);

	public static DXTVector2i One { get; } = new DXTVector2i(1, 1);

	public DXTVector2i(int x, int y)
	{
		X = x;
		Y = y;
	}

	public int Length()
	{
		return (int)Math.Sqrt(LengthSqr());
	}

	public int LengthSqr()
	{
		return X * X + Y * Y;
	}

	public void Normalize()
	{
		int num = Length();
		Divide(ref this, num, out this);
	}

	public DXTVector2i Normalized()
	{
		DXTVector2i result = this;
		result.Normalize();
		return result;
	}

	public int Distance(DXTVector2i v)
	{
		return Distance(ref this, ref v);
	}

	public float DistanceF(DXTVector2i v)
	{
		return DistanceF(ref this, ref v);
	}

	public int Distance(ref DXTVector2i v)
	{
		return Distance(ref this, ref v);
	}

	public int DistanceSqr(DXTVector2i v)
	{
		return DistanceSqr(ref this, ref v);
	}

	public int DistanceSqr(ref DXTVector2i v)
	{
		return DistanceSqr(ref this, ref v);
	}

	public Vector2 ToVector2()
	{
		return new Vector2(X, Y);
	}

	public bool Equals(DXTVector2i other)
	{
		return Equals(ref this, ref other);
	}

	public bool Equals(ref DXTVector2i other)
	{
		return Equals(ref this, ref other);
	}

	public static bool Equals(ref DXTVector2i v1, ref DXTVector2i v2)
	{
		if (v1.X == v2.X)
		{
			return v1.Y == v2.Y;
		}
		return false;
	}

	public static bool operator ==(DXTVector2i ls, DXTVector2i rs)
	{
		return Equals(ref ls, ref rs);
	}

	public static bool operator !=(DXTVector2i ls, DXTVector2i rs)
	{
		return !Equals(ref ls, ref rs);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		try
		{
			return Equals((DXTVector2i)obj);
		}
		catch (InvalidCastException)
		{
			return false;
		}
	}

	public override int GetHashCode()
	{
		return (X.GetHashCode() * 397) ^ Y.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{{{0}, {1}}}", X, Y);
	}

	public static DXTVector2i operator +(DXTVector2i ls, DXTVector2i rs)
	{
		Add(ref ls, ref rs, out var result);
		return result;
	}

	public static DXTVector2i operator -(DXTVector2i ls, DXTVector2i rs)
	{
		Subtract(ref ls, ref rs, out var result);
		return result;
	}

	public static DXTVector2i operator -(DXTVector2i v)
	{
		v.X = -v.X;
		v.Y = -v.Y;
		return v;
	}

	public static DXTVector2i operator *(DXTVector2i ls, DXTVector2i rs)
	{
		Multiply(ref ls, ref rs, out var result);
		return result;
	}

	public static DXTVector2i operator *(DXTVector2i ls, int rs)
	{
		Multiply(ref ls, rs, out var result);
		return result;
	}

	public static DXTVector2i operator *(DXTVector2i ls, float rs)
	{
		return new DXTVector2i((int)((float)ls.X * rs), (int)((float)ls.Y * rs));
	}

	public static DXTVector2i operator /(DXTVector2i ls, DXTVector2i rs)
	{
		Multiply(ref ls, ref rs, out var result);
		return result;
	}

	public static DXTVector2i operator /(DXTVector2i ls, int rs)
	{
		Divide(ref ls, rs, out var result);
		return result;
	}

	public static implicit operator Vector2(DXTVector2i vector)
	{
		return vector.ToVector2();
	}

	public static void Add(ref DXTVector2i v1, ref DXTVector2i v2, out DXTVector2i result)
	{
		result = new DXTVector2i
		{
			X = v1.X + v2.X,
			Y = v1.Y + v2.Y
		};
	}

	public static void Subtract(ref DXTVector2i v1, ref DXTVector2i v2, out DXTVector2i result)
	{
		result = new DXTVector2i
		{
			X = v1.X - v2.X,
			Y = v1.Y - v2.Y
		};
	}

	public static void Multiply(ref DXTVector2i v1, ref DXTVector2i v2, out DXTVector2i result)
	{
		result = new DXTVector2i
		{
			X = v1.X * v2.X,
			Y = v1.Y * v2.Y
		};
	}

	public static DXTVector2i Multiply(DXTVector2i v1, DXTVector2i v2)
	{
		return new DXTVector2i
		{
			X = v1.X * v2.X,
			Y = v1.Y * v2.Y
		};
	}

	public static void Multiply(ref DXTVector2i v1, float scalar, out DXTVector2i result)
	{
		result = new DXTVector2i
		{
			X = (int)((float)v1.X * scalar),
			Y = (int)((float)v1.Y * scalar)
		};
	}

	public static void Divide(ref DXTVector2i v1, ref DXTVector2i v2, out DXTVector2i result)
	{
		result = new DXTVector2i
		{
			X = v1.X / v2.X,
			Y = v1.Y / v2.Y
		};
	}

	public static void Divide(ref DXTVector2i v1, float divisor, out DXTVector2i result)
	{
		Multiply(ref v1, 1f / divisor, out result);
	}

	public static int Distance(ref DXTVector2i v1, ref DXTVector2i v2)
	{
		return (int)Math.Sqrt(DistanceSqr(ref v1, ref v2));
	}

	public static float DistanceF(ref DXTVector2i v1, ref DXTVector2i v2)
	{
		return (float)Math.Sqrt(DistanceSqr(ref v1, ref v2));
	}

	public static int DistanceSqr(ref DXTVector2i v1, ref DXTVector2i v2)
	{
		int num = v1.X - v2.X;
		int num2 = v1.Y - v2.Y;
		return num * num + num2 * num2;
	}

	public static void GetDirection(ref DXTVector2i from, ref DXTVector2i to, out DXTVector2i dir)
	{
		Subtract(ref to, ref from, out dir);
		dir.Normalize();
	}

	public static DXTVector2i Min(DXTVector2i v1, DXTVector2i v2)
	{
		Min(ref v1, ref v2, out var result);
		return result;
	}

	public static void Min(ref DXTVector2i v1, ref DXTVector2i v2, out DXTVector2i result)
	{
		result = new DXTVector2i(Math.Min(v1.X, v2.X), Math.Min(v1.Y, v2.Y));
	}

	public static DXTVector2i Max(DXTVector2i v1, DXTVector2i v2)
	{
		Max(ref v1, ref v2, out var result);
		return result;
	}

	public static void Max(ref DXTVector2i v1, ref DXTVector2i v2, out DXTVector2i result)
	{
		result = new DXTVector2i
		{
			X = Math.Max(v1.X, v2.X),
			Y = Math.Max(v1.Y, v2.Y)
		};
	}

	public static int Dot(DXTVector2i v1, DXTVector2i v2)
	{
		return v1.X * v2.X + v1.Y * v2.Y;
	}
}
