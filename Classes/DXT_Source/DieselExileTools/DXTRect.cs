using System;
using System.Drawing;
using System.Numerics;

namespace DieselExileTools;

public struct DXTRect
{
	public enum TriangleType
	{
		Up,
		Down,
		Left,
		Right
	}

	public float Left { get; set; }

	public float Top { get; set; }

	public float Right { get; set; }

	public float Bottom { get; set; }

	public float Width
	{
		get
		{
			return Right - Left;
		}
		set
		{
			Right = Left + value;
		}
	}

	public float Height
	{
		get
		{
			return Bottom - Top;
		}
		set
		{
			Bottom = Top + value;
		}
	}

	public Vector2 TopLeft
	{
		get
		{
			return new Vector2(Left, Top);
		}
		set
		{
			Left = value.X;
			Top = value.Y;
		}
	}

	public Vector2 TopRight
	{
		get
		{
			return new Vector2(Right, Top);
		}
		set
		{
			Right = value.X;
			Top = value.Y;
		}
	}

	public Vector2 BottomLeft
	{
		get
		{
			return new Vector2(Left, Bottom);
		}
		set
		{
			Left = value.X;
			Bottom = value.Y;
		}
	}

	public Vector2 BottomRight
	{
		get
		{
			return new Vector2(Right, Bottom);
		}
		set
		{
			Right = value.X;
			Bottom = value.Y;
		}
	}

	public bool IsInvalid
	{
		get
		{
			if (!(Width <= 0f))
			{
				return Height <= 0f;
			}
			return true;
		}
	}

	public Vector2 Size => new Vector2(Width, Height);

	public Vector2 Center => new Vector2((Left + Right) * 0.5f, (Top + Bottom) * 0.5f);

	public Vector2 CenterRounded => new Vector2(MathF.Round(Center.X), MathF.Round(Center.Y));

	public DXTRect(float left, float top, float right, float bottom)
	{
		Left = left;
		Top = top;
		Right = right;
		Bottom = bottom;
	}

	public DXTRect(int left, int top, int right, int bottom)
	{
		Left = left;
		Top = top;
		Right = right;
		Bottom = bottom;
	}

	public DXTRect(Vector2 topLeft, Vector2 bottomRight)
	{
		Left = topLeft.X;
		Top = topLeft.Y;
		Right = bottomRight.X;
		Bottom = bottomRight.Y;
	}

	public DXTRect(Vector2 topLeft, float width, float height)
	{
		Left = topLeft.X;
		Top = topLeft.Y;
		Right = topLeft.X + width;
		Bottom = topLeft.Y + height;
	}

	public DXTRect(Vector2 topLeft, int width, int height)
	{
		Left = topLeft.X;
		Top = topLeft.Y;
		Right = topLeft.X + (float)width;
		Bottom = topLeft.Y + (float)height;
	}

	public static DXTRect FromTopRight(Vector2 topRight, float width, float height)
	{
		return new DXTRect(topRight.X - width, topRight.Y, topRight.X, topRight.Y + height);
	}

	public static DXTRect FromBottomLeft(Vector2 bottomLeft, float width, float height)
	{
		return new DXTRect(bottomLeft.X, bottomLeft.Y - height, bottomLeft.X + width, bottomLeft.Y);
	}

	public static DXTRect FromBottomRight(Vector2 bottomRight, float width, float height)
	{
		return new DXTRect(bottomRight.X - width, bottomRight.Y - height, bottomRight.X, bottomRight.Y);
	}

	public override string ToString()
	{
		return $"Rect(L:{Left}, T:{Top}, R:{Right}, B:{Bottom}, W:{Width}, H:{Height})";
	}

	public string ToFormattedString(string format = "F1")
	{
		return $"Rect(L:{Left.ToString(format)}, T:{Top.ToString(format)}, R:{Right.ToString(format)}, B:{Bottom.ToString(format)}, W:{Width.ToString(format)}, H:{Height.ToString(format)})";
	}

	public readonly DXTRect Shrink(float amount)
	{
		return new DXTRect(Left + amount, Top + amount, Right - amount, Bottom - amount);
	}

	public readonly DXTRect Shrink(float left, float top, float right, float bottom)
	{
		return new DXTRect(Left + left, Top + top, Right - right, Bottom - bottom);
	}

	public readonly DXTRect Shrink(DXTPadding padding)
	{
		return new DXTRect(Left + (float)padding.Left, Top + (float)padding.Top, Right - (float)padding.Right, Bottom - (float)padding.Bottom);
	}

	public readonly DXTRect Expand(float amount)
	{
		return new DXTRect(Left - amount, Top - amount, Right + amount, Bottom + amount);
	}

	public readonly DXTRect Expand(float left, float top, float right, float bottom)
	{
		return new DXTRect(Left - left, Top - top, Right + right, Bottom + bottom);
	}

	public readonly DXTRect Expand(DXTPadding padding)
	{
		return new DXTRect(Left - (float)padding.Left, Top - (float)padding.Top, Right + (float)padding.Right, Bottom + (float)padding.Bottom);
	}

	public readonly DXTRect Pad(float left, float top, float right, float bottom)
	{
		return new DXTRect(Left + left, Top + top, Right - right, Bottom - bottom);
	}

	public readonly DXTRect Move(Vector2 delta)
	{
		return new DXTRect(Left + delta.X, Top + delta.Y, Right + delta.X, Bottom + delta.Y);
	}

	public (Vector2, Vector2, Vector2) ToTriangle(TriangleType type)
	{
		Vector2 topLeft = TopLeft;
		Vector2 topRight = TopRight;
		Vector2 bottomLeft = BottomLeft;
		Vector2 bottomRight = BottomRight;
		Vector2 item = new Vector2((Left + Right) / 2f, Top);
		Vector2 item2 = new Vector2((Left + Right) / 2f, Bottom);
		Vector2 item3 = new Vector2(Left, (Top + Bottom) / 2f);
		Vector2 item4 = new Vector2(Right, (Top + Bottom) / 2f);
		return type switch
		{
			TriangleType.Up => (topLeft, topRight, item2), 
			TriangleType.Down => (bottomLeft, bottomRight, item), 
			TriangleType.Left => (topLeft, bottomLeft, item4), 
			TriangleType.Right => (topRight, bottomRight, item3), 
			_ => (topLeft, topRight, bottomLeft), 
		};
	}

	public readonly bool Contains(Vector2 point)
	{
		if (point.X >= Left && point.X <= Right && point.Y >= Top)
		{
			return point.Y <= Bottom;
		}
		return false;
	}

	public readonly bool Contains(Point point)
	{
		if ((float)point.X >= Left && (float)point.X <= Right && (float)point.Y >= Top)
		{
			return (float)point.Y <= Bottom;
		}
		return false;
	}

	public readonly bool Contains(Rectangle rect)
	{
		if ((float)rect.Left >= Left && (float)rect.Right <= Right && (float)rect.Top >= Top)
		{
			return (float)rect.Bottom <= Bottom;
		}
		return false;
	}

	public readonly bool Contains(RectangleF rect)
	{
		if (rect.Left >= Left && rect.Right <= Right && rect.Top >= Top)
		{
			return rect.Bottom <= Bottom;
		}
		return false;
	}

	public readonly bool Intersects(DXTRect other)
	{
		if (Left < other.Right && Right > other.Left && Top < other.Bottom)
		{
			return Bottom > other.Top;
		}
		return false;
	}
}
