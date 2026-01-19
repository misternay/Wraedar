using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ClickableTransparentOverlay;
using GameHelper;
using GameHelper.RemoteObjects.UiElement;
using ImGuiNET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Point = System.Drawing.Point;
using RectangleF = System.Drawing.RectangleF;

namespace DieselExileTools;

public static class DXT
{
	public class IconAtlas
	{
		public string Name { get; }

		public string FilePath { get; }

		public nint TextureId { get; }

		public Vector2 IconSize { get; }

		public Vector2 AtlasSize { get; }

		public int IconsPerRow { get; }

		public int IconsPerColumn { get; }

		public int TotalIcons { get; }

		public IconAtlas(string name, string filePath, Vector2 iconSize)
		{
			if (File.Exists(filePath))
			{
				Image<Rgba32> val = Image.Load<Rgba32>(filePath);
				try
				{
					AtlasSize = new Vector2(((Image)val).Width, ((Image)val).Height);
					nint textureId = default(nint);
                    uint w = 0, h = 0;
					// Use file-based API instead of Image<Rgba32> overload
					Core.Overlay.AddOrGetImagePointer(filePath, false, out textureId, out w, out h);
					TextureId = textureId;
					Log($"Initialised IconAtlas: {name}, Size: {AtlasSize}, Dir: {filePath}", whenVisibleOnly: false);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
				Name = name;
				FilePath = filePath;
				IconSize = iconSize;
				IconsPerRow = (int)(AtlasSize.X / iconSize.X);
				IconsPerColumn = (int)(AtlasSize.Y / IconSize.Y);
				TotalIcons = IconsPerRow * IconsPerColumn;
				return;
			}
			throw new FileNotFoundException("IconAtlas file not found: " + filePath);
		}

		public RectangleF GetIconUV(int iconIndex)
		{
			float x = (float)(iconIndex % IconsPerRow) * IconSize.X / AtlasSize.X;
			float y = (float)(iconIndex / IconsPerRow) * IconSize.Y / AtlasSize.Y;
			return new RectangleF(x, y, IconSize.X / AtlasSize.X, IconSize.Y / AtlasSize.Y);
		}

		public DXTRect GetIconUVRect(int iconIndex)
		{
			return new DXTRect(new Vector2((float)(iconIndex % IconsPerRow) * IconSize.X / AtlasSize.X, (float)(iconIndex / IconsPerRow) * IconSize.Y / AtlasSize.Y), IconSize.X / AtlasSize.X, IconSize.Y / AtlasSize.Y);
		}

		public (Vector2 uv0, Vector2 uv1) GetIconUVs(int iconIndex)
		{
			int num = iconIndex % IconsPerRow;
			int num2 = iconIndex / IconsPerRow;
			float x = (float)num * IconSize.X / AtlasSize.X;
			float y = (float)num2 * IconSize.Y / AtlasSize.Y;
			float x2 = (float)(num + 1) * IconSize.X / AtlasSize.X;
			float y2 = (float)(num2 + 1) * IconSize.Y / AtlasSize.Y;
			Vector2 item = new Vector2(x, y);
			Vector2 item2 = new Vector2(x2, y2);
			return (uv0: item, uv1: item2);
		}
	}

	public static class Colors
	{
		public static readonly System.Drawing.Color WindowBackground = System.Drawing.Color.FromArgb(0, 0, 0);

		public static readonly System.Drawing.Color Border = System.Drawing.Color.FromArgb(0, 0, 0);

		public static readonly System.Drawing.Color Hover = Color.FromRGBA(255, 255, 255, 25);

		public static readonly System.Drawing.Color Panel = Color.FromHsla(220f, 0.15f, 0.14f);

		public static readonly System.Drawing.Color PanelInnerGlow = Color.FromRGBA(255, 255, 255, 5);

		public static readonly System.Drawing.Color PanelHeader = Color.FromHsla(220f, 0.15f, 0.24f);

		public static readonly System.Drawing.Color ControlRed = Color.FromHSLA(0f, 64f, 26f);

		public static readonly System.Drawing.Color ControlOrange = Color.FromHSLA(15f, 79f, 36f);

		public static readonly System.Drawing.Color ControlAmber = Color.FromHSLA(26f, 91f, 36f);

		public static readonly System.Drawing.Color ControlYellow = Color.FromHSLA(41f, 97f, 39f);

		public static readonly System.Drawing.Color ControlLime = Color.FromHSLA(86f, 78f, 27f);

		public static readonly System.Drawing.Color ControlGreen = Color.FromHSLA(142f, 72f, 29f);

		public static readonly System.Drawing.Color ControlEmerald = Color.FromHSLA(163f, 94f, 24f);

		public static readonly System.Drawing.Color ControlTeal = Color.FromHSLA(175f, 86f, 28f);

		public static readonly System.Drawing.Color ControlCyan = Color.FromHSLA(191f, 92f, 35f);

		public static readonly System.Drawing.Color ControlSky = Color.FromHSLA(205f, 99f, 45f);

		public static readonly System.Drawing.Color ControlBlue = Color.FromHSLA(221f, 80f, 43f);

		public static readonly System.Drawing.Color ControlIndigo = Color.FromHSLA(245f, 58f, 50f);

		public static readonly System.Drawing.Color ControlViolet = Color.FromHSLA(263f, 70f, 48f);

		public static readonly System.Drawing.Color ControlPurple = Color.FromHSLA(273f, 67f, 39f);

		public static readonly System.Drawing.Color ControlFuchsia = Color.FromHSLA(295f, 72f, 40f);

		public static readonly System.Drawing.Color ControlPink = Color.FromHSLA(335f, 78f, 42f);

		public static readonly System.Drawing.Color ControlRose = Color.FromHSLA(343f, 80f, 35f);

		public static readonly System.Drawing.Color ControlSlate = Color.FromHSLA(215f, 20f, 34f);

		public static readonly System.Drawing.Color ControlGray = Color.FromHSLA(215f, 14f, 34f);

		public static readonly System.Drawing.Color ControlZinc = Color.FromHSLA(240f, 5f, 34f);

		public static readonly System.Drawing.Color ControlNeutral = Color.FromHSLA(0f, 0f, 32f);

		public static readonly System.Drawing.Color ControlStone = Color.FromHSLA(30f, 6f, 25f);

		public static readonly System.Drawing.Color ControlChecked = Color.FromHSLA(205f, 91f, 44f);

		public static readonly System.Drawing.Color TextRed = Color.FromHSLA(0f, 72f, 51f);

		public static readonly System.Drawing.Color TextOrange = Color.FromHSLA(25f, 95f, 53f);

		public static readonly System.Drawing.Color TextAmber = Color.FromHSLA(38f, 92f, 50f);

		public static readonly System.Drawing.Color TextYellow = Color.FromHSLA(50f, 98f, 64f);

		public static readonly System.Drawing.Color TextLime = Color.FromHSLA(83f, 78f, 55f);

		public static readonly System.Drawing.Color TextGreen = Color.FromHSLA(142f, 71f, 45f);

		public static readonly System.Drawing.Color TextEmerald = Color.FromHSLA(158f, 64f, 52f);

		public static readonly System.Drawing.Color TextTeal = Color.FromHSLA(172f, 66f, 50f);

		public static readonly System.Drawing.Color TextCyan = Color.FromHSLA(188f, 86f, 53f);

		public static readonly System.Drawing.Color TextSky = Color.FromHSLA(198f, 93f, 60f);

		public static readonly System.Drawing.Color TextBlue = Color.FromHSLA(217f, 91f, 64f);

		public static readonly System.Drawing.Color TextIndigo = Color.FromHSLA(234f, 89f, 74f);

		public static readonly System.Drawing.Color TextViolet = Color.FromHSLA(255f, 92f, 76f);

		public static readonly System.Drawing.Color TextPurple = Color.FromHSLA(270f, 95f, 75f);

		public static readonly System.Drawing.Color TextFuchsia = Color.FromHSLA(292f, 91f, 73f);

		public static readonly System.Drawing.Color TextPink = Color.FromHSLA(329f, 86f, 70f);

		public static readonly System.Drawing.Color TextRose = Color.FromHSLA(351f, 94f, 68f);

		public static readonly System.Drawing.Color TextSlate = Color.FromHSLA(215f, 20f, 65f);

		public static readonly System.Drawing.Color TextStone = Color.FromHSLA(24f, 5f, 64f);

		public static readonly System.Drawing.Color Text = Color.FromRGBA(255, 255, 255, 193);

		public static readonly System.Drawing.Color TextChecked = Color.FromHSLA(205f, 95f, 67f);

		public static readonly System.Drawing.Color TextDisabled = Color.FromHsla(220f, 0.15f, 0.55f);

		public static readonly System.Drawing.Color TextOnColor = Color.FromRGBA(255, 255, 255, 240);

		public static readonly System.Drawing.Color TextonControl = Text;

		public static readonly System.Drawing.Color TitleText = TextOnColor;

		public static readonly System.Drawing.Color Menu = Color.FromHsla(220f, 0.15f, 0.18f);

		public static readonly System.Drawing.Color Input = Color.FromHsla(220f, 0.15f, 0.08f);

		public static readonly System.Drawing.Color InputInnerGlow = Color.FromRGBA(255, 255, 255, 5);

		public static readonly System.Drawing.Color InputText = Color.FromHsla(220f, 0.15f, 0.65f);

		public static readonly System.Drawing.Color InputHovered = Color.FromRGBA(255, 255, 255, 8);

		public static readonly System.Drawing.Color Button = Color.FromHsla(220f, 0.15f, 0.28f);

		public static readonly System.Drawing.Color ButtonClose = Color.FromHsla(0f, 0.9f, 0.15f);

		public static readonly System.Drawing.Color ButtonInnerGlow = Color.FromRGBA(255, 255, 255, 12);

		public static readonly System.Drawing.Color ButtonChecked = ControlChecked;

		public static readonly System.Drawing.Color ButtonHovered = Hover;

		public static readonly System.Drawing.Color ButtonText = Text;

		public static readonly System.Drawing.Color ButtonTextDisabled = TextDisabled;

		public static readonly System.Drawing.Color ButtonTextChecked = TextOnColor;

		public static readonly System.Drawing.Color SwatchInnerGlow = Color.FromRGBA(255, 255, 255, 15);
	}

	public class Config
	{
		public required string PluginName { get; set; }

		public required string PluginDirectory { get; set; }

		public required DXTSettings Settings { get; set; }
	}

	public static class Color
	{
		public static System.Drawing.Color FromHsla(float h, float s, float l, float a = 1f)
		{
			h = (h % 360f + 360f) % 360f;
			s = Math.Clamp(s, 0f, 1f);
			l = Math.Clamp(l, 0f, 1f);
			a = Math.Clamp(a, 0f, 1f);
			float num = (1f - Math.Abs(2f * l - 1f)) * s;
			float num2 = num * (1f - Math.Abs(h / 60f % 2f - 1f));
			float num3 = l - num / 2f;
			float num4 = 0f;
			float num5 = 0f;
			float num6 = 0f;
			if (h < 60f)
			{
				num4 = num;
				num5 = num2;
			}
			else if (h < 120f)
			{
				num4 = num2;
				num5 = num;
			}
			else if (h < 180f)
			{
				num5 = num;
				num6 = num2;
			}
			else if (h < 240f)
			{
				num5 = num2;
				num6 = num;
			}
			else if (h < 300f)
			{
				num4 = num2;
				num6 = num;
			}
			else
			{
				num4 = num;
				num6 = num2;
			}
			return System.Drawing.Color.FromArgb((int)(a * 255f), (int)((num4 + num3) * 255f), (int)((num5 + num3) * 255f), (int)((num6 + num3) * 255f));
		}

		public static System.Drawing.Color FromHSLA(float h, float s, float l, float a = 100f)
		{
			s = Math.Clamp(s / 100f, 0f, 1f);
			l = Math.Clamp(l / 100f, 0f, 1f);
			a = Math.Clamp(a / 100f, 0f, 1f);
			return FromHsla(h, s, l, a);
		}

		public static System.Drawing.Color FromHEX(string hex)
		{
			hex = hex.Replace("#", "");
			if (hex.Length == 6)
			{
				hex += "FF";
			}
			byte red = Convert.ToByte(hex.Substring(0, 2), 16);
			byte green = Convert.ToByte(hex.Substring(2, 2), 16);
			byte blue = Convert.ToByte(hex.Substring(4, 2), 16);
			return System.Drawing.Color.FromArgb(Convert.ToByte(hex.Substring(6, 2), 16), red, green, blue);
		}

		public static System.Drawing.Color FromRGBA(int r, int g, int b, int a = 255)
		{
			return System.Drawing.Color.FromArgb(a, r, g, b);
		}
	}

	public static class CollapsingPanel
	{
		public class Options
		{
			public required string Label { get; set; }

			public int PadLeft { get; set; } = 3;

			public int PadTop { get; set; } = 3;

			public int HeaderHeight { get; set; } = 20;

			public int? HeaderWidth { get; set; }

			public int HeaderSpacing { get; set; } = 1;

			public int? Width { get; set; } = 0;

			public int? Height { get; set; }

			public System.Drawing.Color Color { get; set; } = Colors.Panel;

			public System.Drawing.Color HeaderColor { get; set; } = Colors.PanelHeader;

			public System.Drawing.Color HeaderTextColor { get; set; } = Colors.ButtonText;

			public System.Drawing.Color InnerGlowColor { get; set; } = Colors.PanelInnerGlow;

			public bool Debug { get; set; }

			public Vector2 CalculatedSize { get; set; }
		}

		public static bool Begin(string uniqueID, ref bool collapsed, Options options)
		{
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(uniqueID))
			{
				throw new ArgumentException("uniqueID cannot be null or empty", "uniqueID");
			}
			Panel.Options options2 = new Panel.Options
			{
				PadLeft = options.PadLeft,
				PadTop = options.PadTop,
				Width = options.Width,
				Height = options.Height,
				Debug = options.Debug,
				Color = options.Color,
				InnerGlowColor = options.InnerGlowColor
			};
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
			float x = ImGui.GetContentRegionAvail().X;
			Vector2 vector = cursorScreenPos;
			float x2 = ImGui.CalcTextSize(options.Label).X + 8f;
			if (options.HeaderWidth.HasValue && options.HeaderWidth > 0)
			{
				x2 = options.HeaderWidth.Value;
			}
			else if (options.Width.HasValue)
			{
				x2 = ((!(options.Width <= 0)) ? ((float)options.Width.Value) : (x + (float)options.Width.Value));
			}
			int headerHeight = options.HeaderHeight;
			windowDrawList.AddRectFilled(vector, vector + new Vector2(x2, headerHeight), options.HeaderColor.ToImGui());
			windowDrawList.AddRect(vector, vector + new Vector2(x2, headerHeight), options.InnerGlowColor.ToImGui());
			if (!string.IsNullOrEmpty(options.Label))
			{
				Vector2 vector2 = ImGui.CalcTextSize(options.Label);
				ImFontPtr font = ImGui.GetFont();
				int num = ((font.GetDebugName() == "unifont.otf, 16px") ? 2 : 0);
				Vector2 vector3 = vector + new Vector2(4f, (float)Math.Ceiling(((float)headerHeight - vector2.Y) / 2f) - (float)num);
				windowDrawList.AddText(vector3, options.HeaderTextColor.ToImGui(), options.Label);
			}
			ImGui.SetCursorScreenPos(vector);
			if (ImGui.InvisibleButton(uniqueID + "_headerbutton", new Vector2(x2, headerHeight)))
			{
				collapsed = !collapsed;
			}
			if (collapsed)
			{
				return false;
			}
			Vector2 cursorScreenPos2 = ImGui.GetCursorScreenPos();
			cursorScreenPos2.Y += options.HeaderSpacing;
			ImGui.SetCursorScreenPos(cursorScreenPos2);
			Panel.Begin(uniqueID, options2);
			ImGui.TableSetColumnIndex(1);
			ImGui.SetCursorPosY(ImGui.GetCursorPosY());
			return true;
		}

		public static void End(string uniqueID)
		{
			Panel.End(uniqueID);
		}
	}

	public static class Panel
	{
		public class Options
		{
			public int PadLeft { get; set; } = 3;

			public int PadTop { get; set; } = 3;

			public int? Width { get; set; } = 0;

			public int? Height { get; set; }

			public System.Drawing.Color Color { get; set; } = Colors.Panel;

			public System.Drawing.Color InnerGlowColor { get; set; } = Colors.PanelInnerGlow;

			public bool Debug { get; set; }
		}

		private static readonly Dictionary<string, Options> panelOptions = new Dictionary<string, Options>();

		public static void Begin(string uniqueID, Options options)
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(uniqueID))
			{
				throw new ArgumentException("uniqueID cannot be null or empty", "uniqueID");
			}
			if (options == null)
			{
				throw new ArgumentNullException("options", "Options cannot be null");
			}
			panelOptions[uniqueID] = options;
			Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
			ImGuiChildFlags val = (ImGuiChildFlags)0;
			Vector2 vector = new Vector2(0f, 0f);
			if (options.Width.HasValue)
			{
				if (options.Width.Value <= 0)
				{
					vector.X = contentRegionAvail.X + (float)options.Width.Value;
				}
				else
				{
					vector.X = options.Width.Value;
				}
			}
			else
			{
				val = val | (ImGuiChildFlags)0x10;
			}
			if (options.Height.HasValue)
			{
				if (options.Height.Value <= 0)
				{
					vector.Y = contentRegionAvail.Y + (float)options.Height.Value;
				}
				else
				{
					vector.Y = options.Height.Value;
				}
			}
			else
			{
				val = val | (ImGuiChildFlags)0x20;
			}
			ImGui.PushStyleColor((ImGuiCol)3, 0u);
			ImGui.BeginChild(uniqueID, vector, val, (ImGuiWindowFlags)24);
			Vector2 windowPos = ImGui.GetWindowPos();
			Vector2 windowSize = ImGui.GetWindowSize();
			Vector2 vector2 = windowPos;
			Vector2 vector3 = new Vector2(windowPos.X + windowSize.X, windowPos.Y + windowSize.Y);
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			windowDrawList.AddRectFilled(vector2, vector3, options.Color.ToImGui());
			windowDrawList.AddRect(vector2, vector3, options.InnerGlowColor.ToImGui());
			if (options.PadTop > 0)
			{
				ImGui.Dummy(new Vector2(0f, options.PadTop));
			}
			if (options.PadLeft > 0)
			{
				ImGui.Indent((float)options.PadLeft);
			}
		}

		public static void End(string uniqueID)
		{
			Options options = (panelOptions.ContainsKey(uniqueID) ? panelOptions[uniqueID] : null) ?? throw new ArgumentException("No panel with uniqueID:'" + uniqueID + "' open, did you forget to call Panel.Begin()?", "uniqueID");
			if (options.PadLeft > 0)
			{
				ImGui.Unindent((float)options.PadLeft);
			}
			ImGui.EndChild();
			ImGui.PopStyleColor();
		}
	}

	public static class PopupModalWindow
	{
		public class Options
		{
			public string? Title;

			public int Width { get; set; } = 300;

			public int Height { get; set; } = 100;

			public int TitleBarHeight { get; set; } = 18;

			public DXTPadding PanelPadding { get; set; } = new DXTPadding(3, 0, 3, 3);

			public System.Drawing.Color BackgroundColor { get; set; } = System.Drawing.Color.Black;

			public System.Drawing.Color PanelColor { get; set; } = Colors.Panel;

			public System.Drawing.Color PanelBorderColor { get; set; } = Colors.PanelInnerGlow;

			public System.Drawing.Color TitleTextColor { get; set; } = Colors.TextonControl;
		}

		private static void PushStrippedStyles()
		{
			ImGui.PushStyleVar((ImGuiStyleVar)2, new Vector2(0f, 0f));
			ImGui.PushStyleVar((ImGuiStyleVar)11, new Vector2(0f, 0f));
			ImGui.PushStyleColor((ImGuiCol)4, new Vector4(0f, 0f, 0f, 0f));
			ImGui.PushStyleColor((ImGuiCol)5, new Vector4(0f, 0f, 0f, 0f));
			ImGui.PushStyleColor((ImGuiCol)0, Colors.Text.ToImGui());
		}

		private static void PopStrippedStyles()
		{
			ImGui.PopStyleColor(3);
			ImGui.PopStyleVar(2);
		}

		public static void Open(string unique_id)
		{
			ImGui.OpenPopup(unique_id);
		}

		public static bool Begin(string unique_id, Options? options = null)
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(unique_id))
			{
				throw new ArgumentException("unique_id cannot be null or empty", "unique_id");
			}
			if (options == null)
			{
				options = new Options();
			}
			if (!ImGui.IsPopupOpen(unique_id))
			{
				return false;
			}
			PushStrippedStyles();
			ImGui.SetNextWindowSize(new Vector2(options.Width, options.Height), (ImGuiCond)1);
			bool open = true;
            if (!ImGui.BeginPopupModal(unique_id, ref open, (ImGuiWindowFlags)65))
			{
				PopStrippedStyles();
				return false;
			}
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			DXTRect dXTRect = new DXTRect(ImGui.GetWindowPos(), options.Width, options.Height);
			DXTRect dXTRect2 = new DXTRect(new Vector2(dXTRect.Left + (float)options.PanelPadding.Left, dXTRect.Top + (float)options.TitleBarHeight + (float)options.PanelPadding.Top), options.Width - options.PanelPadding.Right - options.PanelPadding.Left, options.Height - options.TitleBarHeight - options.PanelPadding.Top - options.PanelPadding.Bottom);
			windowDrawList.AddRectFilled(dXTRect.TopLeft, dXTRect.BottomRight, options.BackgroundColor.ToImGui(), 0f);
			windowDrawList.AddRectFilled(dXTRect2.TopLeft, dXTRect2.BottomRight, options.PanelColor.ToImGui(), 0f);
			windowDrawList.AddRect(dXTRect2.TopLeft, dXTRect2.BottomRight, options.PanelBorderColor.ToImGui(), 0f, (ImDrawFlags)0, 1f);
			if (!string.IsNullOrEmpty(options.Title))
			{
				windowDrawList.AddText(dXTRect.TopLeft + new Vector2(2f, 0f), options.TitleTextColor.ToImGui(), options.Title);
			}
			ImGui.SetCursorScreenPos(dXTRect2.TopLeft);
			return true;
		}

		public static void End()
		{
			ImGui.EndPopup();
			PopStrippedStyles();
		}
	}

	public static class PopupWindow
	{
		public class Options
		{
			public string? Title;

			public Vector2 Offset { get; set; } = new Vector2(10f, 10f);

			public Vector2 Size { get; set; } = new Vector2(200f, 100f);

			public int TitleBarHeight { get; set; } = 18;

			public DXTPadding PanelPadding { get; set; } = new DXTPadding(3, 0, 3, 3);

			public System.Drawing.Color BackgroundColor { get; set; } = System.Drawing.Color.Black;

			public System.Drawing.Color PanelColor { get; set; } = Colors.Panel;

			public System.Drawing.Color PanelBorderColor { get; set; } = Colors.PanelInnerGlow;

			public System.Drawing.Color TitleTextColor { get; set; } = Colors.TextonControl;
		}

		private static void PushStrippedStyles()
		{
			ImGui.PushStyleVar((ImGuiStyleVar)2, new Vector2(0f, 0f));
			ImGui.PushStyleVar((ImGuiStyleVar)11, new Vector2(0f, 0f));
			ImGui.PushStyleColor((ImGuiCol)4, new Vector4(0f, 0f, 0f, 0f));
			ImGui.PushStyleColor((ImGuiCol)5, new Vector4(0f, 0f, 0f, 0f));
			ImGui.PushStyleColor((ImGuiCol)0, Colors.Text.ToImGui());
		}

		private static void PopStrippedStyles()
		{
			ImGui.PopStyleColor(3);
			ImGui.PopStyleVar(2);
		}

		public static void OpenOnMouse(string unique_id, Vector2 offset)
		{
			ImGui.SetNextWindowPos(ImGui.GetMousePos() + offset, (ImGuiCond)1);
			ImGui.OpenPopup("##" + unique_id + "POPUP");
		}

		public static void Open(string unique_id, Vector2 position)
		{
			ImGui.SetNextWindowPos(position);
			ImGui.OpenPopup("##" + unique_id + "POPUP");
		}

		public static bool Begin(string unique_id, Options options)
		{
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(unique_id))
			{
				throw new ArgumentException("unique_id cannot be null or empty", "unique_id");
			}
			if (options == null)
			{
				throw new ArgumentNullException("options", "Options cannot be null");
			}
			if (!ImGui.IsPopupOpen("##" + unique_id + "POPUP"))
			{
				return false;
			}
			PushStrippedStyles();
			ImGui.SetNextWindowSize(new Vector2(options.Size.X + 5f, options.Size.Y + 5f), (ImGuiCond)1);
			if (!ImGui.BeginPopup("##" + unique_id + "POPUP"))
			{
				PopStrippedStyles();
				return false;
			}
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			Vector2 vector = ImGui.GetWindowPos() + new Vector2(1f, 1f);
			Vector2 vector2 = vector + new Vector2(options.PanelPadding.Left, options.TitleBarHeight + options.PanelPadding.Top);
			Vector2 vector3 = new Vector2(options.Size.X - (float)options.PanelPadding.Right - (float)options.PanelPadding.Left, options.Size.Y - (float)options.TitleBarHeight - (float)options.PanelPadding.Top - (float)options.PanelPadding.Bottom);
			windowDrawList.AddRectFilled(vector, vector + options.Size, options.BackgroundColor.ToImGui(), 0f);
			windowDrawList.AddRectFilled(vector2, vector2 + vector3, options.PanelColor.ToImGui(), 0f);
			windowDrawList.AddRect(vector2, vector2 + vector3, options.PanelBorderColor.ToImGui(), 0f, (ImDrawFlags)0, 1f);
			if (!string.IsNullOrEmpty(options.Title))
			{
				windowDrawList.AddText(vector + new Vector2(2f, 0f), options.TitleTextColor.ToImGui(), options.Title);
			}
			ImGui.SetCursorScreenPos(vector2);
			return true;
		}

		public static void End()
		{
			ImGui.EndPopup();
			PopStrippedStyles();
		}
	}

	public static class Window
	{
		public class Options
		{
			public string? Title;

			public int TitleBarHeight { get; set; } = 20;

			public bool ShowCloseButton { get; set; } = true;

			public bool ShowMinimizeButton { get; set; } = true;

			public System.Drawing.Color BorderColor { get; set; } = Colors.Border;

			public System.Drawing.Color BackgroundColor { get; set; } = System.Drawing.Color.Black;

			public System.Drawing.Color GripColor { get; set; } = Colors.Button;

			public System.Drawing.Color ScrollbarColor { get; set; } = System.Drawing.Color.FromArgb(0, 0, 0, 20);

			public System.Drawing.Color ScrollbarGripColor { get; set; } = Colors.Button;

			public System.Drawing.Color TitleTextColor { get; set; } = Colors.TextonControl;

			public System.Drawing.Color CloseButtonColor { get; set; } = System.Drawing.Color.FromArgb(70, 12, 12);

			public System.Drawing.Color CloseButtonHoverColor { get; set; } = System.Drawing.Color.FromArgb(130, 23, 23);

			public System.Drawing.Color MinimizeButtonColor { get; set; } = System.Drawing.Color.FromArgb(69, 32, 13);

			public System.Drawing.Color MinimizeButtonMinimisedColor { get; set; } = System.Drawing.Color.FromArgb(173, 81, 31);

			public System.Drawing.Color MinimizeButtonHoverColor { get; set; } = System.Drawing.Color.FromArgb(173, 81, 31);

			public bool Movable { get; set; } = true;

			public bool Resizable { get; set; }

			public Vector2? ResetPosition { get; set; }

			public Vector2? ResetSize { get; set; }

			public float? LockWidth { get; set; }

			public float? LockHeight { get; set; }

			public float MinWidth { get; set; } = 100f;

			public float MaxWidth { get; set; } = 5000f;

			public float MinHeight { get; set; } = 100f;

			public float MaxHeight { get; set; } = 5000f;
		}

		private class DrawResult
		{
			public bool DrawContent;

			public bool IsClosed;
		}

		private static readonly Dictionary<string, bool> _minimizedStates = new Dictionary<string, bool>();

		private static void PushStrippedStyles(Options options)
		{
			ImGui.PushStyleVar((ImGuiStyleVar)2, Vector2.Zero);
			ImGui.PushStyleVar((ImGuiStyleVar)11, Vector2.Zero);
			ImGui.PushStyleVar((ImGuiStyleVar)4, 0f);
			ImGui.PushStyleVar((ImGuiStyleVar)3, 0f);
			ImGui.PushStyleVar((ImGuiStyleVar)14, Vector2.Zero);
			ImGui.PushStyleVar((ImGuiStyleVar)15, Vector2.Zero);
			ImGui.PushStyleVar((ImGuiStyleVar)19, 0f);
			ImGui.PushStyleVar((ImGuiStyleVar)18, 10f);
			ImGui.PushStyleColor((ImGuiCol)14, options.ScrollbarColor.ToImGui());
			ImGui.PushStyleColor((ImGuiCol)15, options.ScrollbarGripColor.ToImGui());
			ImGui.PushStyleColor((ImGuiCol)2, System.Drawing.Color.Transparent.ToImGui());
			ImGui.PushStyleColor((ImGuiCol)7, System.Drawing.Color.Transparent.ToImGui());
			ImGui.PushStyleColor((ImGuiCol)5, System.Drawing.Color.Transparent.ToImGui());
			ImGui.PushStyleColor((ImGuiCol)0, Colors.Text.ToImGui());
		}

		private static void PopStrippedStyles()
		{
			ImGui.PopStyleVar(8);
			ImGui.PopStyleColor(6);
		}

		public static bool Begin(string uniqueID, Options options)
		{
			return InternalBegin(uniqueID, options).DrawContent;
		}

		public static bool Begin(string uniqueID, ref bool isOpen, Options options)
		{
			if (!isOpen)
			{
				return false;
			}
			DrawResult drawResult = InternalBegin(uniqueID, options);
			if (drawResult.IsClosed)
			{
				isOpen = false;
			}
			return drawResult.DrawContent;
		}

		private static DrawResult InternalBegin(string uniqueID, Options options)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(uniqueID))
			{
				throw new ArgumentException("uniqueID cannot be null or empty", "uniqueID");
			}
			if (options == null)
			{
				throw new ArgumentNullException("options", "Options cannot be null");
			}
			PushStrippedStyles(options);
			if (!_minimizedStates.TryGetValue(uniqueID, out var value))
			{
				value = false;
				_minimizedStates[uniqueID] = false;
			}
			ImGuiWindowFlags val = (ImGuiWindowFlags)524297;
			if (!options.Movable)
			{
				val = val | (ImGuiWindowFlags)4;
			}
			if (!options.Resizable || value)
			{
				val = val | (ImGuiWindowFlags)2;
			}
			if (options.ResetPosition.HasValue)
			{
				ImGui.SetNextWindowPos(options.ResetPosition.Value, (ImGuiCond)1);
				options.ResetPosition = null;
			}
			if (options.ResetSize.HasValue)
			{
				ImGui.SetNextWindowSize(options.ResetSize.Value, (ImGuiCond)1);
				options.ResetSize = null;
			}
			float x = options.MinWidth;
			float x2 = options.MaxWidth;
			float y = options.MinHeight;
			float y2 = options.MaxHeight;
			if (options.LockWidth.HasValue)
			{
				x = (x2 = options.LockWidth.Value);
			}
			if (options.LockHeight.HasValue)
			{
				y = (y2 = options.LockHeight.Value);
			}
			ImGui.SetNextWindowSizeConstraints(new Vector2(x, y), new Vector2(x2, y2));
			if (!ImGui.Begin("##" + uniqueID + "Window", val))
			{
				PopStrippedStyles();
				return new DrawResult
				{
					DrawContent = false,
					IsClosed = true
				};
			}
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			Vector2 windowPos = ImGui.GetWindowPos();
			Vector2 windowSize = ImGui.GetWindowSize();
			Vector2 vector = (value ? (windowPos + new Vector2(windowSize.X, options.TitleBarHeight)) : (windowPos + windowSize));
			windowDrawList.AddRectFilled(windowPos, vector, options.BackgroundColor.ToImGui());
			windowDrawList.AddRect(windowPos, vector, options.BorderColor.ToImGui());
			if (!string.IsNullOrEmpty(options.Title))
			{
				windowDrawList.AddText(windowPos + new Vector2(4f, 1f), options.TitleTextColor.ToImGui(), options.Title);
			}
			if (options.Resizable && !value)
			{
				DrawGrip(windowPos, windowSize, options.GripColor.ToImGui());
			}
			Vector2 vector2 = new Vector2(options.TitleBarHeight - 4, options.TitleBarHeight - 4);
			Vector2 cursorScreenPos = windowPos + new Vector2(windowSize.X - vector2.X - 2f, 2f);
			if (options.ShowCloseButton)
			{
				ImGui.SetCursorScreenPos(cursorScreenPos);
				if (Button.Draw("##" + uniqueID + "_close", new Button.Options
				{
					Width = (int)vector2.X,
					Height = (int)vector2.Y,
					Color = options.CloseButtonColor,
					HoveredColor = options.CloseButtonHoverColor,
					Tooltip = new Tooltip.Options("Close Window")
				}))
				{
					ImGui.End();
					PopStrippedStyles();
					return new DrawResult
					{
						DrawContent = false,
						IsClosed = true
					};
				}
			}
			if (options.ShowMinimizeButton)
			{
				cursorScreenPos.X -= vector2.X + 2f;
				ImGui.SetCursorScreenPos(cursorScreenPos);
				if (Button.Draw("##" + uniqueID + "_minimize", new Button.Options
				{
					Width = (int)vector2.X,
					Height = (int)vector2.Y,
					Color = (value ? options.MinimizeButtonMinimisedColor : options.MinimizeButtonColor),
					HoveredColor = options.MinimizeButtonMinimisedColor,
					Tooltip = new Tooltip.Options(value ? "Restore Window" : "Minimize Window")
				}))
				{
					_minimizedStates[uniqueID] = !_minimizedStates[uniqueID];
				}
				if (_minimizedStates[uniqueID])
				{
					ImGui.End();
					PopStrippedStyles();
					return new DrawResult
					{
						DrawContent = false,
						IsClosed = false
					};
				}
			}
			ImGui.SetCursorScreenPos(new Vector2(windowPos.X, windowPos.Y + (float)options.TitleBarHeight));
			return new DrawResult
			{
				DrawContent = true,
				IsClosed = false
			};
		}

		public static void End()
		{
			ImGui.End();
			PopStrippedStyles();
		}

		private static void DrawGrip(Vector2 winPos, Vector2 winSize, uint gripColor)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			float num = 16f;
			Vector2 vector = winPos + new Vector2(winSize.X - num, winSize.Y - num);
			Vector2 vector2 = winPos + new Vector2(winSize.X, winSize.Y);
			Vector2 vector3 = vector + new Vector2(num, 0f);
			Vector2 vector4 = vector + new Vector2(0f, num);
			Vector2 vector5 = vector2;
			windowDrawList.AddTriangleFilled(vector3, vector4, vector5, gripColor);
			ImGuiIOPtr iO = ImGui.GetIO();
			Vector2 mousePos = iO.MousePos;
			if (mousePos.X >= vector.X && mousePos.X <= vector2.X && mousePos.Y >= vector.Y && mousePos.Y <= vector2.Y)
			{
				windowDrawList.AddTriangleFilled(vector3, vector4, vector5, 452984831u);
			}
		}
	}

	public static class Bar
	{
		public class Options
		{
			public string Label { get; set; } = "";

			public int? Width { get; set; }

			public int? Height { get; set; } = 0;

			public System.Drawing.Color? BorderColor { get; set; } = System.Drawing.Color.Black;

			public System.Drawing.Color? InnerGlowColor { get; set; } = Colors.ButtonInnerGlow;

			public System.Drawing.Color Color { get; set; } = Colors.Button;

			public System.Drawing.Color TextColor { get; set; } = Colors.ButtonText;
		}

		public static void Draw(Options? options = null)
		{
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			if (options == null)
			{
				options = new Options();
			}
			int? width = options.Width;
			int num = ((!width.HasValue) ? ((int)Math.Ceiling(ImGui.CalcTextSize(options.Label).X + 10f)) : ((width.GetValueOrDefault() > 0) ? width.Value : ((int)ImGui.GetContentRegionAvail().X + options.Width.Value)));
			int width2 = num;
			int? height = options.Height;
			num = ((!height.HasValue) ? ((int)Math.Ceiling(ImGui.CalcTextSize(options.Label).Y + 4f)) : ((height.GetValueOrDefault() > 0) ? height.Value : ((int)ImGui.GetFrameHeight() + options.Height.Value)));
			int height2 = num;
			DXTRect dXTRect = new DXTRect(ImGui.GetCursorScreenPos(), width2, height2);
			DXTRect dXTRect2 = new DXTRect(dXTRect.TopLeft + new Vector2(1f, 1f), dXTRect.BottomRight - new Vector2(1f, 1f));
			Vector2 vector = new Vector2(0f, 0f);
			Vector2 vector2 = new Vector2(0f, 0f);
			if (!string.IsNullOrEmpty(options.Label))
			{
				vector = ImGui.CalcTextSize(options.Label);
				ImFontPtr font = ImGui.GetFont();
				int num2 = ((font.GetDebugName() == "unifont.otf, 16px") ? 2 : 0);
				vector2 = dXTRect.TopLeft + new Vector2(4f, (float)Math.Ceiling((dXTRect.Height - vector.Y) / 2f) - (float)num2);
			}
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			windowDrawList.AddRectFilled(dXTRect.TopLeft, dXTRect.BottomRight, options.Color.ToImGui());
			if (options.BorderColor.HasValue)
			{
				windowDrawList.AddRect(dXTRect.TopLeft, dXTRect.BottomRight, options.BorderColor.Value.ToImGui());
			}
			if (options.InnerGlowColor.HasValue)
			{
				windowDrawList.AddRect(dXTRect2.TopLeft, dXTRect2.BottomRight, options.InnerGlowColor.Value.ToImGui());
			}
			ImGui.SetCursorScreenPos(dXTRect.TopLeft);
			ImGui.Dummy(new Vector2(dXTRect.Width, dXTRect.Height));
			if (vector.X > 0f)
			{
				windowDrawList.AddText(vector2, options.TextColor.ToImGui(), options.Label);
			}
		}
	}

	public static class Button
	{
		public class Options
		{
			public string Label { get; set; } = "";

			public bool Enabled { get; set; } = true;

			public int? Width { get; set; }

			public int? Height { get; set; } = 0;

			public System.Drawing.Color? BorderColor { get; set; } = System.Drawing.Color.Black;

			public System.Drawing.Color? InnerGlowColor { get; set; } = Colors.ButtonInnerGlow;

			public System.Drawing.Color Color { get; set; } = Colors.Button;

			public System.Drawing.Color TextColor { get; set; } = Colors.ButtonText;

			public System.Drawing.Color CheckedTextColor { get; set; } = Colors.ButtonTextChecked;

			public System.Drawing.Color CheckedColor { get; set; } = Colors.ButtonChecked;

			public System.Drawing.Color HoveredColor { get; set; } = Colors.ButtonHovered;

			public Tooltip.Options? Tooltip { get; set; }

			public Tooltip.Options? DisabledTooltip { get; set; }
		}

		public static bool Draw(string uniqueId, Options? options = null)
		{
			return InternalDraw(uniqueId, options, null) != ClickType.None;
		}

		public static bool Draw(string uniqueId, ref bool checkedState, Options? options = null)
		{
			switch (InternalDraw(uniqueId, options, checkedState))
			{
			case ClickType.None:
				return false;
			case ClickType.Left:
				checkedState = !checkedState;
				break;
			}
			return true;
		}

		private static ClickType InternalDraw(string uniqueId, Options? options, bool? checkedState)
		{
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_0256: Unknown result type (might be due to invalid IL or missing references)
			//IL_025b: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(uniqueId))
			{
				throw new ArgumentException("uniqueId cannot be null or empty", "uniqueId");
			}
			if (options == null)
			{
				throw new ArgumentNullException("options", "Options cannot be null");
			}
			int? width = options.Width;
			int num = ((!width.HasValue) ? ((int)Math.Ceiling(ImGui.CalcTextSize(options.Label).X + 10f)) : ((width.GetValueOrDefault() > 0) ? width.Value : ((int)ImGui.GetContentRegionAvail().X + options.Width.Value)));
			int width2 = num;
			int? height = options.Height;
			num = ((!height.HasValue) ? ((int)Math.Ceiling(ImGui.CalcTextSize(options.Label).Y + 4f)) : ((height.GetValueOrDefault() > 0) ? height.Value : ((int)ImGui.GetFrameHeight() + options.Height.Value)));
			int height2 = num;
			DXTRect dXTRect = new DXTRect(ImGui.GetCursorScreenPos(), width2, height2);
			DXTRect dXTRect2 = new DXTRect(dXTRect.TopLeft + new Vector2(1f, 1f), dXTRect.BottomRight - new Vector2(1f, 1f));
			Vector2 vector = new Vector2(0f, 0f);
			Vector2 vector2 = new Vector2(0f, 0f);
			if (!string.IsNullOrEmpty(options.Label))
			{
				vector = ImGui.CalcTextSize(options.Label);
				ImFontPtr font = ImGui.GetFont();
				int num2 = ((font.GetDebugName() == "unifont.otf, 16px") ? 2 : 0);
				vector2 = dXTRect.TopLeft + new Vector2((float)Math.Round((dXTRect.Width - vector.X) / 2f), (float)Math.Ceiling((dXTRect.Height - vector.Y) / 2f) - (float)num2);
			}
			System.Drawing.Color color = ((checkedState.HasValue && checkedState.Value) ? options.CheckedColor : options.Color);
			System.Drawing.Color color2 = ((checkedState.HasValue && checkedState.Value) ? options.CheckedTextColor : options.TextColor);
			if (!options.Enabled)
			{
				color2 = color2.WithAlpha(0.6f);
				color = color.Desaturate(1f);
			}
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			windowDrawList.AddRectFilled(dXTRect.TopLeft, dXTRect.BottomRight, color.ToImGui());
			if (options.BorderColor.HasValue)
			{
				windowDrawList.AddRect(dXTRect.TopLeft, dXTRect.BottomRight, options.BorderColor.Value.ToImGui());
			}
			ImGui.SetCursorScreenPos(dXTRect.TopLeft);
			ImGui.InvisibleButton("##" + uniqueId, new Vector2(dXTRect.Width, dXTRect.Height));
			if (options.Enabled)
			{
				if (ImGui.IsItemHovered())
				{
					windowDrawList.AddRectFilled(dXTRect2.TopLeft, dXTRect2.BottomRight, options.HoveredColor.ToImGui());
					if (options.Tooltip != null)
					{
						Tooltip.Draw(options.Tooltip);
					}
				}
				if (vector.X > 0f)
				{
					windowDrawList.AddText(vector2, color2.ToImGui(), options.Label);
				}
				if (options.InnerGlowColor.HasValue)
				{
					windowDrawList.AddRect(dXTRect2.TopLeft, dXTRect2.BottomRight, options.InnerGlowColor.Value.ToImGui());
				}
				if (ImGui.IsItemClicked((ImGuiMouseButton)1))
				{
					return ClickType.Right;
				}
				if (ImGui.IsItemClicked((ImGuiMouseButton)0))
				{
					return ClickType.Left;
				}
			}
			else
			{
				if (vector.X > 0f)
				{
					windowDrawList.AddText(vector2, color2.ToImGui(), options.Label);
				}
				if (ImGui.IsItemHovered() && options.DisabledTooltip != null)
				{
					Tooltip.Draw(options.DisabledTooltip);
				}
			}
			return ClickType.None;
		}
	}

	public static class Checkbox
	{
		public class Options
		{
			public int? Height { get; set; }

			public System.Drawing.Color? BorderColor { get; set; } = System.Drawing.Color.Black;

			public System.Drawing.Color? InnerGlowColor { get; set; } = Colors.InputInnerGlow;

			public System.Drawing.Color BackgroundColor { get; set; } = Colors.Input;

			public System.Drawing.Color TextColor { get; set; } = Colors.Text;

			public System.Drawing.Color CheckColor { get; set; } = Colors.ButtonChecked;

			public System.Drawing.Color HoveredColor { get; set; } = Colors.InputHovered;

			public Tooltip.Options? Tooltip { get; set; }
		}

		public static bool Draw(string uniqueId, ref bool value, Options? options = null)
		{
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(uniqueId))
			{
				throw new ArgumentException("uniqueId cannot be null or empty", "uniqueId");
			}
			if (options == null)
			{
				options = new Options();
			}
			DXTRect dXTRect = new DXTRect(ImGui.GetCursorScreenPos(), ((float?)options.Height) ?? ImGui.GetFrameHeight(), ((float?)options.Height) ?? ImGui.GetFrameHeight());
			DXTRect dXTRect2 = dXTRect.Shrink(1f);
			DXTRect dXTRect3 = dXTRect.Shrink(3f);
			DXTRect dXTRect4 = dXTRect;
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			windowDrawList.AddRectFilled(dXTRect.TopLeft, dXTRect.BottomRight, options.BackgroundColor.ToImGui());
			if (options.BorderColor.HasValue)
			{
				windowDrawList.AddRect(dXTRect.TopLeft, dXTRect.BottomRight, options.BorderColor.Value.ToImGui());
			}
			if (options.InnerGlowColor.HasValue)
			{
				windowDrawList.AddRect(dXTRect2.TopLeft, dXTRect2.BottomRight, options.InnerGlowColor.Value.ToImGui());
			}
			if (value)
			{
				Vector2 vector = new Vector2(dXTRect3.TopLeft.X + dXTRect3.Width * 0.15f, dXTRect3.TopLeft.Y + dXTRect3.Height * 0.55f);
				Vector2 vector2 = new Vector2(dXTRect3.TopLeft.X + dXTRect3.Width * 0.4f, dXTRect3.TopLeft.Y + dXTRect3.Height * 0.8f);
				Vector2 vector3 = new Vector2(dXTRect3.TopLeft.X + dXTRect3.Width * 0.85f, dXTRect3.TopLeft.Y + dXTRect3.Height * 0.25f);
				windowDrawList.PathClear();
				windowDrawList.PathLineTo(vector);
				windowDrawList.PathLineTo(vector2);
				windowDrawList.PathLineTo(vector3);
				windowDrawList.PathStroke(options.CheckColor.ToImGui(), (ImDrawFlags)0, 2f);
			}
			ImGui.SetCursorScreenPos(dXTRect4.TopLeft);
			ImGui.InvisibleButton("##" + uniqueId, new Vector2(dXTRect4.Width, dXTRect4.Height));
			if (ImGui.IsItemHovered())
			{
				windowDrawList.AddRectFilled(dXTRect2.TopLeft, dXTRect2.BottomRight, options.HoveredColor.ToImGui());
				if (options.Tooltip != null)
				{
					Tooltip.Draw(options.Tooltip);
				}
			}
			if (ImGui.IsItemClicked((ImGuiMouseButton)0))
			{
				value = !value;
			}
			return value;
		}
	}

	public static class ColorSelect
	{
		public class Options
		{
			public Vector2? ColorPickerWindowOffset;

			public int? Width;

			public int? Height;

			public System.Drawing.Color BorderColor = System.Drawing.Color.Black;
		}

		private static void InternalDraw(string uniqueID, string label, ref System.Drawing.Color color, Options? options = null)
		{
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			if (options == null)
			{
				options = new Options();
			}
			Vector2 vector = new Vector2(((float?)options.Width) ?? ImGui.GetFrameHeight(), ((float?)options.Height) ?? ImGui.GetFrameHeight());
			Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
			bool num = ImGui.InvisibleButton("##" + uniqueID + "InvisibleButton", vector);
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			DXT.Draw.Checkerboard(cursorScreenPos, vector.X, vector.Y);
			windowDrawList.AddRectFilled(cursorScreenPos, cursorScreenPos + vector, color.ToImGui());
			windowDrawList.AddRect(cursorScreenPos, cursorScreenPos + vector, options.BorderColor.ToImGui());
			if (ImGui.IsItemHovered())
			{
				(int, int, int, int) tuple = color.ToHSLA().ToPercentRounded();
				Tooltip.Draw(new Tooltip.Options
				{
					Lines = new List<Tooltip.Line>
					{
						new Tooltip.Title
						{
							Text = label
						},
						new Tooltip.Separator(),
						new Tooltip.DoubleLine
						{
							LeftText = "RGBA:",
							RightText = $"{color.R},{color.G},{color.B},{color.A}"
						},
						new Tooltip.DoubleLine
						{
							LeftText = "HSLA:",
							RightText = $"{tuple.Item1},{tuple.Item2},{tuple.Item3},{tuple.Item4}"
						},
						new Tooltip.DoubleLine
						{
							LeftText = "HEX:",
							RightText = "#" + color.ToHEX()
						}
					}
				});
			}
			if (num)
			{
				ColorPicker.Open(uniqueID, color, options.ColorPickerWindowOffset);
			}
			ColorPicker.Draw(uniqueID, ref color, label);
		}

		public static void Draw(string uniqueID, string label, ref System.Drawing.Color color, Options? options = null)
		{
			InternalDraw(uniqueID, label, ref color, options);
		}
	}

	[Obsolete("Use DXT.Label instead")]
	public static class Display
	{
		public class Options
		{
			public Vector2 PositionOffset { get; set; } = new Vector2(0f, 0f);

			public int? Width { get; set; }

			public int? Height { get; set; }

			public int TextPaddingLeft { get; set; } = 3;

			public int TextPaddingRight { get; set; } = 3;

			public System.Drawing.Color? BackgroundColor { get; set; } = Colors.Input;

			public System.Drawing.Color? BorderColor { get; set; } = System.Drawing.Color.Black;

			public System.Drawing.Color TextColor { get; set; } = Colors.InputText;

			public Tooltip.Options? Tooltip { get; set; }

			public bool DrawBackground { get; set; } = true;
		}

		public static void Draw(string uniqueID, string value, Options? options = null)
		{
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			if (options == null)
			{
				options = new Options();
			}
			DXTRect dXTRect = new DXTRect(ImGui.GetCursorScreenPos(), ((float?)options.Width) ?? ImGui.GetContentRegionAvail().X, ((float?)options.Height) ?? ImGui.GetFrameHeight());
			float num = dXTRect.Width - (float)(options.TextPaddingLeft + options.TextPaddingRight);
			ImFontPtr font = ImGui.GetFont();
			int num2 = ((font.GetDebugName() == "unifont.otf, 16px") ? 2 : 0);
			Vector2 cursorScreenPos = new Vector2(dXTRect.TopLeft.X + (float)options.TextPaddingLeft, dXTRect.TopLeft.Y + (float)Math.Max(Math.Round((dXTRect.Height - ImGui.GetFrameHeight() - (float)num2) * 0.5f), 0.0));
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			if (options.DrawBackground)
			{
				if (options.BackgroundColor.HasValue)
				{
					windowDrawList.AddRectFilled(dXTRect.TopLeft, dXTRect.BottomRight, options.BackgroundColor.Value.ToImGui());
				}
				if (options.BorderColor.HasValue)
				{
					windowDrawList.AddRect(dXTRect.TopLeft, dXTRect.BottomRight, options.BorderColor.Value.ToImGui());
				}
			}
			string text = value;
			if (ImGui.CalcTextSize(text).X > num)
			{
				int num3 = text.Length;
				while (num3 > 0 && ImGui.CalcTextSize(text.Substring(0, num3) + "…").X > num)
				{
					num3--;
				}
				text = ((num3 > 0) ? (text.Substring(0, num3) + "…") : "");
			}
			ImGui.SetCursorScreenPos(cursorScreenPos);
			ImGui.TextColored(options.TextColor.ToVector4(), text);
			ImGui.SetCursorScreenPos(dXTRect.TopLeft);
			ImGui.InvisibleButton("##" + uniqueID + "display", new Vector2(dXTRect.Width, dXTRect.Height));
			if (ImGui.IsItemHovered() && options.Tooltip != null)
			{
				Tooltip.Draw(options.Tooltip);
			}
		}
	}

	public static class IconSelect
	{
		public class Options
		{
			public Vector2? IconPickerWindowOffset;

			public Vector2 PositionOffset { get; set; } = new Vector2(0f, 0f);

			public int? Width { get; set; }

			public int? Height { get; set; }

			public System.Drawing.Color IconColor { get; set; } = System.Drawing.Color.White;

			public System.Drawing.Color Color { get; set; } = Colors.Button;

			public System.Drawing.Color? BorderColor { get; set; } = System.Drawing.Color.Black;

			public System.Drawing.Color? InnerGlowColor { get; set; } = Colors.ButtonInnerGlow;

			public System.Drawing.Color? HoveredColor { get; set; } = Colors.ButtonHovered;
		}

		private static void InternalDraw(string uniqueID, string label, ref int selectedIndex, IconAtlas iconAtlas, Options options)
		{
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(uniqueID))
			{
				throw new ArgumentException("uniqueId cannot be null or empty", "uniqueID");
			}
			if (options == null)
			{
				throw new ArgumentNullException("options", "Options cannot be null");
			}
			Vector2 vector = ImGui.GetCursorScreenPos() + options.PositionOffset;
			float num = ((float?)options.Width) ?? ImGui.GetFrameHeight();
			float num2 = ((float?)options.Height) ?? ImGui.GetFrameHeight();
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			windowDrawList.AddRectFilled(vector, vector + new Vector2(num, num2), options.Color.ToImGui());
			ImGui.SetCursorScreenPos(vector);
			bool num3 = ImGui.InvisibleButton("##" + uniqueID, new Vector2(num, num2));
			if (ImGui.IsItemHovered())
			{
				if (options.HoveredColor.HasValue)
				{
					windowDrawList.AddRectFilled(vector + new Vector2(1f, 1f), vector + new Vector2(num - 1f, num2 - 1f), options.HoveredColor.Value.ToImGui());
				}
				Tooltip.Draw(new Tooltip.Options
				{
					Lines = new List<Tooltip.Line>
					{
						new Tooltip.Title
						{
							Text = label
						},
						new Tooltip.Separator(),
						new Tooltip.DoubleLine
						{
							LeftText = "Index:",
							RightText = $"{selectedIndex}"
						}
					}
				});
			}
			var (vector2, vector3) = iconAtlas.GetIconUVs(selectedIndex);
			windowDrawList.AddImage((IntPtr)iconAtlas.TextureId, vector, vector + new Vector2(num, num2), vector2, vector3, options.IconColor.ToImGui());
			if (options.BorderColor.HasValue)
			{
				windowDrawList.AddRect(vector, vector + new Vector2(num, num2), options.BorderColor.Value.ToImGui());
			}
			if (options.InnerGlowColor.HasValue)
			{
				windowDrawList.AddRect(vector + new Vector2(1f, 1f), vector + new Vector2(num - 1f, num2 - 1f), options.InnerGlowColor.Value.ToImGui());
			}
			if (num3)
			{
				IconPicker.Open(uniqueID, iconAtlas, options.IconPickerWindowOffset);
			}
			IconPicker.Draw(uniqueID, iconAtlas, ref selectedIndex, new IconPicker.Options
			{
				Title = label,
				IconColor = options.IconColor
			});
		}

		public static void Draw(string uniqueID, string label, ref int selectedIndex, IconAtlas iconAtlas, Options? options = null)
		{
			if (options == null)
			{
				options = new Options();
			}
			InternalDraw(uniqueID, label, ref selectedIndex, iconAtlas, options);
		}
	}

	public static class Input
	{
		public class Options
		{
			public int? Width { get; set; }

			public int? Height { get; set; }

			public int PadLeft { get; set; } = 3;

			public int PadRight { get; set; } = 3;

			public System.Drawing.Color BackgroundColor { get; set; } = Colors.Input;

			public System.Drawing.Color TextColor { get; set; } = Colors.InputText;

			public System.Drawing.Color BorderColor { get; set; } = System.Drawing.Color.Black;

			public System.Drawing.Color? InnerGlowColor { get; set; } = Colors.InputInnerGlow;

			public System.Drawing.Color HoveredColor { get; set; } = Colors.InputHovered;

			public Tooltip.Options? Tooltip { get; set; }

			public ImGuiInputTextFlags InputTextFlags { get; set; }
		}

		public static bool Draw(string uniqueID, ref string value, Options options)
		{
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(uniqueID))
			{
				throw new ArgumentException("uniqueID cannot be null or empty", "uniqueID");
			}
			if (options == null)
			{
				options = new Options();
			}
			Vector2 vector = ImGui.CalcTextSize(value);
			int? width = options.Width;
			int num = ((!width.HasValue) ? ((int)Math.Ceiling(vector.X + (float)options.PadLeft + (float)options.PadRight)) : ((width.GetValueOrDefault() > 0) ? width.Value : ((int)ImGui.GetContentRegionAvail().X + options.Width.Value)));
			int width2 = num;
			int? height = options.Height;
			num = ((!height.HasValue) ? ((int)Math.Ceiling(vector.Y + 4f)) : ((height.GetValueOrDefault() > 0) ? height.Value : ((int)ImGui.GetFrameHeight() + options.Height.Value)));
			int height2 = num;
			DXTRect dXTRect = new DXTRect(ImGui.GetCursorScreenPos(), width2, height2);
			DXTRect dXTRect2 = dXTRect.Shrink(1f);
			DXTRect dXTRect3 = new DXTRect(dXTRect.Left + (float)options.PadLeft, dXTRect.Top + (dXTRect.Height - vector.Y) * 0.5f, dXTRect.Right, dXTRect.Bottom);
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			windowDrawList.AddRectFilled(dXTRect.TopLeft, dXTRect.BottomRight, options.BackgroundColor.ToImGui());
			windowDrawList.AddRect(dXTRect.TopLeft, dXTRect.BottomRight, options.BorderColor.ToImGui());
			if (options.InnerGlowColor.HasValue)
			{
				windowDrawList.AddRect(dXTRect2.TopLeft, dXTRect2.BottomRight, options.InnerGlowColor.Value.ToImGui());
			}
			bool result = false;
			ImGui.PushStyleColor((ImGuiCol)7, 0u);
			ImGui.PushStyleColor((ImGuiCol)5, 0u);
			ImGui.PushStyleColor((ImGuiCol)0, options.TextColor.ToImGui());
			ImGui.SetCursorScreenPos(dXTRect3.TopLeft);
			ImGui.PushItemWidth(dXTRect3.Width);
			if (ImGui.InputText("##" + uniqueID + "input", ref value, 64u, options.InputTextFlags))
			{
				result = true;
			}
			if (ImGui.IsItemHovered())
			{
				windowDrawList.AddRectFilled(dXTRect2.TopLeft, dXTRect2.BottomRight, options.HoveredColor.ToImGui());
				if (options.Tooltip != null)
				{
					Tooltip.Draw(options.Tooltip);
				}
			}
			ImGui.PopItemWidth();
			ImGui.PopStyleColor(3);
			return result;
		}
	}

	public static class InputSpinner
	{
		public class Options
		{
			public Vector2 PositionOffset { get; set; } = new Vector2(0f, 0f);

			public int? Width { get; set; }

			public int? Height { get; set; }

			public float Min { get; set; }

			public float Max { get; set; } = 100f;

			public float Step { get; set; } = 1f;

			public float? ShiftStep { get; set; }

			public System.Drawing.Color BackgroundColor { get; set; } = Colors.Input;

			public System.Drawing.Color BorderColor { get; set; } = System.Drawing.Color.Black;

			public Tooltip.Options? Tooltip { get; set; }

			public ImGuiInputTextFlags InputTextFlags { get; set; }
		}

		private static string FormatDisplayValue(float value)
		{
			string text = value.ToString("F1");
			if (text.EndsWith(".0"))
			{
				text = text.Substring(0, text.Length - 2);
			}
			return text;
		}

		private static bool InternalDraw(string uniqueID, ref float value, bool isInt, Options options)
		{
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(uniqueID))
			{
				throw new ArgumentException("uniqueID cannot be null or empty", "uniqueID");
			}
			if (options == null)
			{
				options = new Options();
			}
			Vector2 vector = ImGui.GetCursorScreenPos() + options.PositionOffset;
			float num = ((float?)options.Width) ?? ImGui.GetContentRegionAvail().X;
			float y = ((float?)options.Height) ?? ImGui.GetFrameHeight();
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			windowDrawList.AddRectFilled(vector, vector + new Vector2(num, y), options.BackgroundColor.ToImGui());
			windowDrawList.AddRect(vector, vector + new Vector2(num, y), options.BorderColor.ToImGui());
			bool flag = false;
			string text = (isInt ? ((int)value).ToString() : FormatDisplayValue(value));
			ImGui.PushStyleColor((ImGuiCol)7, 0u);
			ImGui.PushStyleColor((ImGuiCol)5, 0u);
			ImGui.SetCursorScreenPos(vector + new Vector2(3f, 1f));
			ImGui.PushItemWidth(num - 8f);
			if (ImGui.InputText("##" + uniqueID + "input", ref text, 4u, (ImGuiInputTextFlags)1 | options.InputTextFlags))
			{
				flag = true;
			}
			if (ImGui.IsItemHovered())
			{
				ImGuiIOPtr iO = ImGui.GetIO();
				float mouseWheel = iO.MouseWheel;
				if (mouseWheel != 0f)
				{
					if (isInt)
					{
						if (!int.TryParse(text, out var result))
						{
							result = (int)value;
						}
						int num2 = result;
						float num3 = Math.Sign(mouseWheel);
						iO = ImGui.GetIO();
						result = num2 + (int)(num3 * ((iO.KeyShift && options.ShiftStep.HasValue) ? options.ShiftStep.Value : options.Step));
						text = ((int)Math.Clamp(result, options.Min, options.Max)).ToString();
						flag = true;
					}
					else
					{
						if (!float.TryParse(text, out var result2))
						{
							result2 = value;
						}
						float num4 = result2;
						float num5 = Math.Sign(mouseWheel);
						iO = ImGui.GetIO();
						result2 = num4 + num5 * ((iO.KeyShift && options.ShiftStep.HasValue) ? options.ShiftStep.Value : options.Step);
						text = Math.Clamp(result2, options.Min, options.Max).ToString();
						flag = true;
					}
				}
				if (options.Tooltip != null)
				{
					Tooltip.Draw(options.Tooltip);
				}
			}
			if (flag)
			{
				float result4;
				if (isInt)
				{
					int num6 = text.IndexOf('.');
					if (num6 >= 0)
					{
						text = text.Substring(0, num6);
					}
					if (int.TryParse(text, out var result3))
					{
						value = Math.Clamp(result3, options.Min, options.Max);
					}
				}
				else if (float.TryParse(text, out result4))
				{
					value = Math.Clamp(result4, options.Min, options.Max);
				}
			}
			ImGui.PopItemWidth();
			ImGui.PopStyleColor(2);
			return flag;
		}

		public static bool Draw(string uniqueID, ref float value, Options options)
		{
			return InternalDraw(uniqueID, ref value, isInt: false, options);
		}

		public static bool Draw(string uniqueID, ref int value, Options options)
		{
			float value2 = value;
			bool num = InternalDraw(uniqueID, ref value2, isInt: true, options);
			if (num)
			{
				value = (int)value2;
			}
			return num;
		}
	}

	public static class Label
	{
		public class Options
		{
			public int? Width { get; set; } = 0;

			public int? Height { get; set; } = 0;

			public int PadLeft { get; set; }

			public int PadRight { get; set; }

			public TextAlign HorizontalAlign { get; set; }

			public System.Drawing.Color TextColor { get; set; } = Colors.ButtonText;

			public Tooltip.Options? Tooltip { get; set; }

			public bool DrawBG { get; set; }

			public System.Drawing.Color? BackgroundColor { get; set; } = Colors.Input;

			public System.Drawing.Color? BorderColor { get; set; } = System.Drawing.Color.Black;

			public System.Drawing.Color? InnerGlowColor { get; set; } = Colors.InputInnerGlow;

			public System.Drawing.Color? HoveredColor { get; set; } = Colors.InputHovered;
		}

		private static int _labelIdCounter;

		public static void Draw(string text, Options? options = null)
		{
			Draw(new PlainText(text), options);
		}

		public static void Draw(ColoredText text, Options? options = null)
		{
			Draw((IRenderableText)text, options);
		}

		public static void Draw(IRenderableText text, Options? options = null)
		{
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_034f: Unknown result type (might be due to invalid IL or missing references)
			if (options == null)
			{
				options = new Options();
			}
			int? width = options.Width;
			int num = ((!width.HasValue) ? ((int)Math.Ceiling(text.Width + (float)options.PadLeft + (float)options.PadRight)) : ((width.GetValueOrDefault() > 0) ? width.Value : ((int)ImGui.GetContentRegionAvail().X + options.Width.Value)));
			int width2 = num;
			int? height = options.Height;
			num = ((!height.HasValue) ? ((int)Math.Ceiling(text.Height + 4f)) : ((height.GetValueOrDefault() > 0) ? height.Value : ((int)ImGui.GetFrameHeight() + options.Height.Value)));
			int height2 = num;
			DXTRect dXTRect = new DXTRect(ImGui.GetCursorScreenPos(), width2, height2);
			DXTRect dXTRect2 = dXTRect.Shrink(1f);
			float num2 = dXTRect.Width - (float)options.PadLeft - (float)options.PadRight;
			IRenderableText renderableText = ((text.Width > num2) ? text.Ellipsize(num2) : text);
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			if (options.DrawBG)
			{
				if (options.BackgroundColor.HasValue)
				{
					windowDrawList.AddRectFilled(dXTRect.TopLeft, dXTRect.BottomRight, options.BackgroundColor.Value.ToImGui());
				}
				if (options.BorderColor.HasValue)
				{
					windowDrawList.AddRect(dXTRect.TopLeft, dXTRect.BottomRight, options.BorderColor.Value.ToImGui());
				}
				if (options.InnerGlowColor.HasValue)
				{
					windowDrawList.AddRect(dXTRect2.TopLeft, dXTRect2.BottomRight, options.InnerGlowColor.Value.ToImGui());
				}
			}
			ImFontPtr font = ImGui.GetFont();
			int num3 = ((font.GetDebugName() == "unifont.otf, 16px") ? 2 : 0);
			float x = dXTRect.TopLeft.Y + (float)Math.Ceiling((dXTRect.Height - renderableText.Height) / 2f) - (float)num3;
			ImGui.SetCursorScreenPos(dXTRect.TopLeft);
			int value = Interlocked.Increment(ref _labelIdCounter);
			ImGui.InvisibleButton($"##Label{value}", new Vector2(dXTRect.Width, dXTRect.Height));
			if (ImGui.IsItemHovered())
			{
				if (options.HoveredColor.HasValue && options.DrawBG)
				{
					windowDrawList.AddRectFilled(dXTRect.TopLeft, dXTRect.BottomRight, options.HoveredColor.Value.ToImGui());
				}
				if (options.Tooltip != null)
				{
					Tooltip.Draw(options.Tooltip);
				}
			}
			float x2 = dXTRect.TopLeft.X + (float)options.PadLeft;
			if (options.HorizontalAlign == TextAlign.Center)
			{
				x2 = dXTRect.Left + (dXTRect.Width - renderableText.Width) / 2f;
			}
			else if (options.HorizontalAlign == TextAlign.Right)
			{
				x2 = dXTRect.TopLeft.X + dXTRect.Width - renderableText.Width - (float)options.PadRight;
			}
			renderableText.Draw(windowDrawList, new Vector2(MathF.Round(x2), MathF.Round(x)), options.TextColor);
		}
	}

	public static class Select
	{
		public class Options
		{
			public Tooltip.Options? Tooltip { get; set; }

			public List<string>? Items { get; set; }

			public int? Width { get; set; }

			public int? Height { get; set; } = 0;

			public System.Drawing.Color? SelectBorderColor { get; set; } = System.Drawing.Color.Black;

			public System.Drawing.Color? SelectInnerGlowColor { get; set; } = Colors.ButtonInnerGlow;

			public System.Drawing.Color SelectColor { get; set; } = Colors.Button;

			public System.Drawing.Color SelectTextColor { get; set; } = Colors.ButtonText;

			public System.Drawing.Color HoveredColor { get; set; } = Colors.ButtonHovered;

			public DXTPadding DropdownPadding { get; set; } = new DXTPadding(2, 2, 2, 2);

			public int DropdownItemSpacing { get; set; } = 3;

			public System.Drawing.Color DropdownBackgroundColor { get; set; } = Colors.Panel;

			public System.Drawing.Color DropdownBorderColor { get; set; } = System.Drawing.Color.Black;

			public System.Drawing.Color DropdownTextColor { get; set; } = Colors.ButtonText;

			public System.Drawing.Color DropdownSelectedItemColor { get; set; } = Colors.ButtonChecked;

			public System.Drawing.Color DropdownSelectedTextColor { get; set; } = Colors.ButtonTextChecked;
		}

		public static bool Draw(string uniqueId, ref int selected, Options options)
		{
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_042f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0434: Unknown result type (might be due to invalid IL or missing references)
			//IL_045c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0461: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(uniqueId))
			{
				throw new ArgumentException("uniqueId cannot be null or empty", "uniqueId");
			}
			if (options == null)
			{
				throw new ArgumentNullException("options", "Options cannot be null");
			}
			if (options.Items == null || options.Items.Count == 0)
			{
				return false;
			}
			bool result = false;
			string text = options.Items[selected] ?? "";
			DXTPadding dXTPadding = new DXTPadding(4, 8, 5, 7);
			int? width = options.Width;
			int num = ((!width.HasValue) ? ((int)Math.Ceiling(ImGui.CalcTextSize(text).X + 10f)) : ((width.GetValueOrDefault() > 0) ? width.Value : ((int)ImGui.GetContentRegionAvail().X + options.Width.Value)));
			int num2 = num;
			int? height = options.Height;
			num = ((!height.HasValue) ? ((int)Math.Ceiling(ImGui.CalcTextSize(text).Y + 4f)) : ((height.GetValueOrDefault() > 0) ? height.Value : ((int)ImGui.GetFrameHeight() + options.Height.Value)));
			int height2 = num;
			DXTRect dXTRect = new DXTRect(ImGui.GetCursorScreenPos(), num2, height2);
			DXTRect dXTRect2 = new DXTRect(dXTRect.TopLeft + new Vector2(1f, 1f), dXTRect.BottomRight - new Vector2(1f, 1f));
			Vector2 vector = new Vector2(0f, 0f);
			Vector2 vector2 = new Vector2(0f, 0f);
			if (!string.IsNullOrEmpty(text))
			{
				vector = ImGui.CalcTextSize(text);
				ImFontPtr font = ImGui.GetFont();
				int num3 = ((font.GetDebugName() == "unifont.otf, 16px") ? 2 : 0);
				vector2 = dXTRect.TopLeft + new Vector2(dXTPadding.Left, (float)Math.Ceiling((dXTRect.Height - vector.Y) / 2f) - (float)num3);
			}
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			windowDrawList.AddRectFilled(dXTRect.TopLeft, dXTRect.BottomRight, options.SelectColor.ToImGui());
			if (options.SelectBorderColor.HasValue)
			{
				windowDrawList.AddRect(dXTRect.TopLeft, dXTRect.BottomRight, options.SelectBorderColor.Value.ToImGui());
			}
			ImGui.SetCursorScreenPos(dXTRect.TopLeft);
			ImGui.InvisibleButton("##" + uniqueId, new Vector2(dXTRect.Width, dXTRect.Height));
			if (ImGui.IsItemHovered())
			{
				windowDrawList.AddRectFilled(dXTRect2.TopLeft, dXTRect2.BottomRight, options.HoveredColor.ToImGui());
				if (options.Tooltip != null)
				{
					Tooltip.Draw(options.Tooltip);
				}
			}
			float num4 = dXTRect.Height - (float)dXTPadding.Top - (float)dXTPadding.Bottom;
			float num5 = num4;
			float num6 = dXTRect.BottomRight.X - (float)dXTPadding.Right - num5;
			float num7 = dXTRect.Top + (float)dXTPadding.Top;
			Vector2 vector3 = new Vector2(num6, num7);
			Vector2 vector4 = new Vector2(num6 + num5, num7);
			Vector2 vector5 = new Vector2(num6 + (float)Math.Round(num5 / 2f), num7 + num4);
			windowDrawList.AddTriangleFilled(vector3, vector4, vector5, options.SelectTextColor.ToImGui());
			if (vector.X > 0f)
			{
				windowDrawList.AddText(vector2, options.SelectTextColor.ToImGui(), text);
			}
			if (ImGui.IsItemClicked((ImGuiMouseButton)0))
			{
				ImGui.OpenPopup("##" + uniqueId + "_dropdown_popup");
			}
			float fontSize = ImGui.GetFontSize();
			int dropdownItemSpacing = options.DropdownItemSpacing;
			int count = options.Items.Count;
			DXTPadding dropdownPadding = options.DropdownPadding;
			float num8 = (float)count * fontSize + (float)((count > 1) ? ((count - 1) * dropdownItemSpacing) : 0);
			float num9 = (float)dropdownPadding.Top + num8 + (float)dropdownPadding.Bottom;
			ImGui.SetNextWindowPos(dXTRect.BottomLeft);
			ImGui.SetNextWindowSize(new Vector2(dXTRect.Width, num9));
			if (ImGui.BeginPopup("##" + uniqueId + "_dropdown_popup"))
			{
				ImGuiIOPtr iO = ImGui.GetIO();
				float val = iO.DisplaySize.Y - dXTRect.BottomLeft.Y;
				float num10 = Math.Min(num9, val);
				ImDrawListPtr windowDrawList2 = ImGui.GetWindowDrawList();
				Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
				Vector2 vector6 = cursorScreenPos + new Vector2(num2, num10);
				windowDrawList2.AddRectFilled(cursorScreenPos, vector6, options.DropdownBackgroundColor.ToImGui());
				windowDrawList2.AddRect(cursorScreenPos, vector6, options.DropdownBorderColor.ToImGui());
				ImGui.BeginChild("##" + uniqueId + "dropdown_scroll", new Vector2(num2, num10 - (float)options.DropdownPadding.Top - (float)options.DropdownPadding.Bottom), (ImGuiChildFlags)0);
				ImDrawListPtr windowDrawList3 = ImGui.GetWindowDrawList();
				Vector2 cursorScreenPos2 = ImGui.GetCursorScreenPos();
				for (int i = 0; i < count; i++)
				{
					DXTRect dXTRect3 = new DXTRect(cursorScreenPos2 + new Vector2(dropdownPadding.Left, (float)dropdownPadding.Top + (float)i * fontSize + (float)(i * dropdownItemSpacing)), num2 - dropdownPadding.Left - dropdownPadding.Right, fontSize);
					string text2 = options.Items[i];
					Vector2 vector7 = ImGui.CalcTextSize(text2);
					Vector2 vector8 = dXTRect3.TopLeft + new Vector2(6f, (float)Math.Ceiling((dXTRect3.Height - vector7.Y) / 2f));
					if (i == selected)
					{
						windowDrawList3.AddRectFilled(dXTRect3.TopLeft, dXTRect3.BottomRight, options.DropdownSelectedItemColor.ToImGui());
						windowDrawList3.AddText(vector8, options.DropdownSelectedTextColor.ToImGui(), text2);
					}
					else
					{
						windowDrawList3.AddText(vector8, options.DropdownTextColor.ToImGui(), text2);
					}
					if (ImGui.InvisibleButton($"##{uniqueId}_item_{i}", dXTRect3.Size))
					{
						selected = i;
						result = true;
						ImGui.CloseCurrentPopup();
					}
					if (ImGui.IsItemHovered())
					{
						windowDrawList3.AddRectFilled(dXTRect3.TopLeft, dXTRect3.BottomRight, options.HoveredColor.ToImGui());
					}
				}
				ImGui.EndChild();
				ImGui.EndPopup();
			}
			return result;
		}
	}

	public static class Slider
	{
		public class Options
		{
			public int GripWidth { get; set; } = 5;

			public int? Width { get; set; }

			public int? Height { get; set; }

			public float Min { get; set; }

			public float Max { get; set; } = 100f;

			public float Step { get; set; } = 1f;

			public float? ShiftStep { get; set; }

			public System.Drawing.Color BackgroundColor { get; set; } = Colors.Input;

			public System.Drawing.Color BorderColor { get; set; } = System.Drawing.Color.Black;

			public System.Drawing.Color GripColor { get; set; } = Colors.Button;

			public System.Drawing.Color TextColor { get; set; } = Colors.InputText;

			public Tooltip.Options? Tooltip { get; set; }

			public ImGuiInputTextFlags InputTextFlags { get; set; }
		}

		private static readonly uint GripinnerGlow = Color.FromRGBA(255, 255, 255, 10).ToImGui();

		private static readonly uint InputInnerGlow = Colors.InputInnerGlow.ToImGui();

		private static string FormatDisplayValue(float value)
		{
			string text = value.ToString("F1");
			if (text.EndsWith(".0"))
			{
				text = text.Substring(0, text.Length - 2);
			}
			return text;
		}

		private static bool InternalDraw(string uniqueID, ref float value, bool isInt, Options options)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(uniqueID))
			{
				throw new ArgumentException("uniqueID cannot be null or empty", "uniqueID");
			}
			if (options == null)
			{
				options = new Options();
			}
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			DXTRect dXTRect = new DXTRect(ImGui.GetCursorScreenPos(), ((float?)options.Width) ?? ImGui.GetContentRegionAvail().X, ((float?)options.Height) ?? ImGui.GetFrameHeight());
			DXTRect dXTRect2 = new DXTRect(dXTRect.TopLeft + new Vector2(1f, 1f), dXTRect.BottomRight - new Vector2(1f, 1f));
			ImFontPtr font = ImGui.GetFont();
			int num = ((font.GetDebugName() == "unifont.otf, 16px") ? 2 : 0);
			float y = (float)Math.Max(Math.Floor((dXTRect.Height - ImGui.GetFrameHeight() - (float)num) * 0.5f), 0.0);
			windowDrawList.AddRect(dXTRect.TopLeft, dXTRect.BottomRight, options.BorderColor.ToImGui());
			windowDrawList.AddRectFilled(dXTRect2.TopLeft, dXTRect2.BottomRight, options.BackgroundColor.ToImGui());
			windowDrawList.AddRect(dXTRect2.TopLeft, dXTRect2.BottomRight, InputInnerGlow);
			float num2 = (value - options.Min) / (options.Max - options.Min);
			int num3 = Math.Max(1, options.GripWidth);
			int num4 = Math.Clamp((int)Math.Round(dXTRect2.TopLeft.X + num2 * (dXTRect2.Width - 1f) - (float)num3 / 2f), (int)dXTRect2.TopLeft.X, (int)dXTRect2.BottomRight.X - num3);
			DXTRect dXTRect3 = new DXTRect(new Vector2(num4, dXTRect2.TopLeft.Y), new Vector2(num4 + num3, dXTRect2.BottomRight.Y));
			windowDrawList.AddRect(dXTRect3.TopLeft - new Vector2(1f, 1f), dXTRect3.BottomRight + new Vector2(1f, 1f), options.BorderColor.ToImGui());
			windowDrawList.AddRectFilled(dXTRect3.TopLeft, dXTRect3.BottomRight, options.GripColor.ToImGui());
			windowDrawList.AddRect(dXTRect3.TopLeft, dXTRect3.BottomRight, GripinnerGlow);
			bool result = false;
			ImGui.PushStyleColor((ImGuiCol)7, 0u);
			ImGui.PushStyleColor((ImGuiCol)8, 0u);
			ImGui.PushStyleColor((ImGuiCol)9, 0u);
			ImGui.PushStyleColor((ImGuiCol)9, 0u);
			ImGui.PushStyleColor((ImGuiCol)5, 0u);
			ImGui.PushStyleColor((ImGuiCol)19, 0u);
			ImGui.PushStyleColor((ImGuiCol)20, 0u);
			ImGui.PushStyleColor((ImGuiCol)0, options.TextColor.ToImGui());
			ImGui.PushStyleVar((ImGuiStyleVar)11, new Vector2(0f, y));
			ImGui.PushItemWidth(dXTRect.Width);
			ImGui.SetCursorScreenPos(dXTRect.TopLeft);
			if (isInt)
			{
				int num5 = (int)Math.Round(value);
				if (ImGui.SliderInt("##" + uniqueID + "slider", ref num5, (int)options.Min, (int)options.Max))
				{
					value = num5;
					result = true;
				}
			}
			else if (ImGui.SliderFloat("##" + uniqueID + "slider", ref value, options.Min, options.Max))
			{
				result = true;
			}
			ImGui.PopStyleColor(8);
			ImGui.PopStyleVar();
			ImGui.PopItemWidth();
			ImGui.SetCursorScreenPos(dXTRect.TopLeft);
			ImGui.InvisibleButton("##" + uniqueID + "invbutton", new Vector2(dXTRect.Width, dXTRect.Height));
			if (ImGui.IsItemHovered())
			{
				ImGuiIOPtr iO = ImGui.GetIO();
				float mouseWheel = iO.MouseWheel;
				if (mouseWheel != 0f)
				{
					if (isInt)
					{
						int num6 = (int)Math.Round(value);
						num6 += (int)((float)Math.Sign(mouseWheel) * ((Keyboard.IsKeyDown(Keyboard.Keys.LShiftKey) && options.ShiftStep.HasValue) ? options.ShiftStep.Value : options.Step));
						num6 = (int)Math.Clamp(num6, options.Min, options.Max);
						value = num6;
						result = true;
					}
					else
					{
						float num7 = value;
						num7 += (float)Math.Sign(mouseWheel) * ((Keyboard.IsKeyDown(Keyboard.Keys.LShiftKey) && options.ShiftStep.HasValue) ? options.ShiftStep.Value : options.Step);
						num7 = Math.Clamp(num7, options.Min, options.Max);
						value = num7;
						result = true;
					}
				}
				if (options.Tooltip != null)
				{
					Tooltip.Draw(options.Tooltip);
				}
			}
			return result;
		}

		public static bool Draw(string uniqueID, ref float value, Options? option = null)
		{
			Options options = option ?? new Options();
			return InternalDraw(uniqueID, ref value, isInt: false, options);
		}

		public static bool Draw(string uniqueID, ref int value, Options? option = null)
		{
			Options options = option ?? new Options();
			float value2 = value;
			bool num = InternalDraw(uniqueID, ref value2, isInt: true, options);
			if (num)
			{
				value = (int)value2;
			}
			return num;
		}
	}

	public static class Draw
	{
		public static void Checkerboard(Vector2 pos, float width, float height, int cellSize = 4, System.Drawing.Color? col1 = null, System.Drawing.Color? col2 = null)
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			System.Drawing.Color valueOrDefault = col1.GetValueOrDefault();
			if (!col1.HasValue)
			{
				valueOrDefault = Color.FromHEX("CCCCCC");
				col1 = valueOrDefault;
			}
			valueOrDefault = col2.GetValueOrDefault();
			if (!col2.HasValue)
			{
				valueOrDefault = Color.FromHEX("888888");
				col2 = valueOrDefault;
			}
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			int num = (int)Math.Ceiling(width / (float)cellSize);
			int num2 = (int)Math.Ceiling(height / (float)cellSize);
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					uint num3 = (((j + i) % 2 == 0) ? col1.Value.ToImGui() : col2.Value.ToImGui());
					Vector2 vector = pos + new Vector2(j * cellSize, i * cellSize);
					Vector2 vector2 = pos + new Vector2(Math.Min((j + 1) * cellSize, width), Math.Min((i + 1) * cellSize, height));
					windowDrawList.AddRectFilled(vector, vector2, num3);
				}
			}
		}
	}

	public static class ColorPicker
	{
		private const int ControlHeight = 20;

		private const int PopupPaddingBottom = 3;

		private static readonly Vector2 PickerWindowSize = new Vector2(441f, 197f);

		private static readonly Vector2 ContentPadding = new Vector2(10f, 10f);

		private static readonly Vector2 ControlSpacing = new Vector2(8f, 8f);

		private static readonly Vector2 InputSpacing = new Vector2(5f, 5f);

		private static readonly Vector2 SliderSize = new Vector2(362f, 20f);

		private static readonly Vector2 SliderInputSize = new Vector2(45f, 20f);

		private static readonly Vector2 DefaultWindowOffset = new Vector2(10f, 10f);

		private static readonly uint BlackImGui = 4278190080u;

		private static System.Drawing.Color default_color;

		private static DXTHSLA working_HSLA;

		private static Vector2 windowSize = PickerWindowSize;

		private static int calculatedPaletteWindowHeight = 100;

		private static int calculatedPickerWindowHeight = 100;

		private static Dictionary<string, List<PaletteSwatch>> TailwindFiltered => Palettes.Tailwind.All.ToDictionary<KeyValuePair<string, List<PaletteSwatch>>, string, List<PaletteSwatch>>((KeyValuePair<string, List<PaletteSwatch>> kvp) => kvp.Key, (KeyValuePair<string, List<PaletteSwatch>> kvp) => kvp.Value.Where((PaletteSwatch swatch) => !swatch.Name.EndsWith(" 50")).ToList());

		public static void Open(string uniqueID, System.Drawing.Color color, Vector2? windowOffset)
		{
			default_color = color;
			working_HSLA = color.ToHSLA();
			PopupWindow.OpenOnMouse(uniqueID, windowOffset ?? DefaultWindowOffset);
		}

		public static void Draw(string uniqueID, ref System.Drawing.Color color, string Title = "")
		{
			windowSize.Y = calculatedPickerWindowHeight + calculatedPaletteWindowHeight;
			if (PopupWindow.Begin(uniqueID, new PopupWindow.Options
			{
				Size = windowSize,
				Title = "Color Picker: " + Title,
				PanelPadding = new DXTPadding(3, 0, 3, calculatedPaletteWindowHeight + 3)
			}))
			{
				float y = ImGui.GetWindowPos().Y;
				Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
				Vector2 vector = cursorScreenPos + ContentPadding;
				DXTHSLA dXTHSLA = working_HSLA;
				DrawHueSlider(vector, ref working_HSLA);
				vector.Y += 20f + ControlSpacing.Y;
				DrawSaturationSlider(vector, ref working_HSLA);
				vector.Y += 20f + ControlSpacing.Y;
				DrawLightnessSlider(vector, ref working_HSLA);
				vector.Y += 20f + ControlSpacing.Y;
				DrawAlphaSlider(vector, ref working_HSLA);
				if (working_HSLA != dXTHSLA)
				{
					color = Color.FromHsla(working_HSLA.H, working_HSLA.S, working_HSLA.L, working_HSLA.A);
				}
				System.Drawing.Color color2 = color;
				vector.Y += 20f + ControlSpacing.Y;
				int num = (int)(SliderInputSize.X * 3f + InputSpacing.X * 2f);
				DrawHexColorInput(vector, num, ref color);
				vector.X += (float)num + ControlSpacing.X;
				int num2 = (int)(SliderSize.X - (float)num - ControlSpacing.X);
				DrawRGBAInput(vector, num2, ref color);
				vector.X += ControlSpacing.X + (float)num2;
				DrawColorWithReset(vector, (int)SliderInputSize.X, (int)SliderInputSize.X, ref color);
				vector.X = cursorScreenPos.X + ContentPadding.X;
				vector.Y += 20f + InputSpacing.Y;
				ImGui.SetCursorScreenPos(vector);
				int value = color.R;
				if (InputSpinner.Draw("ColorPickerRed", ref value, new InputSpinner.Options
				{
					Width = (int)SliderInputSize.X,
					Height = 20,
					Max = 255f,
					Tooltip = new Tooltip.Options("Red Component")
				}))
				{
					value = Math.Clamp(value, 0, 255);
					color = System.Drawing.Color.FromArgb(color.A, value, color.G, color.B);
				}
				vector.X += SliderInputSize.X + InputSpacing.X;
				ImGui.SetCursorScreenPos(vector);
				int value2 = color.G;
				if (InputSpinner.Draw("ColorPickerGreen", ref value2, new InputSpinner.Options
				{
					Width = (int)SliderInputSize.X,
					Height = 20,
					Max = 255f,
					Tooltip = new Tooltip.Options("Green Component")
				}))
				{
					value2 = Math.Clamp(value2, 0, 255);
					color = System.Drawing.Color.FromArgb(color.A, color.R, value2, color.B);
				}
				vector.X += SliderInputSize.X + InputSpacing.X;
				ImGui.SetCursorScreenPos(vector);
				int value3 = color.B;
				if (InputSpinner.Draw("ColorPickerBlue", ref value3, new InputSpinner.Options
				{
					Width = (int)SliderInputSize.X,
					Height = 20,
					Max = 255f,
					Tooltip = new Tooltip.Options("Blue Component")
				}))
				{
					value3 = Math.Clamp(value3, 0, 255);
					color = System.Drawing.Color.FromArgb(color.A, color.R, color.G, value3);
				}
				vector.X += SliderInputSize.X + ControlSpacing.X;
				ImGui.SetCursorScreenPos(vector);
				Vector2 pos = vector;
				vector.Y += 20f + ContentPadding.Y;
				calculatedPickerWindowHeight = (int)(vector.Y - y - 1f + 3f);
				vector.X = cursorScreenPos.X;
				vector.Y += 3f;
				ImGui.SetCursorScreenPos(vector);
				calculatedPaletteWindowHeight = DrawPalette(TailwindFiltered, 435, 16, new Vector2(-1f, -1f), ref color) + 3;
				if (color2 != color)
				{
					working_HSLA = color.ToHSLA();
				}
				dXTHSLA = working_HSLA;
				DrawHSLAInput(pos, num2, ref working_HSLA);
				if (working_HSLA != dXTHSLA)
				{
					color = Color.FromHsla(working_HSLA.H, working_HSLA.S, working_HSLA.L, working_HSLA.A);
				}
				PopupWindow.End();
			}
		}

		private static void DrawHueSlider(Vector2 pos, ref DXTHSLA working_HSLA)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			windowDrawList.AddRect(pos, pos + SliderSize, BlackImGui);
			Vector2 vector = pos + new Vector2(1f, 1f);
			Vector2 vector2 = SliderSize - new Vector2(2f, 2f);
			int num = 180;
			float num2 = vector2.X / (float)num;
			for (int i = 0; i < num; i++)
			{
				float h = (float)i * 360f / (float)num;
				DXTHSLA dXTHSLA = working_HSLA;
				dXTHSLA.H = h;
				dXTHSLA.A = 1f;
				uint num3 = dXTHSLA.ToImGui();
				Vector2 vector3 = vector + new Vector2((float)i * num2, 0f);
				Vector2 vector4 = vector + new Vector2((float)(i + 1) * num2, vector2.Y);
				windowDrawList.AddRectFilled(vector3, vector4, num3);
			}
			float num4 = (float)Math.Round(vector.X + working_HSLA.H / 360f * vector2.X);
			Vector2 vector5 = new Vector2(num4 - 1f, vector.Y - 1f);
			Vector2 vector6 = new Vector2(num4 + 1f, vector.Y + vector2.Y + 1f);
			windowDrawList.AddRectFilled(vector5, vector6, uint.MaxValue);
			windowDrawList.AddRect(vector5 - new Vector2(1f, 1f), vector6 + new Vector2(1f, 1f), BlackImGui);
			ImGui.SetCursorScreenPos(pos);
			ImGui.InvisibleButton("##hueslidergrip", SliderSize);
			ImGuiIOPtr iO;
			if (ImGui.IsItemActive() && ImGui.IsMouseDown((ImGuiMouseButton)0))
			{
				iO = ImGui.GetIO();
				float num5 = Math.Clamp(iO.MousePos.X - vector.X, 0f, vector2.X);
				working_HSLA.H = num5 / vector2.X * 360f;
			}
			if (ImGui.IsItemHovered())
			{
				iO = ImGui.GetIO();
				float mouseWheel = iO.MouseWheel;
				if (mouseWheel != 0f)
				{
					working_HSLA.H += mouseWheel;
				}
				Tooltip.Draw(new Tooltip.Options
				{
					Lines = { (Tooltip.Line)new Tooltip.Title
					{
						Text = "Hue Slider"
					} }
				});
			}
			ImGui.SetCursorScreenPos(pos + new Vector2(SliderSize.X + ControlSpacing.X, 0f));
			int value = (int)Math.Round(working_HSLA.H);
			if (InputSpinner.Draw("ColorPickerHueSlider", ref value, new InputSpinner.Options
			{
				Width = (int)SliderInputSize.X,
				Height = (int)SliderInputSize.Y,
				Max = 360f
			}))
			{
				working_HSLA.H = value;
			}
		}

		private static void DrawSaturationSlider(Vector2 pos, ref DXTHSLA working_HSLA)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			windowDrawList.AddRect(pos, pos + SliderSize, BlackImGui);
			Vector2 vector = pos + new Vector2(1f, 1f);
			Vector2 vector2 = SliderSize - new Vector2(2f, 2f);
			int num = 100;
			float num2 = vector2.X / (float)num;
			for (int i = 0; i < num; i++)
			{
				DXTHSLA dXTHSLA = working_HSLA;
				dXTHSLA.A = 1f;
				dXTHSLA.S = (float)i / (float)(num - 1);
				uint num3 = dXTHSLA.ToImGui();
				Vector2 vector3 = vector + new Vector2((float)i * num2, 0f);
				Vector2 vector4 = vector + new Vector2((float)(i + 1) * num2, vector2.Y);
				windowDrawList.AddRectFilled(vector3, vector4, num3);
			}
			float num4 = (float)Math.Round(vector.X + working_HSLA.S * vector2.X);
			Vector2 vector5 = new Vector2(num4 - 1f, vector.Y - 1f);
			Vector2 vector6 = new Vector2(num4 + 1f, vector.Y + vector2.Y + 1f);
			windowDrawList.AddRectFilled(vector5, vector6, uint.MaxValue);
			windowDrawList.AddRect(vector5 - new Vector2(1f, 1f), vector6 + new Vector2(1f, 1f), BlackImGui);
			ImGui.SetCursorScreenPos(pos);
			ImGui.InvisibleButton("##satslidergrip", SliderSize);
			ImGuiIOPtr iO;
			if (ImGui.IsItemActive() && ImGui.IsMouseDown((ImGuiMouseButton)0))
			{
				iO = ImGui.GetIO();
				float num5 = Math.Clamp(iO.MousePos.X - vector.X, 0f, vector2.X);
				working_HSLA.S = num5 / vector2.X;
			}
			if (ImGui.IsItemHovered())
			{
				iO = ImGui.GetIO();
				float mouseWheel = iO.MouseWheel;
				if (mouseWheel != 0f)
				{
					iO = ImGui.GetIO();
					if (iO.KeyShift)
					{
						float num6 = MathF.Round(working_HSLA.S * 1000f);
						working_HSLA.S = Math.Clamp((num6 + mouseWheel) / 1000f, 0f, 1f);
					}
					else
					{
						float num7 = MathF.Round(working_HSLA.S * 100f);
						working_HSLA.S = Math.Clamp((num7 + mouseWheel) / 100f, 0f, 1f);
					}
				}
				Tooltip.Draw(new Tooltip.Options
				{
					Lines = { (Tooltip.Line)new Tooltip.Title
					{
						Text = "Saturation Slider"
					} }
				});
			}
			ImGui.SetCursorScreenPos(pos + new Vector2(SliderSize.X + ControlSpacing.X, 0f));
			float value = working_HSLA.S * 100f;
			if (InputSpinner.Draw("ColorPickerSatSlider", ref value, new InputSpinner.Options
			{
				Width = (int)SliderInputSize.X,
				Height = (int)SliderInputSize.Y
			}))
			{
				working_HSLA.S = value / 100f;
			}
		}

		private static void DrawLightnessSlider(Vector2 pos, ref DXTHSLA working_HSLA)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			windowDrawList.AddRect(pos, pos + SliderSize, BlackImGui);
			Vector2 vector = pos + new Vector2(1f, 1f);
			Vector2 vector2 = SliderSize - new Vector2(2f, 2f);
			int num = 100;
			float num2 = vector2.X / (float)num;
			for (int i = 0; i < num; i++)
			{
				DXTHSLA dXTHSLA = working_HSLA;
				dXTHSLA.A = 1f;
				dXTHSLA.L = (float)i / (float)(num - 1);
				uint num3 = dXTHSLA.ToImGui();
				Vector2 vector3 = vector + new Vector2((float)i * num2, 0f);
				Vector2 vector4 = vector + new Vector2((float)(i + 1) * num2, vector2.Y);
				windowDrawList.AddRectFilled(vector3, vector4, num3);
			}
			float num4 = (float)Math.Round(vector.X + working_HSLA.L * vector2.X);
			Vector2 vector5 = new Vector2(num4 - 1f, vector.Y - 1f);
			Vector2 vector6 = new Vector2(num4 + 1f, vector.Y + vector2.Y + 1f);
			windowDrawList.AddRectFilled(vector5, vector6, uint.MaxValue);
			windowDrawList.AddRect(vector5 - new Vector2(1f, 1f), vector6 + new Vector2(1f, 1f), BlackImGui);
			ImGui.SetCursorScreenPos(pos);
			ImGui.InvisibleButton("##lightslidergrip", SliderSize);
			ImGuiIOPtr iO;
			if (ImGui.IsItemActive() && ImGui.IsMouseDown((ImGuiMouseButton)0))
			{
				iO = ImGui.GetIO();
				float num5 = Math.Clamp(iO.MousePos.X - vector.X, 0f, vector2.X);
				working_HSLA.L = num5 / vector2.X;
			}
			if (ImGui.IsItemHovered())
			{
				iO = ImGui.GetIO();
				float mouseWheel = iO.MouseWheel;
				if (mouseWheel != 0f)
				{
					iO = ImGui.GetIO();
					if (iO.KeyShift)
					{
						float num6 = MathF.Round(working_HSLA.L * 1000f);
						working_HSLA.L = Math.Clamp((num6 + mouseWheel) / 1000f, 0f, 1f);
					}
					else
					{
						float num7 = MathF.Round(working_HSLA.L * 100f);
						working_HSLA.L = Math.Clamp((num7 + mouseWheel) / 100f, 0f, 1f);
					}
				}
				Tooltip.Draw(new Tooltip.Options
				{
					Lines = { (Tooltip.Line)new Tooltip.Title
					{
						Text = "Lightness Slider"
					} }
				});
			}
			ImGui.SetCursorScreenPos(pos + new Vector2(SliderSize.X + ControlSpacing.X, 0f));
			float value = working_HSLA.L * 100f;
			if (InputSpinner.Draw("ColorPickerLightnessSlider", ref value, new InputSpinner.Options
			{
				Width = (int)SliderInputSize.X,
				Height = (int)SliderInputSize.Y
			}))
			{
				working_HSLA.L = value / 100f;
			}
		}

		private static void DrawAlphaSlider(Vector2 pos, ref DXTHSLA working_HSLA)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			windowDrawList.AddRect(pos, pos + SliderSize, BlackImGui);
			Vector2 vector = pos + new Vector2(1f, 1f);
			Vector2 vector2 = SliderSize - new Vector2(2f, 2f);
			DXT.Draw.Checkerboard(vector, vector2.X, vector2.Y);
			int num = 100;
			float num2 = vector2.X / (float)num;
			for (int i = 0; i < num; i++)
			{
				DXTHSLA dXTHSLA = working_HSLA;
				dXTHSLA.A = (float)i / (float)(num - 1);
				uint num3 = dXTHSLA.ToImGui();
				Vector2 vector3 = vector + new Vector2((float)i * num2, 0f);
				Vector2 vector4 = vector + new Vector2((float)(i + 1) * num2, vector2.Y);
				windowDrawList.AddRectFilled(vector3, vector4, num3);
			}
			float num4 = (float)Math.Round(vector.X + working_HSLA.A * vector2.X);
			Vector2 vector5 = new Vector2(num4 - 1f, vector.Y - 1f);
			Vector2 vector6 = new Vector2(num4 + 1f, vector.Y + vector2.Y + 1f);
			windowDrawList.AddRectFilled(vector5, vector6, uint.MaxValue);
			windowDrawList.AddRect(vector5 - new Vector2(1f, 1f), vector6 + new Vector2(1f, 1f), BlackImGui);
			ImGui.SetCursorScreenPos(pos);
			ImGui.InvisibleButton("##alphaslidergrip", SliderSize);
			ImGuiIOPtr iO;
			if (ImGui.IsItemActive() && ImGui.IsMouseDown((ImGuiMouseButton)0))
			{
				iO = ImGui.GetIO();
				float num5 = Math.Clamp(iO.MousePos.X - vector.X, 0f, vector2.X);
				working_HSLA.A = num5 / vector2.X;
			}
			if (ImGui.IsItemHovered())
			{
				iO = ImGui.GetIO();
				float mouseWheel = iO.MouseWheel;
				if (mouseWheel != 0f)
				{
					iO = ImGui.GetIO();
					if (iO.KeyShift)
					{
						float num6 = MathF.Round(working_HSLA.A * 1000f);
						working_HSLA.A = Math.Clamp((num6 + mouseWheel) / 1000f, 0f, 1f);
					}
					else
					{
						float num7 = MathF.Round(working_HSLA.A * 100f);
						working_HSLA.A = Math.Clamp((num7 + mouseWheel) / 100f, 0f, 1f);
					}
				}
				Tooltip.Draw(new Tooltip.Options
				{
					Lines = { (Tooltip.Line)new Tooltip.Title
					{
						Text = "Alpha Slider"
					} }
				});
			}
			ImGui.SetCursorScreenPos(pos + new Vector2(SliderSize.X + ControlSpacing.X, 0f));
			float value = working_HSLA.A * 100f;
			if (InputSpinner.Draw("ColorPickerAlphaSlider", ref value, new InputSpinner.Options
			{
				Width = (int)SliderInputSize.X,
				Height = (int)SliderInputSize.Y
			}))
			{
				working_HSLA.A = value / 100f;
			}
		}

		private static void DrawRGBAInput(Vector2 pos, int inputWidth, ref System.Drawing.Color color)
		{
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			string text = $"{color.R},{color.G},{color.B},{color.A}";
			ImGui.SetCursorScreenPos(pos);
			Label.Draw(text, new Label.Options
			{
				Width = inputWidth,
				Height = 20,
				DrawBG = true,
				PadLeft = 3
			});
			if (!ImGui.IsItemHovered())
			{
				return;
			}
			ImGuiIOPtr iO = ImGui.GetIO();
			if (iO.KeyCtrl && ImGui.IsMouseClicked((ImGuiMouseButton)0))
			{
				ImGui.SetClipboardText(text);
			}
			iO = ImGui.GetIO();
			if (iO.KeyCtrl && ImGui.IsMouseClicked((ImGuiMouseButton)1))
			{
				int result;
				string[] array = (from Match m in Regex.Matches(ImGui.GetClipboardText() ?? "", "\\b([0-9]{1,3})\\b")
					select m.Value into s
					where int.TryParse(s, out result) && result >= 0 && result <= 255
					select s).ToArray();
				if (array.Length == 4)
				{
					int r = int.Parse(array[0]);
					int g = int.Parse(array[1]);
					int b = int.Parse(array[2]);
					int a = int.Parse(array[3]);
					color = Color.FromRGBA(r, g, b, a);
				}
				else if (array.Length == 3)
				{
					int r2 = int.Parse(array[0]);
					int g2 = int.Parse(array[1]);
					int b2 = int.Parse(array[2]);
					color = Color.FromRGBA(r2, g2, b2);
				}
			}
			Tooltip.Draw(new Tooltip.Options
			{
				Lines = 
				{
					(Tooltip.Line)new Tooltip.Title
					{
						Text = $"Red: {color.R} Green: {color.G} Blue: {color.B} Alpha: {color.A}"
					},
					(Tooltip.Line)new Tooltip.Separator(),
					(Tooltip.Line)new Tooltip.DoubleLine
					{
						LeftText = "Ctrl + LeftClick:",
						RightText = "Copy to clipboard"
					},
					(Tooltip.Line)new Tooltip.DoubleLine
					{
						LeftText = "Ctrl + RightClick:",
						RightText = "Paste from clipboard"
					}
				}
			});
		}

		private static void DrawHSLAInput(Vector2 pos, int inputWidth, ref DXTHSLA hsla)
		{
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			string text = $"{(int)Math.Round(hsla.H)},{(int)Math.Round(hsla.S * 100f)},{(int)Math.Round(hsla.L * 100f)},{(int)Math.Round(hsla.A * 100f)}";
			ImGui.SetCursorScreenPos(pos);
			Label.Draw(text, new Label.Options
			{
				Width = inputWidth,
				Height = 20,
				DrawBG = true,
				PadLeft = 3
			});
			if (!ImGui.IsItemHovered())
			{
				return;
			}
			ImGuiIOPtr iO = ImGui.GetIO();
			if (iO.KeyCtrl && ImGui.IsMouseClicked((ImGuiMouseButton)0))
			{
				ImGui.SetClipboardText(text);
			}
			iO = ImGui.GetIO();
			if (iO.KeyCtrl && ImGui.IsMouseClicked((ImGuiMouseButton)1))
			{
				int result;
				string[] array = (from Match m in Regex.Matches(ImGui.GetClipboardText() ?? "", "\\b([0-9]{1,3})\\b")
					select m.Value into s
					where int.TryParse(s, out result) && result >= 0 && result <= 360
					select s).ToArray();
				if (array.Length == 4)
				{
					int num = int.Parse(array[0]);
					int num2 = int.Parse(array[1]);
					int num3 = int.Parse(array[2]);
					int num4 = int.Parse(array[3]);
					hsla = new DXTHSLA(num, (float)num2 / 100f, (float)num3 / 100f, (float)num4 / 100f);
				}
				else if (array.Length == 3)
				{
					int num5 = int.Parse(array[0]);
					int num6 = int.Parse(array[1]);
					int num7 = int.Parse(array[2]);
					hsla = new DXTHSLA(num5, (float)num6 / 100f, (float)num7 / 100f);
				}
			}
			Tooltip.Draw(new Tooltip.Options
			{
				Lines = 
				{
					(Tooltip.Line)new Tooltip.Title
					{
						Text = $"Hue: {(int)Math.Round(hsla.H)} Saturation: {(int)Math.Round(hsla.S * 100f)} Lightness: {(int)Math.Round(hsla.L * 100f)} Alpha: {(int)Math.Round(hsla.A * 100f)}"
					},
					(Tooltip.Line)new Tooltip.Separator(),
					(Tooltip.Line)new Tooltip.DoubleLine
					{
						LeftText = "Ctrl + LeftClick:",
						RightText = "Copy to clipboard"
					},
					(Tooltip.Line)new Tooltip.DoubleLine
					{
						LeftText = "Ctrl + RightClick:",
						RightText = "Paste from clipboard"
					}
				}
			});
		}

		private static void DrawHexColorInput(Vector2 pos, int inputWidth, ref System.Drawing.Color color)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			string text = color.ToHEX();
			ImGui.SetCursorScreenPos(pos);
			Display.Draw("##ColorPickerHexInput", text, new Display.Options
			{
				Width = inputWidth,
				Height = 20
			});
			if (!ImGui.IsItemHovered())
			{
				return;
			}
			ImGuiIOPtr iO = ImGui.GetIO();
			if (iO.KeyCtrl && ImGui.IsMouseClicked((ImGuiMouseButton)0))
			{
				ImGui.SetClipboardText(text);
			}
			iO = ImGui.GetIO();
			if (iO.KeyCtrl && ImGui.IsMouseClicked((ImGuiMouseButton)1))
			{
				string text2 = (ImGui.GetClipboardText() ?? "").Replace("#", "");
				if ((text2.Length == 6 || text2.Length == 8) && text2.All((char c) => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
				{
					try
					{
						color = System.Drawing.Color.FromArgb((text2.Length == 8) ? Convert.ToInt32(text2.Substring(6, 2), 16) : 255, Convert.ToInt32(text2.Substring(0, 2), 16), Convert.ToInt32(text2.Substring(2, 2), 16), Convert.ToInt32(text2.Substring(4, 2), 16));
					}
					catch
					{
					}
				}
			}
			Tooltip.Draw(new Tooltip.Options
			{
				Lines = 
				{
					(Tooltip.Line)new Tooltip.Title
					{
						Text = "Color Slider"
					},
					(Tooltip.Line)new Tooltip.Separator(),
					(Tooltip.Line)new Tooltip.DoubleLine
					{
						LeftText = "Ctrl + LeftClick:",
						RightText = "Copy to clipboard"
					},
					(Tooltip.Line)new Tooltip.DoubleLine
					{
						LeftText = "Ctrl + RightClick:",
						RightText = "Paste from clipboard"
					}
				}
			});
		}

		private static void DrawColorWithReset(Vector2 pos, int inputWidth, int inputHeight, ref System.Drawing.Color color)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			int num = 19;
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			DXT.Draw.Checkerboard(pos, inputWidth, inputHeight);
			windowDrawList.AddRectFilled(pos, pos + new Vector2(inputWidth, inputHeight), color.ToImGui());
			windowDrawList.AddRect(pos + new Vector2(1f, 1f), pos + new Vector2(inputWidth - 1, inputHeight - 1), Colors.SwatchInnerGlow.ToImGui());
			windowDrawList.AddRect(pos, pos + new Vector2(inputWidth, inputHeight), BlackImGui);
			DXT.Draw.Checkerboard(pos, num, num);
			windowDrawList.AddRectFilled(pos, pos + new Vector2(num, num), default_color.ToImGui());
			windowDrawList.AddRect(pos + new Vector2(1f, 1f), pos + new Vector2(num - 1, num - 1), Colors.SwatchInnerGlow.ToImGui());
			windowDrawList.AddRect(pos, pos + new Vector2(num, num), BlackImGui);
			ImGui.SetCursorScreenPos(pos);
			if (ImGui.InvisibleButton("##ResetColorBox", new Vector2(num, num)))
			{
				color = default_color;
			}
			if (ImGui.IsItemHovered())
			{
				Tooltip.Draw(new Tooltip.Options
				{
					Lines = 
					{
						(Tooltip.Line)new Tooltip.Title
						{
							Text = "Original Color"
						},
						(Tooltip.Line)new Tooltip.Separator(),
						(Tooltip.Line)new Tooltip.DoubleLine
						{
							LeftText = "Red:",
							RightText = default_color.R.ToString()
						},
						(Tooltip.Line)new Tooltip.DoubleLine
						{
							LeftText = "Green:",
							RightText = default_color.G.ToString()
						},
						(Tooltip.Line)new Tooltip.DoubleLine
						{
							LeftText = "Blue:",
							RightText = default_color.B.ToString()
						},
						(Tooltip.Line)new Tooltip.DoubleLine
						{
							LeftText = "Alpha:",
							RightText = default_color.A.ToString()
						}
					}
				});
			}
		}

		public static int DrawPalette(Dictionary<string, List<PaletteSwatch>> palette, int paletteWidth, int colorHeight, Vector2 colorSpacing, ref System.Drawing.Color selectedColor)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
			int count = palette.Count;
			int num = (int)Math.Round(colorSpacing.X);
			int num2 = (count - 1) * num;
			int num3 = (paletteWidth - num2) / count;
			int num4 = count * num3 + num2;
			int num5 = (paletteWidth - num4) / 2;
			int num6 = num5;
			int num7 = palette.Max<KeyValuePair<string, List<PaletteSwatch>>>((KeyValuePair<string, List<PaletteSwatch>> g) => g.Value.Count);
			int num8 = (int)Math.Round(colorSpacing.Y);
			int num9 = num7 * colorHeight + ((num7 > 1) ? ((num7 - 1) * num8) : 0);
			Vector2 vector = cursorScreenPos;
			Vector2 vector2 = cursorScreenPos + new Vector2(paletteWidth, num9 + 2 * num6);
			windowDrawList.AddRectFilled(vector, vector2, Colors.Panel.ToImGui());
			windowDrawList.AddRect(vector, vector2, Colors.PanelInnerGlow.ToImGui());
			int num10 = 0;
			foreach (KeyValuePair<string, List<PaletteSwatch>> item in palette)
			{
				int num11 = (int)Math.Round(cursorScreenPos.X + (float)num5 + (float)(num10 * (num3 + num)));
				int num12 = (int)Math.Round(cursorScreenPos.Y + (float)num6);
				int count2 = item.Value.Count;
				for (int num13 = 0; num13 < count2; num13++)
				{
					PaletteSwatch paletteSwatch = item.Value[num13];
					int num14 = num12 + num13 * (colorHeight + num8);
					Vector2 vector3 = new Vector2(num11, num14);
					windowDrawList.AddRectFilled(vector3, vector3 + new Vector2(num3, colorHeight), paletteSwatch.Uint);
					windowDrawList.AddRect(vector3 + new Vector2(1f, 1f), vector3 + new Vector2(num3 - 1, colorHeight - 1), Colors.SwatchInnerGlow.ToImGui());
					windowDrawList.AddRect(vector3, vector3 + new Vector2(num3, colorHeight), 4278190080u);
					ImGui.SetCursorScreenPos(vector3);
					if (ImGui.InvisibleButton($"##swatch_{item.Key}_{num13}", new Vector2(num3, colorHeight)))
					{
						selectedColor = paletteSwatch.Color;
					}
					if (ImGui.IsItemHovered())
					{
						Tooltip.Draw(new Tooltip.Options
						{
							Lines = { (Tooltip.Line)new Tooltip.Title
							{
								Text = (paletteSwatch.Name ?? "")
							} }
						});
					}
				}
				num10++;
			}
			return num9 + 2 * num6;
		}
	}

	public enum ToolbarOrientation
	{
		Horizontal,
		Vertical
	}

	public static class FloatingToolbar
	{
		public abstract class Tool
		{
			public abstract Vector2 GetSize();

			public abstract void Draw(string uniqueID, Vector2 position, Vector2 forcedSize);
		}

		public class Button : Tool
		{
			public string Label { get; set; } = "";

			public int? Width { get; set; }

			public int? Height { get; set; }

			public Action? OnClick { get; set; }

			public System.Drawing.Color? Color { get; set; }

			public Func<bool>? GetChecked { get; set; }

			public Action<bool>? SetChecked { get; set; }

			public Tooltip.Options? Tooltip { get; set; }

			public override Vector2 GetSize()
			{
				Vector2 vector = ImGui.CalcTextSize(Label);
				int num = Width ?? ((int)Math.Ceiling(vector.X + 10f));
				int num2 = Height ?? ((int)Math.Ceiling(vector.Y + 4f));
				return new Vector2(num, num2);
			}

			public override void Draw(string uniqueID, Vector2 position, Vector2 forcedSize)
			{
				ImGui.SetCursorScreenPos(position);
				DXT.Button.Options options = new DXT.Button.Options
				{
					Label = Label,
					Width = (int)forcedSize.X,
					Height = (int)forcedSize.Y
				};
				if (Tooltip != null)
				{
					options.Tooltip = Tooltip;
				}
				if (Color.HasValue)
				{
					options.Color = Color.Value;
				}
				if (GetChecked != null && SetChecked != null)
				{
					bool checkedState = GetChecked();
					if (DXT.Button.Draw(uniqueID, ref checkedState, options))
					{
						SetChecked(checkedState);
						OnClick?.Invoke();
					}
				}
				else if (DXT.Button.Draw(uniqueID, options))
				{
					OnClick?.Invoke();
				}
			}
		}

		public class Label : Tool
		{
			public string Text { get; set; } = "";

			public System.Drawing.Color BackgroundColor { get; set; } = System.Drawing.Color.Black;

			public System.Drawing.Color TextColor { get; set; } = Colors.TextonControl;

			public int? Width { get; set; }

			public int? Height { get; set; }

			public override Vector2 GetSize()
			{
				Vector2 vector = ImGui.CalcTextSize(Text);
				int num = Width ?? ((int)Math.Ceiling(vector.X + 4f));
				int num2 = Height ?? ((int)Math.Ceiling(vector.Y + 4f));
				return new Vector2(num, num2);
			}

			public override void Draw(string uniqueID, Vector2 position, Vector2 forcedSize)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0031: Unknown result type (might be due to invalid IL or missing references)
				ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
				windowDrawList.AddRectFilled(position, position + forcedSize, BackgroundColor.ToImGui());
				Vector2 vector = ImGui.CalcTextSize(Text);
				ImFontPtr font = ImGui.GetFont();
				int num = ((font.GetDebugName() == "unifont.otf, 16px") ? 2 : 0);
				Vector2 vector2 = position + new Vector2((float)Math.Round((forcedSize.X - vector.X) / 2f), (float)Math.Round((forcedSize.Y - vector.Y) / 2f) - (float)num);
				windowDrawList.AddText(vector2, TextColor.ToImGui(), Text);
			}
		}

		public class Options
		{
			public System.Drawing.Color BackgroundColor { get; set; } = System.Drawing.Color.Black;

			public System.Drawing.Color BorderColor { get; set; } = System.Drawing.Color.Black;

			public System.Drawing.Color LabelTextColor { get; set; } = Colors.TextonControl;

			public Vector4 Padding { get; set; } = new Vector4(2f, 2f, 2f, 2f);

			public int ButtonSpacing { get; set; } = 1;

			public bool Movable { get; set; } = true;

			public Vector2? ResetPosition { get; set; }

			public ToolbarOrientation Orientation { get; set; }

			public List<Tool> Tools { get; set; } = new List<Tool>();
		}

		private static Dictionary<string, Vector2> _lastPositions = new Dictionary<string, Vector2>();

		public static void Draw(string uniqueID, Options options)
		{
			InternalDraw(uniqueID, options);
		}

		public static void Draw(string uniqueID, List<Tool> tools)
		{
			Options options = new Options
			{
				Tools = tools
			};
			InternalDraw(uniqueID, options);
		}

		private static void InternalDraw(string uniqueID, Options options)
		{
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_022b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(uniqueID))
			{
				throw new ArgumentException("uniqueID cannot be null or empty", "uniqueID");
			}
			if (options == null)
			{
				throw new ArgumentNullException("options", "Options cannot be null");
			}
			List<Vector2> list = new List<Vector2>();
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			for (int i = 0; i < options.Tools.Count; i++)
			{
				Vector2 size = options.Tools[i].GetSize();
				int num5 = (int)size.X;
				int num6 = (int)size.Y;
				list.Add(new Vector2(num5, num6));
				if (options.Orientation == ToolbarOrientation.Horizontal)
				{
					num += (float)num5;
					if (i < options.Tools.Count - 1)
					{
						num += (float)options.ButtonSpacing;
					}
					num4 = Math.Max(num4, num6);
				}
				else
				{
					num2 += (float)num6;
					if (i < options.Tools.Count - 1)
					{
						num2 += (float)options.ButtonSpacing;
					}
					num3 = Math.Max(num3, num5);
				}
			}
			float num7 = options.Padding.W + options.Padding.Y;
			float num8 = options.Padding.X + options.Padding.Z;
			if (options.Orientation == ToolbarOrientation.Horizontal)
			{
				num7 += num;
				num8 += num4;
			}
			else
			{
				num7 += num3;
				num8 += num2;
			}
			if (options.ResetPosition.HasValue)
			{
				ImGui.SetNextWindowPos(options.ResetPosition.Value, (ImGuiCond)1);
				options.ResetPosition = null;
			}
			ImGui.SetNextWindowSize(new Vector2(num7, num8), (ImGuiCond)1);
			ImGuiWindowFlags val = (ImGuiWindowFlags)107;
			if (!options.Movable)
			{
				val = val | (ImGuiWindowFlags)4;
			}
			ImGui.PushStyleVar((ImGuiStyleVar)2, new Vector2(0f, 0f));
			ImGui.PushStyleVar((ImGuiStyleVar)11, new Vector2(0f, 0f));
			ImGui.PushStyleVar((ImGuiStyleVar)4, 0f);
			ImGui.PushStyleVar((ImGuiStyleVar)3, 0f);
			ImGui.PushStyleColor((ImGuiCol)5, options.BorderColor.ToImGui());
			ImGui.PushStyleColor((ImGuiCol)2, System.Drawing.Color.Black.ToImGui());
			ImGui.Begin("##" + uniqueID + "Toolbar", val);
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			Vector2 windowPos = ImGui.GetWindowPos();
			Vector2 windowSize = ImGui.GetWindowSize();
			windowDrawList.AddRectFilled(windowPos, windowPos + windowSize, options.BackgroundColor.ToImGui());
			windowDrawList.AddRect(windowPos, windowPos + windowSize, options.BorderColor.ToImGui());
			Vector2 position = windowPos + new Vector2(options.Padding.W, options.Padding.X);
			for (int j = 0; j < options.Tools.Count; j++)
			{
				Tool tool = options.Tools[j];
				Vector2 size2 = tool.GetSize();
				if (options.Orientation == ToolbarOrientation.Horizontal)
				{
					size2.Y = num4;
				}
				else
				{
					size2.X = num3;
				}
				tool.Draw($"##{uniqueID}_item{j}", position, size2);
				if (options.Orientation == ToolbarOrientation.Horizontal)
				{
					position.X += size2.X + (float)options.ButtonSpacing;
				}
				else
				{
					position.Y += size2.Y + (float)options.ButtonSpacing;
				}
			}
			ImGui.End();
			ImGui.PopStyleColor(2);
			ImGui.PopStyleVar(4);
		}
	}

	public static class IconPicker
	{
		public class Options
		{
			public string Title = "Icon Picker";

			public Vector2 WindowOffset = DefaultWindowOffset;

			public System.Drawing.Color HoveredIconColor = Colors.ButtonHovered;

			public System.Drawing.Color SelectedIconColor = Colors.ButtonChecked;

			public System.Drawing.Color IconColor = System.Drawing.Color.White;
		}

		private static int TitlebarHeight = 20;

		private static readonly DXTPadding PanelPadding = new DXTPadding(3, 0, 3, 3);

		private static readonly Vector2 DefaultWindowOffset = new Vector2(10f, 10f);

		private static Vector2 WindowSize = new Vector2(0f, 0f);

		public static void Open(string uniqueID, IconAtlas iconAtlas, Vector2? windowOffset)
		{
			WindowSize = new Vector2((float)PanelPadding.Left + iconAtlas.AtlasSize.X + (float)PanelPadding.Right, (float)(TitlebarHeight + PanelPadding.Top) + iconAtlas.AtlasSize.Y + (float)PanelPadding.Bottom);
			PopupWindow.Open(uniqueID, windowOffset ?? DefaultWindowOffset);
		}

		public static void Draw(string uniqueID, IconAtlas iconAtlas, ref int selectedIconIndex, Options options = null)
		{
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			if (options == null)
			{
				options = new Options();
			}
			if (!PopupWindow.Begin(uniqueID, new PopupWindow.Options
			{
				Size = WindowSize,
				Title = options.Title,
				PanelPadding = PanelPadding,
				TitleBarHeight = TitlebarHeight
			}))
			{
				return;
			}
			Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			for (int i = 0; i < iconAtlas.IconsPerColumn; i++)
			{
				for (int j = 0; j < iconAtlas.IconsPerRow; j++)
				{
					int num = i * iconAtlas.IconsPerRow + j;
					(Vector2 uv0, Vector2 uv1) iconUVs = iconAtlas.GetIconUVs(num);
					Vector2 item = iconUVs.uv0;
					Vector2 item2 = iconUVs.uv1;
					Vector2 vector = cursorScreenPos + new Vector2((float)j * iconAtlas.IconSize.X, (float)i * iconAtlas.IconSize.Y);
					if (num == selectedIconIndex)
					{
						windowDrawList.AddRectFilled(vector, vector + iconAtlas.IconSize, options.SelectedIconColor.ToImGui());
					}
					ImGui.SetCursorScreenPos(vector);
					if (ImGui.InvisibleButton($"{uniqueID}textureID_{i}_{j}", iconAtlas.IconSize))
					{
						selectedIconIndex = num;
						ImGui.CloseCurrentPopup();
					}
					if (ImGui.IsItemHovered())
					{
						windowDrawList.AddRectFilled(vector, vector + iconAtlas.IconSize, options.HoveredIconColor.ToImGui());
					}
					windowDrawList.AddImage((IntPtr)iconAtlas.TextureId, vector, vector + iconAtlas.IconSize, item, item2, options.IconColor.ToImGui());
				}
			}
			PopupWindow.End();
		}
	}

	public class Menu
	{
		public class Item
		{
			public string Label { get; set; } = "";

			public Action? OnClick { get; set; }

			public List<Item>? SubMenu { get; set; }

			public bool? Checked { get; set; }

			public string? RightLabel { get; set; }

			public bool Enabled { get; set; } = true;

			public bool Separator { get; set; }

			public Tooltip.Options? Tooltip { get; set; }

			public System.Drawing.Color? TextColor { get; set; }

			public System.Drawing.Color? TextCheckedColor { get; set; }

			public System.Drawing.Color? TextRightColor { get; set; }

			public System.Drawing.Color? SeparatorColor { get; set; }
		}

		public class Options
		{
			public Vector2 MenuPadding = new Vector2(8f, 6f);

			public int BorderSize = 1;

			public Vector2 itemSpacing = new Vector2(6f, 4f);

			public System.Drawing.Color BorderColor = Colors.Border;

			public System.Drawing.Color MenuColor = Colors.Menu;

			public System.Drawing.Color TextColor = Colors.Text;

			public System.Drawing.Color TextDisabledColor = Colors.TextDisabled;

			public System.Drawing.Color TextCheckedColor = Colors.TextChecked;

			public System.Drawing.Color SeperatorColor = Colors.Text;

			public System.Drawing.Color RightTextColor = Colors.Text;

			public System.Drawing.Color HoverColor = Color.FromHSLA(205f, 99f, 34f);

			public System.Drawing.Color ActiveColor = Color.FromHSLA(205f, 99f, 40f);
		}

		private static List<Item>? _items;

		private static string _popupId = "";

		private static Options _options;

		public static void Open(string popupId, List<Item> items, Options? options = null)
		{
			_popupId = popupId;
			_items = items;
			_options = options ?? new Options();
			ImGui.OpenPopup(_popupId);
		}

		public static void Draw(string popupId)
		{
			if (!(popupId != _popupId) && _items != null)
			{
				int num = 0;
				int num2 = 0;
				ImGui.PushStyleVar((ImGuiStyleVar)10, (float)_options.BorderSize);
				num++;
				ImGui.PushStyleVar((ImGuiStyleVar)2, _options.MenuPadding);
				num++;
				ImGui.PushStyleVar((ImGuiStyleVar)14, _options.itemSpacing);
				num++;
				ImGui.PushStyleColor((ImGuiCol)5, _options.BorderColor.ToImGui());
				num2++;
				ImGui.PushStyleColor((ImGuiCol)4, _options.MenuColor.ToImGui());
				num2++;
				ImGui.PushStyleColor((ImGuiCol)26, _options.ActiveColor.ToImGui());
				num2++;
				ImGui.PushStyleColor((ImGuiCol)25, _options.HoverColor.ToImGui());
				num2++;
				ImGui.PushStyleColor((ImGuiCol)0, _options.TextColor.ToImGui());
				num2++;
				ImGui.PushStyleColor((ImGuiCol)27, _options.SeperatorColor.ToImGui());
				num2++;
				ImGui.PushStyleColor((ImGuiCol)1, _options.RightTextColor.ToImGui());
				num2++;
				if (ImGui.BeginPopup(popupId))
				{
					DrawItems(_items);
					ImGui.EndPopup();
				}
				if (num > 0)
				{
					ImGui.PopStyleVar(num);
				}
				if (num2 > 0)
				{
					ImGui.PopStyleColor(num2);
				}
			}
		}

		private static void DrawItems(List<Item> items, string idPrefix = "")
		{
			for (int i = 0; i < items.Count; i++)
			{
				Item item = items[i];
				if (item.Separator)
				{
					if (item.SeparatorColor.HasValue)
					{
						ImGui.PushStyleColor((ImGuiCol)27, item.SeparatorColor.Value.ToImGui());
						ImGui.Separator();
						ImGui.PopStyleColor();
					}
					else
					{
						ImGui.Separator();
					}
					continue;
				}
				string text = $"{idPrefix}{i}";
				string text2 = item.Label + "##" + text;
				if (item.SubMenu != null && item.SubMenu.Count > 0)
				{
					if (ImGui.BeginMenu(text2))
					{
						DrawItems(item.SubMenu, idPrefix + i + "_");
						ImGui.EndMenu();
					}
					continue;
				}
				System.Drawing.Color c = ((!item.Checked.HasValue || !item.Checked.Value) ? (item.TextColor ?? _options.TextColor) : (item.TextCheckedColor ?? _options.TextCheckedColor));
				System.Drawing.Color c2 = item.TextRightColor ?? _options.RightTextColor;
				ImGui.PushStyleColor((ImGuiCol)0, c.ToImGui());
				ImGui.PushStyleColor((ImGuiCol)1, c2.ToImGui());
				if (item.Checked.HasValue)
				{
					if (ImGui.MenuItem(text2, item.RightLabel, item.Checked.Value, item.Enabled))
					{
						ImGui.CloseCurrentPopup();
						if (item.OnClick != null)
						{
							Deferred.Enqueue(item.OnClick);
						}
					}
				}
				else if (ImGui.MenuItem(text2, item.RightLabel, false, item.Enabled))
				{
					ImGui.CloseCurrentPopup();
					if (item.OnClick != null)
					{
						Deferred.Enqueue(item.OnClick);
					}
				}
				if (ImGui.IsItemHovered() && item.Enabled && item.Tooltip != null)
				{
					Tooltip.Draw(item.Tooltip);
				}
				ImGui.PopStyleColor(2);
			}
		}
	}

	private static class RecursiveMenuIdea
	{
		public class Item
		{
			public string Label { get; set; } = "";

			public bool Checked { get; set; }

			public int IconIndex { get; set; } = -1;

			public Action? OnClick { get; set; }

			public List<Item>? SubMenu { get; set; }
		}

		public static void DrawMenu(List<Item> items)
		{
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			foreach (Item item in items)
			{
				if (item.SubMenu != null && item.SubMenu.Count > 0)
				{
					if (ImGui.BeginMenu(item.Label))
					{
						DrawMenu(item.SubMenu);
						ImGui.EndMenu();
					}
					continue;
				}
				if (ImGui.MenuItem(item.Label, "", item.Checked))
				{
					item.OnClick?.Invoke();
				}
				ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
				Vector2 itemRectMin = ImGui.GetItemRectMin();
				Vector2 itemRectMax = ImGui.GetItemRectMax();
				if (ImGui.IsItemHovered())
				{
					windowDrawList.AddRectFilled(itemRectMin, itemRectMax, 872415231u);
				}
				if (item.Checked)
				{
					float num = 12f;
					windowDrawList.AddText(new Vector2(itemRectMax.X - num - 2f, itemRectMin.Y + (itemRectMax.Y - itemRectMin.Y - num) / 2f), 4278255360u, "x");
				}
				if (item.IconIndex >= 0)
				{
					float num2 = 16f;
					windowDrawList.AddRectFilled(new Vector2(itemRectMin.X + 2f, itemRectMin.Y + (itemRectMax.Y - itemRectMin.Y - num2) / 2f), new Vector2(itemRectMin.X + 2f + num2, itemRectMin.Y + (itemRectMax.Y - itemRectMin.Y + num2) / 2f), 4294945280u);
				}
			}
		}
	}

	public static class TextInputModal
	{
		public class Options
		{
			public string Title { get; set; } = "Input";

			public string OkLabel { get; set; } = "OK";

			public string CancelLabel { get; set; } = "Cancel";

			public int Width { get; set; } = 300;

			public int buttonWidth { get; set; } = 60;

			public Tooltip.Options? Tooltip { get; set; }
		}

		private class ModalState
		{
			public string InputValue;

			public Options Options;

			public bool ShouldOpen;
		}

		private static readonly int itemSpacing = 3;

		private static readonly int controlHeight = 20;

		private static readonly Dictionary<string, ModalState> _modals = new Dictionary<string, ModalState>();

		public static void Open(string uniquePopupID, string initialValue, Options? options)
		{
			_modals[uniquePopupID] = new ModalState
			{
				InputValue = initialValue,
				Options = (options ?? new Options()),
				ShouldOpen = true
			};
			PopupModalWindow.Open(uniquePopupID);
		}

		public static (bool ok, string? value)? Draw(string uniquePopupID)
		{
			if (!_modals.TryGetValue(uniquePopupID, out ModalState value))
			{
				return null;
			}
			if (value.ShouldOpen)
			{
				ImGui.OpenPopup(uniquePopupID);
				value.ShouldOpen = false;
			}
			if (PopupModalWindow.Begin(uniquePopupID, new PopupModalWindow.Options
			{
				Title = value.Options.Title,
				Height = 105,
				Width = value.Options.Width
			}))
			{
				ImGui.PushStyleVar((ImGuiStyleVar)14, new Vector2(itemSpacing, itemSpacing));
				ImGui.Indent(6f);
				ImGui.Dummy(new Vector2(controlHeight, controlHeight - itemSpacing));
				Input.Draw(uniquePopupID + "Input", ref value.InputValue, new Input.Options
				{
					Width = -6,
					Height = controlHeight,
					Tooltip = (value.Options.Tooltip ?? null)
				});
				ImGui.Dummy(new Vector2(controlHeight, controlHeight - itemSpacing));
				ImGui.Dummy(new Vector2(ImGui.GetContentRegionAvail().X - (float)itemSpacing - (float)value.Options.buttonWidth - (float)itemSpacing - (float)value.Options.buttonWidth - 6f, controlHeight));
				ImGui.SameLine();
				if (Button.Draw(uniquePopupID + "OK", new Button.Options
				{
					Label = value.Options.OkLabel,
					Width = value.Options.buttonWidth,
					Height = controlHeight
				}))
				{
					(bool, string) value2 = (true, value.InputValue.Trim());
					_modals.Remove(uniquePopupID);
					ImGui.CloseCurrentPopup();
					ImGui.Unindent(6f);
					ImGui.PopStyleVar();
					PopupModalWindow.End();
					return value2;
				}
				ImGui.SameLine();
				if (Button.Draw(uniquePopupID + "CANCEL", new Button.Options
				{
					Label = value.Options.CancelLabel,
					Width = value.Options.buttonWidth,
					Height = controlHeight
				}))
				{
					_modals.Remove(uniquePopupID);
					ImGui.CloseCurrentPopup();
					ImGui.Unindent(6f);
					ImGui.PopStyleVar();
					PopupModalWindow.End();
					return (false, null);
				}
				ImGui.Unindent(6f);
				ImGui.PopStyleVar();
				PopupModalWindow.End();
			}
			return null;
		}
	}

	public static class Tooltip
	{
		public class Options
		{
			public List<Line> Lines { get; set; } = new List<Line>();

			public System.Drawing.Color BackgroundColor { get; set; } = Color.FromRGBA(0, 0, 0, 200);

			public System.Drawing.Color BorderColor { get; set; } = System.Drawing.Color.Black;

			public Vector2 Offset { get; set; } = new Vector2(10f, 10f);

			public Vector2 Size { get; set; } = new Vector2(260f, 120f);

			public DXTPadding Padding { get; set; } = new DXTPadding(5, 5, 5, 5);

			public bool FitContent { get; set; } = true;

			public Options()
			{
			}

			public Options(string text)
			{
				int num = 1;
				List<Line> list = new List<Line>(num);
				CollectionsMarshal.SetCount(list, num);
				Span<Line> span = CollectionsMarshal.AsSpan(list);
				int index = 0;
				span[index] = new Description
				{
					Text = text
				};
				Lines = list;
			}

			public Options(string text, System.Drawing.Color color)
			{
				int num = 1;
				List<Line> list = new List<Line>(num);
				CollectionsMarshal.SetCount(list, num);
				Span<Line> span = CollectionsMarshal.AsSpan(list);
				int index = 0;
				span[index] = new Description
				{
					Text = text,
					Color = color
				};
				Lines = list;
			}

			public Options(string title, string description, bool separator = true)
			{
				Lines = new List<Line>
				{
					new Title
					{
						Text = title
					}
				};
				if (separator)
				{
					Lines.Add(new Separator());
				}
				Lines.Add(new Description
				{
					Text = description
				});
			}
		}

		public abstract class Line
		{
		}

		public class Title : Line
		{
			public string Text { get; set; } = string.Empty;

			public System.Drawing.Color Color { get; set; } = DefaultTitleColor;
		}

		public class DoubleLine : Line
		{
			public string LeftText { get; set; } = string.Empty;

			public string RightText { get; set; } = string.Empty;

			public System.Drawing.Color LeftColor { get; set; } = DefaultLeftTextColor;

			public System.Drawing.Color RightColor { get; set; } = DefaultRightTextColor;
		}

		public class Description : Line
		{
			public string Text { get; set; } = string.Empty;

			public System.Drawing.Color Color { get; set; } = DefaultDescriptionColor;
		}

		public class Separator : Line
		{
			public DXTPadding Padding { get; set; } = new DXTPadding(-2, 4, -2, 6);

			public float Thickness { get; set; } = 1f;

			public System.Drawing.Color Color { get; set; } = DefaultSeparatorColor;
		}

		private static readonly System.Drawing.Color DefaultTitleColor = Color.FromRGBA(255, 255, 255);

		private static readonly System.Drawing.Color DefaultDescriptionColor = Color.FromHsla(220f, 0.15f, 0.9f, 255f);

		private static readonly System.Drawing.Color DefaultSeparatorColor = Color.FromHsla(220f, 0.15f, 0.9f, 255f);

		private static readonly System.Drawing.Color DefaultLeftTextColor = Color.FromRGBA(0, 170, 255);

		private static readonly System.Drawing.Color DefaultRightTextColor = Color.FromRGBA(214, 214, 0);

		public static void Draw(string text)
		{
			Draw(new Options(text));
		}

		public static void Draw(string text, System.Drawing.Color color)
		{
			Draw(new Options(text, color));
		}

		public static void Draw(string title, string description, bool separator = true)
		{
			Draw(new Options(title, description, separator));
		}

		public static void Draw(Options options)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			Vector2 mousePos = ImGui.GetMousePos();
			ImDrawListPtr foregroundDrawList = ImGui.GetForegroundDrawList();
			Vector2 vector = mousePos + options.Offset;
			Vector2 vector2 = options.Size;
			if (options.FitContent)
			{
				float num = 0f;
				float num2 = 0f;
				foreach (Line line in options.Lines)
				{
					if (!(line is Title title))
					{
						if (!(line is DoubleLine doubleLine))
						{
							if (!(line is Description description))
							{
								if (line is Separator separator)
								{
									num2 += (float)separator.Padding.Top + separator.Thickness + (float)separator.Padding.Bottom;
								}
								continue;
							}
							string[] array = description.Text.Split('\n');
							float num3 = 0f;
							float num4 = 0f;
							string[] array2 = array;
							for (int i = 0; i < array2.Length; i++)
							{
								Vector2 vector3 = ImGui.CalcTextSize(array2[i]);
								num3 += vector3.Y;
								num4 = Math.Max(num4, vector3.X);
							}
							num = Math.Max(num, num4);
							num2 += num3;
						}
						else
						{
							Vector2 vector4 = ImGui.CalcTextSize(doubleLine.LeftText);
							Vector2 vector5 = ImGui.CalcTextSize(doubleLine.RightText);
							float val = vector4.X + 10f + vector5.X;
							num = Math.Max(num, val);
							num2 += Math.Max(vector4.Y, vector5.Y);
						}
					}
					else
					{
						Vector2 vector6 = ImGui.CalcTextSize(title.Text);
						num = Math.Max(num, vector6.X);
						num2 += vector6.Y;
					}
				}
				vector2 = new Vector2(num + (float)options.Padding.Left + (float)options.Padding.Right, num2 + (float)options.Padding.Top + (float)options.Padding.Bottom);
			}
			foregroundDrawList.AddRectFilled(vector, vector + vector2, options.BackgroundColor.ToImGui());
			foregroundDrawList.AddRect(vector, vector + vector2, options.BorderColor.ToImGui(), 0f, (ImDrawFlags)0, 1f);
			Vector2 vector7 = vector + new Vector2(options.Padding.Left, options.Padding.Top);
			foreach (Line line2 in options.Lines)
			{
				if (!(line2 is Title title2))
				{
					if (!(line2 is DoubleLine doubleLine2))
					{
						if (!(line2 is Description description2))
						{
							if (line2 is Separator separator2)
							{
								vector7.Y += separator2.Padding.Top;
								float y = MathF.Round(vector7.Y);
								Vector2 vector8 = new Vector2(vector.X + (float)options.Padding.Left + (float)separator2.Padding.Left, y);
								Vector2 vector9 = new Vector2(vector.X + vector2.X - (float)options.Padding.Right - (float)separator2.Padding.Right, y);
								foregroundDrawList.AddLine(vector8, vector9, separator2.Color.ToImGui(), separator2.Thickness);
								vector7.Y += separator2.Thickness + (float)separator2.Padding.Bottom;
							}
						}
						else
						{
							string[] array2 = description2.Text.Split('\n');
							foreach (string text in array2)
							{
								foregroundDrawList.AddText(vector7, description2.Color.ToImGui(), text);
								Vector2 vector10 = ImGui.CalcTextSize(text);
								vector7.Y += vector10.Y;
							}
						}
					}
					else
					{
						foregroundDrawList.AddText(vector7, doubleLine2.LeftColor.ToImGui(), doubleLine2.LeftText);
						Vector2 vector11 = ImGui.CalcTextSize(doubleLine2.LeftText);
						Vector2 vector12 = ImGui.CalcTextSize(doubleLine2.RightText);
						Vector2 vector13 = new Vector2(vector.X + vector2.X - (float)options.Padding.Right - vector12.X, vector7.Y);
						foregroundDrawList.AddText(vector13, doubleLine2.RightColor.ToImGui(), doubleLine2.RightText);
						vector7.Y += Math.Max(vector11.Y, vector12.Y);
					}
				}
				else
				{
					foregroundDrawList.AddText(vector7, title2.Color.ToImGui(), title2.Text);
					Vector2 vector14 = ImGui.CalcTextSize(title2.Text);
					vector7.Y += vector14.Y;
				}
			}
		}
	}

	public readonly struct ColorSegment : IEquatable<ColorSegment>
	{
		public string Text { get; }

		public System.Drawing.Color Color { get; }

		public ColorSegment(string text, System.Drawing.Color color)
		{
			Text = text;
			Color = color;
		}

		public bool Equals(ColorSegment other)
		{
			if (Text == other.Text)
			{
				return Color.Equals(other.Color);
			}
			return false;
		}

		public override bool Equals(object? obj)
		{
			if (obj is ColorSegment other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Text, Color);
		}
	}

	public class ColoredText : IRenderableText
	{
		private float? _width;

		private float? _height;

		private Vector2? _size;

		public IReadOnlyList<ColorSegment> Segments { get; }

		public float Width
		{
			get
			{
				if (!_width.HasValue)
				{
					float num = 0f;
					foreach (ColorSegment segment in Segments)
					{
						num += ImGui.CalcTextSize(segment.Text).X;
					}
					_width = num;
				}
				return _width.Value;
			}
		}

		public float Height
		{
			get
			{
				float valueOrDefault = _height.GetValueOrDefault();
				if (!_height.HasValue)
				{
					valueOrDefault = ImGui.CalcTextSize("A").Y;
					_height = valueOrDefault;
				}
				return _height.Value;
			}
		}

		public Vector2 Size
		{
			get
			{
				if (!_size.HasValue)
				{
					_size = new Vector2(Width, Height);
				}
				return _size.Value;
			}
		}

		public int Count => Segments.Count;

		public ColoredText(string input)
		{
			if (input == null)
			{
				input = string.Empty;
			}
			List<ColorSegment> list = ParseSegments(input);
			Segments = list.AsReadOnly();
		}

		public ColoredText(IEnumerable<ColorSegment> segments)
		{
			Segments = segments.ToList().AsReadOnly();
		}

		public string ToUncoloredString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ColorSegment segment in Segments)
			{
				stringBuilder.Append(segment.Text);
			}
			return stringBuilder.ToString();
		}

		public override string ToString()
		{
			return ToUncoloredString();
		}

		public void Draw(ImDrawListPtr drawList, Vector2 pos, System.Drawing.Color? defaultColor = null)
		{
			System.Drawing.Color valueOrDefault = defaultColor.GetValueOrDefault();
			if (!defaultColor.HasValue)
			{
				valueOrDefault = System.Drawing.Color.White;
				defaultColor = valueOrDefault;
			}
			float num = pos.X;
			foreach (ColorSegment segment in Segments)
			{
				System.Drawing.Color c = (segment.Color.IsEmpty ? defaultColor.Value : segment.Color);
				drawList.AddText(new Vector2(num, pos.Y), c.ToImGui(), segment.Text);
				num += ImGui.CalcTextSize(segment.Text).X;
			}
		}

		public IRenderableText Ellipsize(float maxWidth, string ellipsis = "...")
		{
			List<ColorSegment> list = new List<ColorSegment>();
			float num = 0f;
			float x = ImGui.CalcTextSize(ellipsis).X;
			foreach (ColorSegment segment in Segments)
			{
				float x2 = ImGui.CalcTextSize(segment.Text).X;
				if (num + x2 > maxWidth)
				{
					int num2 = 0;
					float num3 = 0f;
					string text = segment.Text;
					for (int i = 0; i < text.Length; i++)
					{
						float x3 = ImGui.CalcTextSize(text[i].ToString()).X;
						if (num + num3 + x3 + x > maxWidth)
						{
							break;
						}
						num3 += x3;
						num2++;
					}
					if (num2 > 0)
					{
						list.Add(new ColorSegment(segment.Text.Substring(0, num2) + ellipsis, segment.Color));
					}
					else if (list.Count > 0)
					{
						list[list.Count - 1] = new ColorSegment(list.Last().Text + ellipsis, list.Last().Color);
					}
					else
					{
						list.Add(new ColorSegment(ellipsis, segment.Color));
					}
					break;
				}
				list.Add(segment);
				num += x2;
			}
			if (list.Count == Segments.Count && list.SequenceEqual(Segments))
			{
				return this;
			}
			return new ColoredText(list);
		}

		private static List<ColorSegment> ParseSegments(string input)
		{
			List<ColorSegment> list = new List<ColorSegment>();
			Stack<System.Drawing.Color> stack = new Stack<System.Drawing.Color>();
			stack.Push(System.Drawing.Color.Empty);
			int num = 0;
			int num2 = 0;
			while (num < input.Length)
			{
				if (input[num] == '|' && num + 9 < input.Length && input[num + 1] == 'c')
				{
					if (num > num2)
					{
						int num3 = num2;
						list.Add(new ColorSegment(input.Substring(num3, num - num3), stack.Peek()));
					}
					string text = input.Substring(num + 2, 8);
					byte red = Convert.ToByte(text.Substring(0, 2), 16);
					byte green = Convert.ToByte(text.Substring(2, 2), 16);
					byte blue = Convert.ToByte(text.Substring(4, 2), 16);
					byte alpha = Convert.ToByte(text.Substring(6, 2), 16);
					stack.Push(System.Drawing.Color.FromArgb(alpha, red, green, blue));
					num += 10;
					num2 = num;
				}
				else if (input[num] == '|' && num + 1 < input.Length && input[num + 1] == 'r')
				{
					if (num > num2)
					{
						int num3 = num2;
						list.Add(new ColorSegment(input.Substring(num3, num - num3), stack.Peek()));
					}
					if (stack.Count > 1)
					{
						stack.Pop();
					}
					num += 2;
					num2 = num;
				}
				else
				{
					num++;
				}
			}
			if (num > num2)
			{
				int num3 = num2;
				list.Add(new ColorSegment(input.Substring(num3, num - num3), stack.Peek()));
			}
			return list;
		}
	}

	public class PlainText : IRenderableText
	{
		private float? _width;

		private float? _height;

		private Vector2? _size;

		public string Text { get; }

		public float Width
		{
			get
			{
				if (!_width.HasValue)
				{
					float valueOrDefault = _width.GetValueOrDefault();
					if (!_width.HasValue)
					{
						valueOrDefault = ImGui.CalcTextSize(Text).X;
						_width = valueOrDefault;
					}
					return _width.Value;
				}
				return _width.Value;
			}
		}

		public float Height
		{
			get
			{
				float valueOrDefault = _height.GetValueOrDefault();
				if (!_height.HasValue)
				{
					valueOrDefault = ImGui.CalcTextSize("A").Y;
					_height = valueOrDefault;
				}
				return _height.Value;
			}
		}

		public Vector2 Size
		{
			get
			{
				if (!_size.HasValue)
				{
					_size = new Vector2(Width, Height);
				}
				return _size.Value;
			}
		}

		public PlainText(string text)
		{
            Text = text ?? string.Empty;
		}

		public void Draw(ImDrawListPtr drawList, Vector2 pos, System.Drawing.Color? color = null)
		{
			System.Drawing.Color valueOrDefault = color.GetValueOrDefault();
			if (!color.HasValue)
			{
				valueOrDefault = System.Drawing.Color.White;
				color = valueOrDefault;
			}
			drawList.AddText(pos, color.Value.ToImGui(), Text);
		}

		public override string ToString()
		{
			return Text;
		}

		public IRenderableText Ellipsize(float maxWidth, string ellipsis = "...")
		{
			if (Width <= maxWidth)
			{
				return this;
			}
			float x = ImGui.CalcTextSize(ellipsis).X;
			float num = 0f;
			int num2 = 0;
			string text = Text;
			for (int i = 0; i < text.Length; i++)
			{
				float x2 = ImGui.CalcTextSize(text[i].ToString()).X;
				if (num + x2 + x > maxWidth)
				{
					break;
				}
				num += x2;
				num2++;
			}
			return new PlainText((num2 < Text.Length) ? (Text.Substring(0, num2) + ellipsis) : Text);
		}
	}

	public enum ClickType
	{
		None,
		Left,
		Right,
		Middle,
		Other
	}

	public enum TextAlign
	{
		Left,
		Center,
		Right
	}

	public interface IRenderableText
	{
		float Width { get; }

		float Height { get; }

		Vector2 Size { get; }

		void Draw(ImDrawListPtr drawList, Vector2 pos, System.Drawing.Color? color = null);

		IRenderableText Ellipsize(float maxWidth, string ellipsis = "...");
	}

	public class PaletteSwatch
	{
		public string Name { get; }

		public System.Drawing.Color Color { get; }

		public uint Uint { get; }

		public Vector4 Vec4 { get; }

		public PaletteSwatch(string name, System.Drawing.Color color)
		{
			Name = name;
			Color = color;
			Uint = color.ToImGui();
			Vec4 = color.ToVector4();
		}
	}

	public static class Palettes
	{
		public static class Tailwind
		{
			public static readonly PaletteSwatch Red50 = new PaletteSwatch("Red 50", System.Drawing.Color.FromArgb(254, 242, 242));

			public static readonly PaletteSwatch Red100 = new PaletteSwatch("Red 100", System.Drawing.Color.FromArgb(254, 226, 226));

			public static readonly PaletteSwatch Red200 = new PaletteSwatch("Red 200", System.Drawing.Color.FromArgb(254, 202, 202));

			public static readonly PaletteSwatch Red300 = new PaletteSwatch("Red 300", System.Drawing.Color.FromArgb(252, 165, 165));

			public static readonly PaletteSwatch Red400 = new PaletteSwatch("Red 400", System.Drawing.Color.FromArgb(248, 113, 113));

			public static readonly PaletteSwatch Red500 = new PaletteSwatch("Red 500", System.Drawing.Color.FromArgb(239, 68, 68));

			public static readonly PaletteSwatch Red600 = new PaletteSwatch("Red 600", System.Drawing.Color.FromArgb(220, 38, 38));

			public static readonly PaletteSwatch Red700 = new PaletteSwatch("Red 700", System.Drawing.Color.FromArgb(185, 28, 28));

			public static readonly PaletteSwatch Red800 = new PaletteSwatch("Red 800", System.Drawing.Color.FromArgb(153, 27, 27));

			public static readonly PaletteSwatch Red900 = new PaletteSwatch("Red 900", System.Drawing.Color.FromArgb(127, 29, 29));

			public static readonly PaletteSwatch Red950 = new PaletteSwatch("Red 950", System.Drawing.Color.FromArgb(69, 10, 10));

			public static readonly PaletteSwatch Orange50 = new PaletteSwatch("Orange 50", System.Drawing.Color.FromArgb(255, 247, 237));

			public static readonly PaletteSwatch Orange100 = new PaletteSwatch("Orange 100", System.Drawing.Color.FromArgb(255, 237, 213));

			public static readonly PaletteSwatch Orange200 = new PaletteSwatch("Orange 200", System.Drawing.Color.FromArgb(254, 215, 170));

			public static readonly PaletteSwatch Orange300 = new PaletteSwatch("Orange 300", System.Drawing.Color.FromArgb(253, 186, 116));

			public static readonly PaletteSwatch Orange400 = new PaletteSwatch("Orange 400", System.Drawing.Color.FromArgb(251, 146, 60));

			public static readonly PaletteSwatch Orange500 = new PaletteSwatch("Orange 500", System.Drawing.Color.FromArgb(249, 115, 22));

			public static readonly PaletteSwatch Orange600 = new PaletteSwatch("Orange 600", System.Drawing.Color.FromArgb(234, 88, 12));

			public static readonly PaletteSwatch Orange700 = new PaletteSwatch("Orange 700", System.Drawing.Color.FromArgb(194, 65, 12));

			public static readonly PaletteSwatch Orange800 = new PaletteSwatch("Orange 800", System.Drawing.Color.FromArgb(154, 52, 18));

			public static readonly PaletteSwatch Orange900 = new PaletteSwatch("Orange 900", System.Drawing.Color.FromArgb(124, 45, 18));

			public static readonly PaletteSwatch Orange950 = new PaletteSwatch("Orange 950", System.Drawing.Color.FromArgb(67, 20, 7));

			public static readonly PaletteSwatch Amber50 = new PaletteSwatch("Amber 50", System.Drawing.Color.FromArgb(255, 251, 235));

			public static readonly PaletteSwatch Amber100 = new PaletteSwatch("Amber 100", System.Drawing.Color.FromArgb(254, 243, 199));

			public static readonly PaletteSwatch Amber200 = new PaletteSwatch("Amber 200", System.Drawing.Color.FromArgb(253, 230, 138));

			public static readonly PaletteSwatch Amber300 = new PaletteSwatch("Amber 300", System.Drawing.Color.FromArgb(252, 211, 77));

			public static readonly PaletteSwatch Amber400 = new PaletteSwatch("Amber 400", System.Drawing.Color.FromArgb(251, 191, 36));

			public static readonly PaletteSwatch Amber500 = new PaletteSwatch("Amber 500", System.Drawing.Color.FromArgb(245, 158, 11));

			public static readonly PaletteSwatch Amber600 = new PaletteSwatch("Amber 600", System.Drawing.Color.FromArgb(217, 119, 6));

			public static readonly PaletteSwatch Amber700 = new PaletteSwatch("Amber 700", System.Drawing.Color.FromArgb(180, 83, 9));

			public static readonly PaletteSwatch Amber800 = new PaletteSwatch("Amber 800", System.Drawing.Color.FromArgb(146, 64, 14));

			public static readonly PaletteSwatch Amber900 = new PaletteSwatch("Amber 900", System.Drawing.Color.FromArgb(120, 53, 15));

			public static readonly PaletteSwatch Amber950 = new PaletteSwatch("Amber 950", System.Drawing.Color.FromArgb(69, 26, 3));

			public static readonly PaletteSwatch Yellow50 = new PaletteSwatch("Yellow 50", System.Drawing.Color.FromArgb(254, 252, 232));

			public static readonly PaletteSwatch Yellow100 = new PaletteSwatch("Yellow 100", System.Drawing.Color.FromArgb(254, 249, 195));

			public static readonly PaletteSwatch Yellow200 = new PaletteSwatch("Yellow 200", System.Drawing.Color.FromArgb(254, 240, 138));

			public static readonly PaletteSwatch Yellow300 = new PaletteSwatch("Yellow 300", System.Drawing.Color.FromArgb(253, 224, 71));

			public static readonly PaletteSwatch Yellow400 = new PaletteSwatch("Yellow 400", System.Drawing.Color.FromArgb(250, 204, 21));

			public static readonly PaletteSwatch Yellow500 = new PaletteSwatch("Yellow 500", System.Drawing.Color.FromArgb(234, 179, 8));

			public static readonly PaletteSwatch Yellow600 = new PaletteSwatch("Yellow 600", System.Drawing.Color.FromArgb(202, 138, 4));

			public static readonly PaletteSwatch Yellow700 = new PaletteSwatch("Yellow 700", System.Drawing.Color.FromArgb(161, 98, 7));

			public static readonly PaletteSwatch Yellow800 = new PaletteSwatch("Yellow 800", System.Drawing.Color.FromArgb(133, 77, 14));

			public static readonly PaletteSwatch Yellow900 = new PaletteSwatch("Yellow 900", System.Drawing.Color.FromArgb(113, 63, 18));

			public static readonly PaletteSwatch Yellow950 = new PaletteSwatch("Yellow 950", System.Drawing.Color.FromArgb(66, 32, 6));

			public static readonly PaletteSwatch Lime50 = new PaletteSwatch("Lime 50", System.Drawing.Color.FromArgb(247, 254, 231));

			public static readonly PaletteSwatch Lime100 = new PaletteSwatch("Lime 100", System.Drawing.Color.FromArgb(236, 252, 203));

			public static readonly PaletteSwatch Lime200 = new PaletteSwatch("Lime 200", System.Drawing.Color.FromArgb(217, 249, 157));

			public static readonly PaletteSwatch Lime300 = new PaletteSwatch("Lime 300", System.Drawing.Color.FromArgb(190, 242, 100));

			public static readonly PaletteSwatch Lime400 = new PaletteSwatch("Lime 400", System.Drawing.Color.FromArgb(163, 230, 53));

			public static readonly PaletteSwatch Lime500 = new PaletteSwatch("Lime 500", System.Drawing.Color.FromArgb(132, 204, 22));

			public static readonly PaletteSwatch Lime600 = new PaletteSwatch("Lime 600", System.Drawing.Color.FromArgb(101, 163, 13));

			public static readonly PaletteSwatch Lime700 = new PaletteSwatch("Lime 700", System.Drawing.Color.FromArgb(77, 124, 15));

			public static readonly PaletteSwatch Lime800 = new PaletteSwatch("Lime 800", System.Drawing.Color.FromArgb(63, 98, 18));

			public static readonly PaletteSwatch Lime900 = new PaletteSwatch("Lime 900", System.Drawing.Color.FromArgb(54, 83, 20));

			public static readonly PaletteSwatch Lime950 = new PaletteSwatch("Lime 950", System.Drawing.Color.FromArgb(26, 46, 5));

			public static readonly PaletteSwatch Green50 = new PaletteSwatch("Green 50", System.Drawing.Color.FromArgb(240, 253, 244));

			public static readonly PaletteSwatch Green100 = new PaletteSwatch("Green 100", System.Drawing.Color.FromArgb(220, 252, 231));

			public static readonly PaletteSwatch Green200 = new PaletteSwatch("Green 200", System.Drawing.Color.FromArgb(187, 247, 208));

			public static readonly PaletteSwatch Green300 = new PaletteSwatch("Green 300", System.Drawing.Color.FromArgb(134, 239, 172));

			public static readonly PaletteSwatch Green400 = new PaletteSwatch("Green 400", System.Drawing.Color.FromArgb(74, 222, 128));

			public static readonly PaletteSwatch Green500 = new PaletteSwatch("Green 500", System.Drawing.Color.FromArgb(34, 197, 94));

			public static readonly PaletteSwatch Green600 = new PaletteSwatch("Green 600", System.Drawing.Color.FromArgb(22, 163, 74));

			public static readonly PaletteSwatch Green700 = new PaletteSwatch("Green 700", System.Drawing.Color.FromArgb(21, 128, 61));

			public static readonly PaletteSwatch Green800 = new PaletteSwatch("Green 800", System.Drawing.Color.FromArgb(22, 101, 52));

			public static readonly PaletteSwatch Green900 = new PaletteSwatch("Green 900", System.Drawing.Color.FromArgb(20, 83, 45));

			public static readonly PaletteSwatch Green950 = new PaletteSwatch("Green 950", System.Drawing.Color.FromArgb(5, 46, 22));

			public static readonly PaletteSwatch Emerald50 = new PaletteSwatch("Emerald 50", System.Drawing.Color.FromArgb(236, 253, 245));

			public static readonly PaletteSwatch Emerald100 = new PaletteSwatch("Emerald 100", System.Drawing.Color.FromArgb(209, 250, 229));

			public static readonly PaletteSwatch Emerald200 = new PaletteSwatch("Emerald 200", System.Drawing.Color.FromArgb(167, 243, 208));

			public static readonly PaletteSwatch Emerald300 = new PaletteSwatch("Emerald 300", System.Drawing.Color.FromArgb(110, 231, 183));

			public static readonly PaletteSwatch Emerald400 = new PaletteSwatch("Emerald 400", System.Drawing.Color.FromArgb(52, 211, 153));

			public static readonly PaletteSwatch Emerald500 = new PaletteSwatch("Emerald 500", System.Drawing.Color.FromArgb(16, 185, 129));

			public static readonly PaletteSwatch Emerald600 = new PaletteSwatch("Emerald 600", System.Drawing.Color.FromArgb(5, 150, 105));

			public static readonly PaletteSwatch Emerald700 = new PaletteSwatch("Emerald 700", System.Drawing.Color.FromArgb(4, 120, 87));

			public static readonly PaletteSwatch Emerald800 = new PaletteSwatch("Emerald 800", System.Drawing.Color.FromArgb(6, 95, 70));

			public static readonly PaletteSwatch Emerald900 = new PaletteSwatch("Emerald 900", System.Drawing.Color.FromArgb(6, 78, 59));

			public static readonly PaletteSwatch Emerald950 = new PaletteSwatch("Emerald 950", System.Drawing.Color.FromArgb(2, 44, 34));

			public static readonly PaletteSwatch Teal50 = new PaletteSwatch("Teal 50", System.Drawing.Color.FromArgb(240, 253, 250));

			public static readonly PaletteSwatch Teal100 = new PaletteSwatch("Teal 100", System.Drawing.Color.FromArgb(204, 251, 241));

			public static readonly PaletteSwatch Teal200 = new PaletteSwatch("Teal 200", System.Drawing.Color.FromArgb(153, 246, 228));

			public static readonly PaletteSwatch Teal300 = new PaletteSwatch("Teal 300", System.Drawing.Color.FromArgb(94, 234, 212));

			public static readonly PaletteSwatch Teal400 = new PaletteSwatch("Teal 400", System.Drawing.Color.FromArgb(45, 212, 191));

			public static readonly PaletteSwatch Teal500 = new PaletteSwatch("Teal 500", System.Drawing.Color.FromArgb(20, 184, 166));

			public static readonly PaletteSwatch Teal600 = new PaletteSwatch("Teal 600", System.Drawing.Color.FromArgb(13, 148, 136));

			public static readonly PaletteSwatch Teal700 = new PaletteSwatch("Teal 700", System.Drawing.Color.FromArgb(15, 118, 110));

			public static readonly PaletteSwatch Teal800 = new PaletteSwatch("Teal 800", System.Drawing.Color.FromArgb(17, 94, 89));

			public static readonly PaletteSwatch Teal900 = new PaletteSwatch("Teal 900", System.Drawing.Color.FromArgb(19, 78, 74));

			public static readonly PaletteSwatch Teal950 = new PaletteSwatch("Teal 950", System.Drawing.Color.FromArgb(4, 47, 46));

			public static readonly PaletteSwatch Cyan50 = new PaletteSwatch("Cyan 50", System.Drawing.Color.FromArgb(236, 254, 255));

			public static readonly PaletteSwatch Cyan100 = new PaletteSwatch("Cyan 100", System.Drawing.Color.FromArgb(207, 250, 254));

			public static readonly PaletteSwatch Cyan200 = new PaletteSwatch("Cyan 200", System.Drawing.Color.FromArgb(165, 243, 252));

			public static readonly PaletteSwatch Cyan300 = new PaletteSwatch("Cyan 300", System.Drawing.Color.FromArgb(103, 232, 249));

			public static readonly PaletteSwatch Cyan400 = new PaletteSwatch("Cyan 400", System.Drawing.Color.FromArgb(34, 211, 238));

			public static readonly PaletteSwatch Cyan500 = new PaletteSwatch("Cyan 500", System.Drawing.Color.FromArgb(6, 182, 212));

			public static readonly PaletteSwatch Cyan600 = new PaletteSwatch("Cyan 600", System.Drawing.Color.FromArgb(8, 145, 178));

			public static readonly PaletteSwatch Cyan700 = new PaletteSwatch("Cyan 700", System.Drawing.Color.FromArgb(14, 116, 144));

			public static readonly PaletteSwatch Cyan800 = new PaletteSwatch("Cyan 800", System.Drawing.Color.FromArgb(21, 94, 117));

			public static readonly PaletteSwatch Cyan900 = new PaletteSwatch("Cyan 900", System.Drawing.Color.FromArgb(22, 78, 99));

			public static readonly PaletteSwatch Cyan950 = new PaletteSwatch("Cyan 950", System.Drawing.Color.FromArgb(8, 51, 68));

			public static readonly PaletteSwatch Sky50 = new PaletteSwatch("Sky 50", System.Drawing.Color.FromArgb(240, 249, 255));

			public static readonly PaletteSwatch Sky100 = new PaletteSwatch("Sky 100", System.Drawing.Color.FromArgb(224, 242, 254));

			public static readonly PaletteSwatch Sky200 = new PaletteSwatch("Sky 200", System.Drawing.Color.FromArgb(186, 230, 253));

			public static readonly PaletteSwatch Sky300 = new PaletteSwatch("Sky 300", System.Drawing.Color.FromArgb(125, 211, 252));

			public static readonly PaletteSwatch Sky400 = new PaletteSwatch("Sky 400", System.Drawing.Color.FromArgb(56, 189, 248));

			public static readonly PaletteSwatch Sky500 = new PaletteSwatch("Sky 500", System.Drawing.Color.FromArgb(14, 165, 233));

			public static readonly PaletteSwatch Sky600 = new PaletteSwatch("Sky 600", System.Drawing.Color.FromArgb(2, 132, 199));

			public static readonly PaletteSwatch Sky700 = new PaletteSwatch("Sky 700", System.Drawing.Color.FromArgb(3, 105, 161));

			public static readonly PaletteSwatch Sky800 = new PaletteSwatch("Sky 800", System.Drawing.Color.FromArgb(7, 89, 133));

			public static readonly PaletteSwatch Sky900 = new PaletteSwatch("Sky 900", System.Drawing.Color.FromArgb(12, 74, 110));

			public static readonly PaletteSwatch Sky950 = new PaletteSwatch("Sky 950", System.Drawing.Color.FromArgb(8, 47, 73));

			public static readonly PaletteSwatch Blue50 = new PaletteSwatch("Blue 50", System.Drawing.Color.FromArgb(239, 246, 255));

			public static readonly PaletteSwatch Blue100 = new PaletteSwatch("Blue 100", System.Drawing.Color.FromArgb(219, 234, 254));

			public static readonly PaletteSwatch Blue200 = new PaletteSwatch("Blue 200", System.Drawing.Color.FromArgb(191, 219, 254));

			public static readonly PaletteSwatch Blue300 = new PaletteSwatch("Blue 300", System.Drawing.Color.FromArgb(147, 197, 253));

			public static readonly PaletteSwatch Blue400 = new PaletteSwatch("Blue 400", System.Drawing.Color.FromArgb(96, 165, 250));

			public static readonly PaletteSwatch Blue500 = new PaletteSwatch("Blue 500", System.Drawing.Color.FromArgb(59, 130, 246));

			public static readonly PaletteSwatch Blue600 = new PaletteSwatch("Blue 600", System.Drawing.Color.FromArgb(37, 99, 235));

			public static readonly PaletteSwatch Blue700 = new PaletteSwatch("Blue 700", System.Drawing.Color.FromArgb(29, 78, 216));

			public static readonly PaletteSwatch Blue800 = new PaletteSwatch("Blue 800", System.Drawing.Color.FromArgb(30, 64, 175));

			public static readonly PaletteSwatch Blue900 = new PaletteSwatch("Blue 900", System.Drawing.Color.FromArgb(30, 58, 138));

			public static readonly PaletteSwatch Blue950 = new PaletteSwatch("Blue 950", System.Drawing.Color.FromArgb(23, 37, 84));

			public static readonly PaletteSwatch Indigo50 = new PaletteSwatch("Indigo 50", System.Drawing.Color.FromArgb(238, 242, 255));

			public static readonly PaletteSwatch Indigo100 = new PaletteSwatch("Indigo 100", System.Drawing.Color.FromArgb(224, 231, 255));

			public static readonly PaletteSwatch Indigo200 = new PaletteSwatch("Indigo 200", System.Drawing.Color.FromArgb(199, 210, 254));

			public static readonly PaletteSwatch Indigo300 = new PaletteSwatch("Indigo 300", System.Drawing.Color.FromArgb(165, 180, 252));

			public static readonly PaletteSwatch Indigo400 = new PaletteSwatch("Indigo 400", System.Drawing.Color.FromArgb(129, 140, 248));

			public static readonly PaletteSwatch Indigo500 = new PaletteSwatch("Indigo 500", System.Drawing.Color.FromArgb(99, 102, 241));

			public static readonly PaletteSwatch Indigo600 = new PaletteSwatch("Indigo 600", System.Drawing.Color.FromArgb(79, 70, 229));

			public static readonly PaletteSwatch Indigo700 = new PaletteSwatch("Indigo 700", System.Drawing.Color.FromArgb(67, 56, 202));

			public static readonly PaletteSwatch Indigo800 = new PaletteSwatch("Indigo 800", System.Drawing.Color.FromArgb(55, 48, 163));

			public static readonly PaletteSwatch Indigo900 = new PaletteSwatch("Indigo 900", System.Drawing.Color.FromArgb(49, 46, 129));

			public static readonly PaletteSwatch Indigo950 = new PaletteSwatch("Indigo 950", System.Drawing.Color.FromArgb(30, 27, 75));

			public static readonly PaletteSwatch Violet50 = new PaletteSwatch("Violet 50", System.Drawing.Color.FromArgb(245, 243, 255));

			public static readonly PaletteSwatch Violet100 = new PaletteSwatch("Violet 100", System.Drawing.Color.FromArgb(237, 233, 254));

			public static readonly PaletteSwatch Violet200 = new PaletteSwatch("Violet 200", System.Drawing.Color.FromArgb(221, 214, 254));

			public static readonly PaletteSwatch Violet300 = new PaletteSwatch("Violet 300", System.Drawing.Color.FromArgb(196, 181, 253));

			public static readonly PaletteSwatch Violet400 = new PaletteSwatch("Violet 400", System.Drawing.Color.FromArgb(167, 139, 250));

			public static readonly PaletteSwatch Violet500 = new PaletteSwatch("Violet 500", System.Drawing.Color.FromArgb(139, 92, 246));

			public static readonly PaletteSwatch Violet600 = new PaletteSwatch("Violet 600", System.Drawing.Color.FromArgb(124, 58, 237));

			public static readonly PaletteSwatch Violet700 = new PaletteSwatch("Violet 700", System.Drawing.Color.FromArgb(109, 40, 217));

			public static readonly PaletteSwatch Violet800 = new PaletteSwatch("Violet 800", System.Drawing.Color.FromArgb(91, 33, 182));

			public static readonly PaletteSwatch Violet900 = new PaletteSwatch("Violet 900", System.Drawing.Color.FromArgb(76, 29, 149));

			public static readonly PaletteSwatch Violet950 = new PaletteSwatch("Violet 950", System.Drawing.Color.FromArgb(46, 16, 101));

			public static readonly PaletteSwatch Purple50 = new PaletteSwatch("Purple 50", System.Drawing.Color.FromArgb(250, 245, 255));

			public static readonly PaletteSwatch Purple100 = new PaletteSwatch("Purple 100", System.Drawing.Color.FromArgb(243, 232, 255));

			public static readonly PaletteSwatch Purple200 = new PaletteSwatch("Purple 200", System.Drawing.Color.FromArgb(233, 213, 255));

			public static readonly PaletteSwatch Purple300 = new PaletteSwatch("Purple 300", System.Drawing.Color.FromArgb(216, 180, 254));

			public static readonly PaletteSwatch Purple400 = new PaletteSwatch("Purple 400", System.Drawing.Color.FromArgb(192, 132, 252));

			public static readonly PaletteSwatch Purple500 = new PaletteSwatch("Purple 500", System.Drawing.Color.FromArgb(168, 85, 247));

			public static readonly PaletteSwatch Purple600 = new PaletteSwatch("Purple 600", System.Drawing.Color.FromArgb(147, 51, 234));

			public static readonly PaletteSwatch Purple700 = new PaletteSwatch("Purple 700", System.Drawing.Color.FromArgb(126, 34, 206));

			public static readonly PaletteSwatch Purple800 = new PaletteSwatch("Purple 800", System.Drawing.Color.FromArgb(107, 33, 168));

			public static readonly PaletteSwatch Purple900 = new PaletteSwatch("Purple 900", System.Drawing.Color.FromArgb(88, 28, 135));

			public static readonly PaletteSwatch Purple950 = new PaletteSwatch("Purple 950", System.Drawing.Color.FromArgb(59, 7, 100));

			public static readonly PaletteSwatch Fuchsia50 = new PaletteSwatch("Fuchsia 50", System.Drawing.Color.FromArgb(253, 244, 255));

			public static readonly PaletteSwatch Fuchsia100 = new PaletteSwatch("Fuchsia 100", System.Drawing.Color.FromArgb(250, 232, 255));

			public static readonly PaletteSwatch Fuchsia200 = new PaletteSwatch("Fuchsia 200", System.Drawing.Color.FromArgb(245, 208, 254));

			public static readonly PaletteSwatch Fuchsia300 = new PaletteSwatch("Fuchsia 300", System.Drawing.Color.FromArgb(240, 171, 252));

			public static readonly PaletteSwatch Fuchsia400 = new PaletteSwatch("Fuchsia 400", System.Drawing.Color.FromArgb(232, 121, 249));

			public static readonly PaletteSwatch Fuchsia500 = new PaletteSwatch("Fuchsia 500", System.Drawing.Color.FromArgb(217, 70, 239));

			public static readonly PaletteSwatch Fuchsia600 = new PaletteSwatch("Fuchsia 600", System.Drawing.Color.FromArgb(192, 38, 211));

			public static readonly PaletteSwatch Fuchsia700 = new PaletteSwatch("Fuchsia 700", System.Drawing.Color.FromArgb(162, 28, 175));

			public static readonly PaletteSwatch Fuchsia800 = new PaletteSwatch("Fuchsia 800", System.Drawing.Color.FromArgb(134, 25, 143));

			public static readonly PaletteSwatch Fuchsia900 = new PaletteSwatch("Fuchsia 900", System.Drawing.Color.FromArgb(112, 26, 117));

			public static readonly PaletteSwatch Fuchsia950 = new PaletteSwatch("Fuchsia 950", System.Drawing.Color.FromArgb(74, 4, 78));

			public static readonly PaletteSwatch Pink50 = new PaletteSwatch("Pink 50", System.Drawing.Color.FromArgb(253, 242, 248));

			public static readonly PaletteSwatch Pink100 = new PaletteSwatch("Pink 100", System.Drawing.Color.FromArgb(252, 231, 243));

			public static readonly PaletteSwatch Pink200 = new PaletteSwatch("Pink 200", System.Drawing.Color.FromArgb(251, 207, 232));

			public static readonly PaletteSwatch Pink300 = new PaletteSwatch("Pink 300", System.Drawing.Color.FromArgb(249, 168, 212));

			public static readonly PaletteSwatch Pink400 = new PaletteSwatch("Pink 400", System.Drawing.Color.FromArgb(244, 114, 182));

			public static readonly PaletteSwatch Pink500 = new PaletteSwatch("Pink 500", System.Drawing.Color.FromArgb(236, 72, 153));

			public static readonly PaletteSwatch Pink600 = new PaletteSwatch("Pink 600", System.Drawing.Color.FromArgb(219, 39, 119));

			public static readonly PaletteSwatch Pink700 = new PaletteSwatch("Pink 700", System.Drawing.Color.FromArgb(190, 24, 93));

			public static readonly PaletteSwatch Pink800 = new PaletteSwatch("Pink 800", System.Drawing.Color.FromArgb(157, 23, 77));

			public static readonly PaletteSwatch Pink900 = new PaletteSwatch("Pink 900", System.Drawing.Color.FromArgb(131, 24, 67));

			public static readonly PaletteSwatch Pink950 = new PaletteSwatch("Pink 950", System.Drawing.Color.FromArgb(80, 7, 36));

			public static readonly PaletteSwatch Rose50 = new PaletteSwatch("Rose 50", System.Drawing.Color.FromArgb(255, 241, 242));

			public static readonly PaletteSwatch Rose100 = new PaletteSwatch("Rose 100", System.Drawing.Color.FromArgb(255, 228, 230));

			public static readonly PaletteSwatch Rose200 = new PaletteSwatch("Rose 200", System.Drawing.Color.FromArgb(254, 205, 211));

			public static readonly PaletteSwatch Rose300 = new PaletteSwatch("Rose 300", System.Drawing.Color.FromArgb(253, 164, 175));

			public static readonly PaletteSwatch Rose400 = new PaletteSwatch("Rose 400", System.Drawing.Color.FromArgb(251, 113, 133));

			public static readonly PaletteSwatch Rose500 = new PaletteSwatch("Rose 500", System.Drawing.Color.FromArgb(244, 63, 94));

			public static readonly PaletteSwatch Rose600 = new PaletteSwatch("Rose 600", System.Drawing.Color.FromArgb(225, 29, 72));

			public static readonly PaletteSwatch Rose700 = new PaletteSwatch("Rose 700", System.Drawing.Color.FromArgb(190, 18, 60));

			public static readonly PaletteSwatch Rose800 = new PaletteSwatch("Rose 800", System.Drawing.Color.FromArgb(159, 18, 57));

			public static readonly PaletteSwatch Rose900 = new PaletteSwatch("Rose 900", System.Drawing.Color.FromArgb(136, 19, 55));

			public static readonly PaletteSwatch Rose950 = new PaletteSwatch("Rose 950", System.Drawing.Color.FromArgb(76, 5, 25));

			public static readonly PaletteSwatch Slate50 = new PaletteSwatch("Slate 50", System.Drawing.Color.FromArgb(248, 250, 252));

			public static readonly PaletteSwatch Slate100 = new PaletteSwatch("Slate 100", System.Drawing.Color.FromArgb(241, 245, 249));

			public static readonly PaletteSwatch Slate200 = new PaletteSwatch("Slate 200", System.Drawing.Color.FromArgb(226, 232, 240));

			public static readonly PaletteSwatch Slate300 = new PaletteSwatch("Slate 300", System.Drawing.Color.FromArgb(203, 213, 225));

			public static readonly PaletteSwatch Slate400 = new PaletteSwatch("Slate 400", System.Drawing.Color.FromArgb(148, 163, 184));

			public static readonly PaletteSwatch Slate500 = new PaletteSwatch("Slate 500", System.Drawing.Color.FromArgb(100, 116, 139));

			public static readonly PaletteSwatch Slate600 = new PaletteSwatch("Slate 600", System.Drawing.Color.FromArgb(71, 85, 105));

			public static readonly PaletteSwatch Slate700 = new PaletteSwatch("Slate 700", System.Drawing.Color.FromArgb(51, 65, 85));

			public static readonly PaletteSwatch Slate800 = new PaletteSwatch("Slate 800", System.Drawing.Color.FromArgb(30, 41, 59));

			public static readonly PaletteSwatch Slate900 = new PaletteSwatch("Slate 900", System.Drawing.Color.FromArgb(15, 23, 42));

			public static readonly PaletteSwatch Slate950 = new PaletteSwatch("Slate 950", System.Drawing.Color.FromArgb(2, 6, 23));

			public static readonly PaletteSwatch Gray50 = new PaletteSwatch("Gray 50", System.Drawing.Color.FromArgb(249, 250, 251));

			public static readonly PaletteSwatch Gray100 = new PaletteSwatch("Gray 100", System.Drawing.Color.FromArgb(243, 244, 246));

			public static readonly PaletteSwatch Gray200 = new PaletteSwatch("Gray 200", System.Drawing.Color.FromArgb(229, 231, 235));

			public static readonly PaletteSwatch Gray300 = new PaletteSwatch("Gray 300", System.Drawing.Color.FromArgb(209, 213, 219));

			public static readonly PaletteSwatch Gray400 = new PaletteSwatch("Gray 400", System.Drawing.Color.FromArgb(156, 163, 175));

			public static readonly PaletteSwatch Gray500 = new PaletteSwatch("Gray 500", System.Drawing.Color.FromArgb(107, 114, 128));

			public static readonly PaletteSwatch Gray600 = new PaletteSwatch("Gray 600", System.Drawing.Color.FromArgb(75, 85, 99));

			public static readonly PaletteSwatch Gray700 = new PaletteSwatch("Gray 700", System.Drawing.Color.FromArgb(55, 65, 81));

			public static readonly PaletteSwatch Gray800 = new PaletteSwatch("Gray 800", System.Drawing.Color.FromArgb(31, 41, 55));

			public static readonly PaletteSwatch Gray900 = new PaletteSwatch("Gray 900", System.Drawing.Color.FromArgb(17, 24, 39));

			public static readonly PaletteSwatch Gray950 = new PaletteSwatch("Gray 950", System.Drawing.Color.FromArgb(3, 7, 18));

			public static readonly PaletteSwatch Zinc50 = new PaletteSwatch("Zinc 50", System.Drawing.Color.FromArgb(250, 250, 250));

			public static readonly PaletteSwatch Zinc100 = new PaletteSwatch("Zinc 100", System.Drawing.Color.FromArgb(244, 244, 245));

			public static readonly PaletteSwatch Zinc200 = new PaletteSwatch("Zinc 200", System.Drawing.Color.FromArgb(228, 228, 231));

			public static readonly PaletteSwatch Zinc300 = new PaletteSwatch("Zinc 300", System.Drawing.Color.FromArgb(212, 212, 216));

			public static readonly PaletteSwatch Zinc400 = new PaletteSwatch("Zinc 400", System.Drawing.Color.FromArgb(161, 161, 170));

			public static readonly PaletteSwatch Zinc500 = new PaletteSwatch("Zinc 500", System.Drawing.Color.FromArgb(113, 113, 122));

			public static readonly PaletteSwatch Zinc600 = new PaletteSwatch("Zinc 600", System.Drawing.Color.FromArgb(82, 82, 91));

			public static readonly PaletteSwatch Zinc700 = new PaletteSwatch("Zinc 700", System.Drawing.Color.FromArgb(63, 63, 70));

			public static readonly PaletteSwatch Zinc800 = new PaletteSwatch("Zinc 800", System.Drawing.Color.FromArgb(39, 39, 42));

			public static readonly PaletteSwatch Zinc900 = new PaletteSwatch("Zinc 900", System.Drawing.Color.FromArgb(24, 24, 27));

			public static readonly PaletteSwatch Zinc950 = new PaletteSwatch("Zinc 950", System.Drawing.Color.FromArgb(9, 9, 11));

			public static readonly PaletteSwatch Neutral50 = new PaletteSwatch("Neutral 50", System.Drawing.Color.FromArgb(250, 250, 250));

			public static readonly PaletteSwatch Neutral100 = new PaletteSwatch("Neutral 100", System.Drawing.Color.FromArgb(245, 245, 245));

			public static readonly PaletteSwatch Neutral200 = new PaletteSwatch("Neutral 200", System.Drawing.Color.FromArgb(229, 229, 229));

			public static readonly PaletteSwatch Neutral300 = new PaletteSwatch("Neutral 300", System.Drawing.Color.FromArgb(212, 212, 212));

			public static readonly PaletteSwatch Neutral400 = new PaletteSwatch("Neutral 400", System.Drawing.Color.FromArgb(163, 163, 163));

			public static readonly PaletteSwatch Neutral500 = new PaletteSwatch("Neutral 500", System.Drawing.Color.FromArgb(115, 115, 115));

			public static readonly PaletteSwatch Neutral600 = new PaletteSwatch("Neutral 600", System.Drawing.Color.FromArgb(82, 82, 82));

			public static readonly PaletteSwatch Neutral700 = new PaletteSwatch("Neutral 700", System.Drawing.Color.FromArgb(64, 64, 64));

			public static readonly PaletteSwatch Neutral800 = new PaletteSwatch("Neutral 800", System.Drawing.Color.FromArgb(38, 38, 38));

			public static readonly PaletteSwatch Neutral900 = new PaletteSwatch("Neutral 900", System.Drawing.Color.FromArgb(23, 23, 23));

			public static readonly PaletteSwatch Neutral950 = new PaletteSwatch("Neutral 950", System.Drawing.Color.FromArgb(10, 10, 10));

			public static readonly PaletteSwatch Stone50 = new PaletteSwatch("Stone 50", System.Drawing.Color.FromArgb(250, 250, 249));

			public static readonly PaletteSwatch Stone100 = new PaletteSwatch("Stone 100", System.Drawing.Color.FromArgb(245, 245, 244));

			public static readonly PaletteSwatch Stone200 = new PaletteSwatch("Stone 200", System.Drawing.Color.FromArgb(231, 229, 228));

			public static readonly PaletteSwatch Stone300 = new PaletteSwatch("Stone 300", System.Drawing.Color.FromArgb(214, 211, 209));

			public static readonly PaletteSwatch Stone400 = new PaletteSwatch("Stone 400", System.Drawing.Color.FromArgb(168, 162, 158));

			public static readonly PaletteSwatch Stone500 = new PaletteSwatch("Stone 500", System.Drawing.Color.FromArgb(120, 113, 108));

			public static readonly PaletteSwatch Stone600 = new PaletteSwatch("Stone 600", System.Drawing.Color.FromArgb(87, 83, 78));

			public static readonly PaletteSwatch Stone700 = new PaletteSwatch("Stone 700", System.Drawing.Color.FromArgb(68, 64, 60));

			public static readonly PaletteSwatch Stone800 = new PaletteSwatch("Stone 800", System.Drawing.Color.FromArgb(41, 37, 36));

			public static readonly PaletteSwatch Stone900 = new PaletteSwatch("Stone 900", System.Drawing.Color.FromArgb(28, 25, 23));

			public static readonly PaletteSwatch Stone950 = new PaletteSwatch("Stone 950", System.Drawing.Color.FromArgb(12, 10, 9));

			public static readonly Dictionary<string, List<PaletteSwatch>> All = new Dictionary<string, List<PaletteSwatch>>
			{
				{
					"Red",
					new List<PaletteSwatch>
					{
						Red50, Red100, Red200, Red300, Red400, Red500, Red600, Red700, Red800, Red900,
						Red950
					}
				},
				{
					"Orange",
					new List<PaletteSwatch>
					{
						Orange50, Orange100, Orange200, Orange300, Orange400, Orange500, Orange600, Orange700, Orange800, Orange900,
						Orange950
					}
				},
				{
					"Amber",
					new List<PaletteSwatch>
					{
						Amber50, Amber100, Amber200, Amber300, Amber400, Amber500, Amber600, Amber700, Amber800, Amber900,
						Amber950
					}
				},
				{
					"Yellow",
					new List<PaletteSwatch>
					{
						Yellow50, Yellow100, Yellow200, Yellow300, Yellow400, Yellow500, Yellow600, Yellow700, Yellow800, Yellow900,
						Yellow950
					}
				},
				{
					"Lime",
					new List<PaletteSwatch>
					{
						Lime50, Lime100, Lime200, Lime300, Lime400, Lime500, Lime600, Lime700, Lime800, Lime900,
						Lime950
					}
				},
				{
					"Green",
					new List<PaletteSwatch>
					{
						Green50, Green100, Green200, Green300, Green400, Green500, Green600, Green700, Green800, Green900,
						Green950
					}
				},
				{
					"Emerald",
					new List<PaletteSwatch>
					{
						Emerald50, Emerald100, Emerald200, Emerald300, Emerald400, Emerald500, Emerald600, Emerald700, Emerald800, Emerald900,
						Emerald950
					}
				},
				{
					"Teal",
					new List<PaletteSwatch>
					{
						Teal50, Teal100, Teal200, Teal300, Teal400, Teal500, Teal600, Teal700, Teal800, Teal900,
						Teal950
					}
				},
				{
					"Cyan",
					new List<PaletteSwatch>
					{
						Cyan50, Cyan100, Cyan200, Cyan300, Cyan400, Cyan500, Cyan600, Cyan700, Cyan800, Cyan900,
						Cyan950
					}
				},
				{
					"Sky",
					new List<PaletteSwatch>
					{
						Sky50, Sky100, Sky200, Sky300, Sky400, Sky500, Sky600, Sky700, Sky800, Sky900,
						Sky950
					}
				},
				{
					"Blue",
					new List<PaletteSwatch>
					{
						Blue50, Blue100, Blue200, Blue300, Blue400, Blue500, Blue600, Blue700, Blue800, Blue900,
						Blue950
					}
				},
				{
					"Indigo",
					new List<PaletteSwatch>
					{
						Indigo50, Indigo100, Indigo200, Indigo300, Indigo400, Indigo500, Indigo600, Indigo700, Indigo800, Indigo900,
						Indigo950
					}
				},
				{
					"Violet",
					new List<PaletteSwatch>
					{
						Violet50, Violet100, Violet200, Violet300, Violet400, Violet500, Violet600, Violet700, Violet800, Violet900,
						Violet950
					}
				},
				{
					"Purple",
					new List<PaletteSwatch>
					{
						Purple50, Purple100, Purple200, Purple300, Purple400, Purple500, Purple600, Purple700, Purple800, Purple900,
						Purple950
					}
				},
				{
					"Fuchsia",
					new List<PaletteSwatch>
					{
						Fuchsia50, Fuchsia100, Fuchsia200, Fuchsia300, Fuchsia400, Fuchsia500, Fuchsia600, Fuchsia700, Fuchsia800, Fuchsia900,
						Fuchsia950
					}
				},
				{
					"Pink",
					new List<PaletteSwatch>
					{
						Pink50, Pink100, Pink200, Pink300, Pink400, Pink500, Pink600, Pink700, Pink800, Pink900,
						Pink950
					}
				},
				{
					"Rose",
					new List<PaletteSwatch>
					{
						Rose50, Rose100, Rose200, Rose300, Rose400, Rose500, Rose600, Rose700, Rose800, Rose900,
						Rose950
					}
				},
				{
					"Slate",
					new List<PaletteSwatch>
					{
						Slate50, Slate100, Slate200, Slate300, Slate400, Slate500, Slate600, Slate700, Slate800, Slate900,
						Slate950
					}
				},
				{
					"Gray",
					new List<PaletteSwatch>
					{
						Gray50, Gray100, Gray200, Gray300, Gray400, Gray500, Gray600, Gray700, Gray800, Gray900,
						Gray950
					}
				},
				{
					"Zinc",
					new List<PaletteSwatch>
					{
						Zinc50, Zinc100, Zinc200, Zinc300, Zinc400, Zinc500, Zinc600, Zinc700, Zinc800, Zinc900,
						Zinc950
					}
				},
				{
					"Neutral",
					new List<PaletteSwatch>
					{
						Neutral50, Neutral100, Neutral200, Neutral300, Neutral400, Neutral500, Neutral600, Neutral700, Neutral800, Neutral900,
						Neutral950
					}
				},
				{
					"Stone",
					new List<PaletteSwatch>
					{
						Stone50, Stone100, Stone200, Stone300, Stone400, Stone500, Stone600, Stone700, Stone800, Stone900,
						Stone950
					}
				}
			};
		}

		public static class Material
		{
			public static readonly PaletteSwatch Red50 = new PaletteSwatch("Red 50", System.Drawing.Color.FromArgb(255, 235, 238));

			public static readonly PaletteSwatch Red100 = new PaletteSwatch("Red 100", System.Drawing.Color.FromArgb(255, 205, 210));

			public static readonly PaletteSwatch Red200 = new PaletteSwatch("Red 200", System.Drawing.Color.FromArgb(239, 154, 154));

			public static readonly PaletteSwatch Red300 = new PaletteSwatch("Red 300", System.Drawing.Color.FromArgb(229, 115, 115));

			public static readonly PaletteSwatch Red400 = new PaletteSwatch("Red 400", System.Drawing.Color.FromArgb(239, 83, 80));

			public static readonly PaletteSwatch Red500 = new PaletteSwatch("Red 500", System.Drawing.Color.FromArgb(244, 67, 54));

			public static readonly PaletteSwatch Red600 = new PaletteSwatch("Red 600", System.Drawing.Color.FromArgb(229, 57, 53));

			public static readonly PaletteSwatch Red700 = new PaletteSwatch("Red 700", System.Drawing.Color.FromArgb(211, 47, 47));

			public static readonly PaletteSwatch Red800 = new PaletteSwatch("Red 800", System.Drawing.Color.FromArgb(198, 40, 40));

			public static readonly PaletteSwatch Red900 = new PaletteSwatch("Red 900", System.Drawing.Color.FromArgb(183, 28, 28));

			public static readonly PaletteSwatch RedA100 = new PaletteSwatch("Red A100", System.Drawing.Color.FromArgb(255, 138, 128));

			public static readonly PaletteSwatch RedA200 = new PaletteSwatch("Red A200", System.Drawing.Color.FromArgb(255, 82, 82));

			public static readonly PaletteSwatch RedA400 = new PaletteSwatch("Red A400", System.Drawing.Color.FromArgb(255, 23, 68));

			public static readonly PaletteSwatch RedA700 = new PaletteSwatch("Red A700", System.Drawing.Color.FromArgb(213, 0, 0));

			public static readonly PaletteSwatch Pink50 = new PaletteSwatch("Pink 50", System.Drawing.Color.FromArgb(252, 228, 236));

			public static readonly PaletteSwatch Pink100 = new PaletteSwatch("Pink 100", System.Drawing.Color.FromArgb(248, 187, 208));

			public static readonly PaletteSwatch Pink200 = new PaletteSwatch("Pink 200", System.Drawing.Color.FromArgb(244, 143, 177));

			public static readonly PaletteSwatch Pink300 = new PaletteSwatch("Pink 300", System.Drawing.Color.FromArgb(240, 98, 146));

			public static readonly PaletteSwatch Pink400 = new PaletteSwatch("Pink 400", System.Drawing.Color.FromArgb(236, 64, 122));

			public static readonly PaletteSwatch Pink500 = new PaletteSwatch("Pink 500", System.Drawing.Color.FromArgb(233, 30, 99));

			public static readonly PaletteSwatch Pink600 = new PaletteSwatch("Pink 600", System.Drawing.Color.FromArgb(216, 27, 96));

			public static readonly PaletteSwatch Pink700 = new PaletteSwatch("Pink 700", System.Drawing.Color.FromArgb(194, 24, 91));

			public static readonly PaletteSwatch Pink800 = new PaletteSwatch("Pink 800", System.Drawing.Color.FromArgb(173, 20, 87));

			public static readonly PaletteSwatch Pink900 = new PaletteSwatch("Pink 900", System.Drawing.Color.FromArgb(136, 14, 79));

			public static readonly PaletteSwatch PinkA100 = new PaletteSwatch("Pink A100", System.Drawing.Color.FromArgb(255, 128, 171));

			public static readonly PaletteSwatch PinkA200 = new PaletteSwatch("Pink A200", System.Drawing.Color.FromArgb(255, 64, 129));

			public static readonly PaletteSwatch PinkA400 = new PaletteSwatch("Pink A400", System.Drawing.Color.FromArgb(245, 0, 87));

			public static readonly PaletteSwatch PinkA700 = new PaletteSwatch("Pink A700", System.Drawing.Color.FromArgb(197, 17, 98));

			public static readonly PaletteSwatch Purple50 = new PaletteSwatch("Purple 50", System.Drawing.Color.FromArgb(243, 229, 245));

			public static readonly PaletteSwatch Purple100 = new PaletteSwatch("Purple 100", System.Drawing.Color.FromArgb(225, 190, 231));

			public static readonly PaletteSwatch Purple200 = new PaletteSwatch("Purple 200", System.Drawing.Color.FromArgb(206, 147, 216));

			public static readonly PaletteSwatch Purple300 = new PaletteSwatch("Purple 300", System.Drawing.Color.FromArgb(186, 104, 200));

			public static readonly PaletteSwatch Purple400 = new PaletteSwatch("Purple 400", System.Drawing.Color.FromArgb(171, 71, 188));

			public static readonly PaletteSwatch Purple500 = new PaletteSwatch("Purple 500", System.Drawing.Color.FromArgb(156, 39, 176));

			public static readonly PaletteSwatch Purple600 = new PaletteSwatch("Purple 600", System.Drawing.Color.FromArgb(142, 36, 170));

			public static readonly PaletteSwatch Purple700 = new PaletteSwatch("Purple 700", System.Drawing.Color.FromArgb(123, 31, 162));

			public static readonly PaletteSwatch Purple800 = new PaletteSwatch("Purple 800", System.Drawing.Color.FromArgb(106, 27, 154));

			public static readonly PaletteSwatch Purple900 = new PaletteSwatch("Purple 900", System.Drawing.Color.FromArgb(74, 20, 140));

			public static readonly PaletteSwatch PurpleA100 = new PaletteSwatch("Purple A100", System.Drawing.Color.FromArgb(234, 128, 252));

			public static readonly PaletteSwatch PurpleA200 = new PaletteSwatch("Purple A200", System.Drawing.Color.FromArgb(224, 64, 251));

			public static readonly PaletteSwatch PurpleA400 = new PaletteSwatch("Purple A400", System.Drawing.Color.FromArgb(213, 0, 249));

			public static readonly PaletteSwatch PurpleA700 = new PaletteSwatch("Purple A700", System.Drawing.Color.FromArgb(170, 0, 255));

			public static readonly PaletteSwatch DeepPurple50 = new PaletteSwatch("Deep Purple 50", System.Drawing.Color.FromArgb(237, 231, 246));

			public static readonly PaletteSwatch DeepPurple100 = new PaletteSwatch("Deep Purple 100", System.Drawing.Color.FromArgb(209, 196, 233));

			public static readonly PaletteSwatch DeepPurple200 = new PaletteSwatch("Deep Purple 200", System.Drawing.Color.FromArgb(179, 157, 219));

			public static readonly PaletteSwatch DeepPurple300 = new PaletteSwatch("Deep Purple 300", System.Drawing.Color.FromArgb(149, 117, 205));

			public static readonly PaletteSwatch DeepPurple400 = new PaletteSwatch("Deep Purple 400", System.Drawing.Color.FromArgb(126, 87, 194));

			public static readonly PaletteSwatch DeepPurple500 = new PaletteSwatch("Deep Purple 500", System.Drawing.Color.FromArgb(103, 58, 183));

			public static readonly PaletteSwatch DeepPurple600 = new PaletteSwatch("Deep Purple 600", System.Drawing.Color.FromArgb(94, 53, 177));

			public static readonly PaletteSwatch DeepPurple700 = new PaletteSwatch("Deep Purple 700", System.Drawing.Color.FromArgb(81, 45, 168));

			public static readonly PaletteSwatch DeepPurple800 = new PaletteSwatch("Deep Purple 800", System.Drawing.Color.FromArgb(69, 39, 160));

			public static readonly PaletteSwatch DeepPurple900 = new PaletteSwatch("Deep Purple 900", System.Drawing.Color.FromArgb(49, 27, 146));

			public static readonly PaletteSwatch DeepPurpleA100 = new PaletteSwatch("Deep Purple A100", System.Drawing.Color.FromArgb(179, 136, 255));

			public static readonly PaletteSwatch DeepPurpleA200 = new PaletteSwatch("Deep Purple A200", System.Drawing.Color.FromArgb(124, 77, 255));

			public static readonly PaletteSwatch DeepPurpleA400 = new PaletteSwatch("Deep Purple A400", System.Drawing.Color.FromArgb(101, 31, 255));

			public static readonly PaletteSwatch DeepPurpleA700 = new PaletteSwatch("Deep Purple A700", System.Drawing.Color.FromArgb(98, 0, 234));

			public static readonly PaletteSwatch Indigo50 = new PaletteSwatch("Indigo 50", System.Drawing.Color.FromArgb(232, 234, 246));

			public static readonly PaletteSwatch Indigo100 = new PaletteSwatch("Indigo 100", System.Drawing.Color.FromArgb(197, 202, 233));

			public static readonly PaletteSwatch Indigo200 = new PaletteSwatch("Indigo 200", System.Drawing.Color.FromArgb(159, 168, 218));

			public static readonly PaletteSwatch Indigo300 = new PaletteSwatch("Indigo 300", System.Drawing.Color.FromArgb(121, 134, 203));

			public static readonly PaletteSwatch Indigo400 = new PaletteSwatch("Indigo 400", System.Drawing.Color.FromArgb(92, 107, 192));

			public static readonly PaletteSwatch Indigo500 = new PaletteSwatch("Indigo 500", System.Drawing.Color.FromArgb(63, 81, 181));

			public static readonly PaletteSwatch Indigo600 = new PaletteSwatch("Indigo 600", System.Drawing.Color.FromArgb(57, 73, 171));

			public static readonly PaletteSwatch Indigo700 = new PaletteSwatch("Indigo 700", System.Drawing.Color.FromArgb(48, 63, 159));

			public static readonly PaletteSwatch Indigo800 = new PaletteSwatch("Indigo 800", System.Drawing.Color.FromArgb(40, 53, 147));

			public static readonly PaletteSwatch Indigo900 = new PaletteSwatch("Indigo 900", System.Drawing.Color.FromArgb(26, 35, 126));

			public static readonly PaletteSwatch IndigoA100 = new PaletteSwatch("Indigo A100", System.Drawing.Color.FromArgb(140, 158, 255));

			public static readonly PaletteSwatch IndigoA200 = new PaletteSwatch("Indigo A200", System.Drawing.Color.FromArgb(83, 109, 254));

			public static readonly PaletteSwatch IndigoA400 = new PaletteSwatch("Indigo A400", System.Drawing.Color.FromArgb(61, 90, 254));

			public static readonly PaletteSwatch IndigoA700 = new PaletteSwatch("Indigo A700", System.Drawing.Color.FromArgb(48, 79, 254));

			public static readonly PaletteSwatch Blue50 = new PaletteSwatch("Blue 50", System.Drawing.Color.FromArgb(227, 242, 253));

			public static readonly PaletteSwatch Blue100 = new PaletteSwatch("Blue 100", System.Drawing.Color.FromArgb(187, 222, 251));

			public static readonly PaletteSwatch Blue200 = new PaletteSwatch("Blue 200", System.Drawing.Color.FromArgb(144, 202, 249));

			public static readonly PaletteSwatch Blue300 = new PaletteSwatch("Blue 300", System.Drawing.Color.FromArgb(100, 181, 246));

			public static readonly PaletteSwatch Blue400 = new PaletteSwatch("Blue 400", System.Drawing.Color.FromArgb(66, 165, 245));

			public static readonly PaletteSwatch Blue500 = new PaletteSwatch("Blue 500", System.Drawing.Color.FromArgb(33, 150, 243));

			public static readonly PaletteSwatch Blue600 = new PaletteSwatch("Blue 600", System.Drawing.Color.FromArgb(30, 136, 229));

			public static readonly PaletteSwatch Blue700 = new PaletteSwatch("Blue 700", System.Drawing.Color.FromArgb(25, 118, 210));

			public static readonly PaletteSwatch Blue800 = new PaletteSwatch("Blue 800", System.Drawing.Color.FromArgb(21, 101, 192));

			public static readonly PaletteSwatch Blue900 = new PaletteSwatch("Blue 900", System.Drawing.Color.FromArgb(13, 71, 161));

			public static readonly PaletteSwatch BlueA100 = new PaletteSwatch("Blue A100", System.Drawing.Color.FromArgb(130, 177, 255));

			public static readonly PaletteSwatch BlueA200 = new PaletteSwatch("Blue A200", System.Drawing.Color.FromArgb(68, 138, 255));

			public static readonly PaletteSwatch BlueA400 = new PaletteSwatch("Blue A400", System.Drawing.Color.FromArgb(41, 121, 255));

			public static readonly PaletteSwatch BlueA700 = new PaletteSwatch("Blue A700", System.Drawing.Color.FromArgb(41, 98, 255));

			public static readonly PaletteSwatch LightBlue50 = new PaletteSwatch("Light Blue 50", System.Drawing.Color.FromArgb(225, 245, 254));

			public static readonly PaletteSwatch LightBlue100 = new PaletteSwatch("Light Blue 100", System.Drawing.Color.FromArgb(179, 229, 252));

			public static readonly PaletteSwatch LightBlue200 = new PaletteSwatch("Light Blue 200", System.Drawing.Color.FromArgb(129, 212, 250));

			public static readonly PaletteSwatch LightBlue300 = new PaletteSwatch("Light Blue 300", System.Drawing.Color.FromArgb(79, 195, 247));

			public static readonly PaletteSwatch LightBlue400 = new PaletteSwatch("Light Blue 400", System.Drawing.Color.FromArgb(41, 182, 246));

			public static readonly PaletteSwatch LightBlue500 = new PaletteSwatch("Light Blue 500", System.Drawing.Color.FromArgb(3, 169, 244));

			public static readonly PaletteSwatch LightBlue600 = new PaletteSwatch("Light Blue 600", System.Drawing.Color.FromArgb(3, 155, 229));

			public static readonly PaletteSwatch LightBlue700 = new PaletteSwatch("Light Blue 700", System.Drawing.Color.FromArgb(2, 136, 209));

			public static readonly PaletteSwatch LightBlue800 = new PaletteSwatch("Light Blue 800", System.Drawing.Color.FromArgb(2, 119, 189));

			public static readonly PaletteSwatch LightBlue900 = new PaletteSwatch("Light Blue 900", System.Drawing.Color.FromArgb(1, 87, 155));

			public static readonly PaletteSwatch LightBlueA100 = new PaletteSwatch("Light Blue A100", System.Drawing.Color.FromArgb(128, 216, 255));

			public static readonly PaletteSwatch LightBlueA200 = new PaletteSwatch("Light Blue A200", System.Drawing.Color.FromArgb(64, 196, 255));

			public static readonly PaletteSwatch LightBlueA400 = new PaletteSwatch("Light Blue A400", System.Drawing.Color.FromArgb(0, 176, 255));

			public static readonly PaletteSwatch LightBlueA700 = new PaletteSwatch("Light Blue A700", System.Drawing.Color.FromArgb(0, 145, 234));

			public static readonly PaletteSwatch Cyan50 = new PaletteSwatch("Cyan 50", System.Drawing.Color.FromArgb(224, 247, 250));

			public static readonly PaletteSwatch Cyan100 = new PaletteSwatch("Cyan 100", System.Drawing.Color.FromArgb(178, 235, 242));

			public static readonly PaletteSwatch Cyan200 = new PaletteSwatch("Cyan 200", System.Drawing.Color.FromArgb(128, 222, 234));

			public static readonly PaletteSwatch Cyan300 = new PaletteSwatch("Cyan 300", System.Drawing.Color.FromArgb(77, 208, 225));

			public static readonly PaletteSwatch Cyan400 = new PaletteSwatch("Cyan 400", System.Drawing.Color.FromArgb(38, 198, 218));

			public static readonly PaletteSwatch Cyan500 = new PaletteSwatch("Cyan 500", System.Drawing.Color.FromArgb(0, 188, 212));

			public static readonly PaletteSwatch Cyan600 = new PaletteSwatch("Cyan 600", System.Drawing.Color.FromArgb(0, 172, 193));

			public static readonly PaletteSwatch Cyan700 = new PaletteSwatch("Cyan 700", System.Drawing.Color.FromArgb(0, 151, 167));

			public static readonly PaletteSwatch Cyan800 = new PaletteSwatch("Cyan 800", System.Drawing.Color.FromArgb(0, 131, 143));

			public static readonly PaletteSwatch Cyan900 = new PaletteSwatch("Cyan 900", System.Drawing.Color.FromArgb(0, 96, 100));

			public static readonly PaletteSwatch CyanA100 = new PaletteSwatch("Cyan A100", System.Drawing.Color.FromArgb(132, 255, 255));

			public static readonly PaletteSwatch CyanA200 = new PaletteSwatch("Cyan A200", System.Drawing.Color.FromArgb(24, 255, 255));

			public static readonly PaletteSwatch CyanA400 = new PaletteSwatch("Cyan A400", System.Drawing.Color.FromArgb(0, 229, 255));

			public static readonly PaletteSwatch CyanA700 = new PaletteSwatch("Cyan A700", System.Drawing.Color.FromArgb(0, 184, 212));

			public static readonly PaletteSwatch Teal50 = new PaletteSwatch("Teal 50", System.Drawing.Color.FromArgb(224, 242, 241));

			public static readonly PaletteSwatch Teal100 = new PaletteSwatch("Teal 100", System.Drawing.Color.FromArgb(178, 223, 219));

			public static readonly PaletteSwatch Teal200 = new PaletteSwatch("Teal 200", System.Drawing.Color.FromArgb(128, 203, 196));

			public static readonly PaletteSwatch Teal300 = new PaletteSwatch("Teal 300", System.Drawing.Color.FromArgb(77, 182, 172));

			public static readonly PaletteSwatch Teal400 = new PaletteSwatch("Teal 400", System.Drawing.Color.FromArgb(38, 166, 154));

			public static readonly PaletteSwatch Teal500 = new PaletteSwatch("Teal 500", System.Drawing.Color.FromArgb(0, 150, 136));

			public static readonly PaletteSwatch Teal600 = new PaletteSwatch("Teal 600", System.Drawing.Color.FromArgb(0, 137, 123));

			public static readonly PaletteSwatch Teal700 = new PaletteSwatch("Teal 700", System.Drawing.Color.FromArgb(0, 121, 107));

			public static readonly PaletteSwatch Teal800 = new PaletteSwatch("Teal 800", System.Drawing.Color.FromArgb(0, 105, 92));

			public static readonly PaletteSwatch Teal900 = new PaletteSwatch("Teal 900", System.Drawing.Color.FromArgb(0, 77, 64));

			public static readonly PaletteSwatch TealA100 = new PaletteSwatch("Teal A100", System.Drawing.Color.FromArgb(167, 255, 235));

			public static readonly PaletteSwatch TealA200 = new PaletteSwatch("Teal A200", System.Drawing.Color.FromArgb(100, 255, 218));

			public static readonly PaletteSwatch TealA400 = new PaletteSwatch("Teal A400", System.Drawing.Color.FromArgb(29, 233, 182));

			public static readonly PaletteSwatch TealA700 = new PaletteSwatch("Teal A700", System.Drawing.Color.FromArgb(0, 191, 165));

			public static readonly PaletteSwatch Green50 = new PaletteSwatch("Green 50", System.Drawing.Color.FromArgb(232, 245, 233));

			public static readonly PaletteSwatch Green100 = new PaletteSwatch("Green 100", System.Drawing.Color.FromArgb(200, 230, 201));

			public static readonly PaletteSwatch Green200 = new PaletteSwatch("Green 200", System.Drawing.Color.FromArgb(165, 214, 167));

			public static readonly PaletteSwatch Green300 = new PaletteSwatch("Green 300", System.Drawing.Color.FromArgb(129, 199, 132));

			public static readonly PaletteSwatch Green400 = new PaletteSwatch("Green 400", System.Drawing.Color.FromArgb(102, 187, 106));

			public static readonly PaletteSwatch Green500 = new PaletteSwatch("Green 500", System.Drawing.Color.FromArgb(76, 175, 80));

			public static readonly PaletteSwatch Green600 = new PaletteSwatch("Green 600", System.Drawing.Color.FromArgb(67, 160, 71));

			public static readonly PaletteSwatch Green700 = new PaletteSwatch("Green 700", System.Drawing.Color.FromArgb(56, 142, 60));

			public static readonly PaletteSwatch Green800 = new PaletteSwatch("Green 800", System.Drawing.Color.FromArgb(46, 125, 50));

			public static readonly PaletteSwatch Green900 = new PaletteSwatch("Green 900", System.Drawing.Color.FromArgb(27, 94, 32));

			public static readonly PaletteSwatch GreenA100 = new PaletteSwatch("Green A100", System.Drawing.Color.FromArgb(185, 246, 202));

			public static readonly PaletteSwatch GreenA200 = new PaletteSwatch("Green A200", System.Drawing.Color.FromArgb(105, 240, 174));

			public static readonly PaletteSwatch GreenA400 = new PaletteSwatch("Green A400", System.Drawing.Color.FromArgb(0, 230, 118));

			public static readonly PaletteSwatch GreenA700 = new PaletteSwatch("Green A700", System.Drawing.Color.FromArgb(0, 200, 83));

			public static readonly PaletteSwatch LightGreen50 = new PaletteSwatch("Light Green 50", System.Drawing.Color.FromArgb(241, 248, 233));

			public static readonly PaletteSwatch LightGreen100 = new PaletteSwatch("Light Green 100", System.Drawing.Color.FromArgb(220, 237, 200));

			public static readonly PaletteSwatch LightGreen200 = new PaletteSwatch("Light Green 200", System.Drawing.Color.FromArgb(197, 225, 165));

			public static readonly PaletteSwatch LightGreen300 = new PaletteSwatch("Light Green 300", System.Drawing.Color.FromArgb(174, 213, 129));

			public static readonly PaletteSwatch LightGreen400 = new PaletteSwatch("Light Green 400", System.Drawing.Color.FromArgb(156, 204, 101));

			public static readonly PaletteSwatch LightGreen500 = new PaletteSwatch("Light Green 500", System.Drawing.Color.FromArgb(139, 195, 74));

			public static readonly PaletteSwatch LightGreen600 = new PaletteSwatch("Light Green 600", System.Drawing.Color.FromArgb(124, 179, 66));

			public static readonly PaletteSwatch LightGreen700 = new PaletteSwatch("Light Green 700", System.Drawing.Color.FromArgb(104, 159, 56));

			public static readonly PaletteSwatch LightGreen800 = new PaletteSwatch("Light Green 800", System.Drawing.Color.FromArgb(85, 139, 47));

			public static readonly PaletteSwatch LightGreen900 = new PaletteSwatch("Light Green 900", System.Drawing.Color.FromArgb(51, 105, 30));

			public static readonly PaletteSwatch LightGreenA100 = new PaletteSwatch("Light Green A100", System.Drawing.Color.FromArgb(204, 255, 144));

			public static readonly PaletteSwatch LightGreenA200 = new PaletteSwatch("Light Green A200", System.Drawing.Color.FromArgb(178, 255, 89));

			public static readonly PaletteSwatch LightGreenA400 = new PaletteSwatch("Light Green A400", System.Drawing.Color.FromArgb(118, 255, 3));

			public static readonly PaletteSwatch LightGreenA700 = new PaletteSwatch("Light Green A700", System.Drawing.Color.FromArgb(100, 221, 23));

			public static readonly PaletteSwatch Lime50 = new PaletteSwatch("Lime 50", System.Drawing.Color.FromArgb(240, 249, 235));

			public static readonly PaletteSwatch Lime100 = new PaletteSwatch("Lime 100", System.Drawing.Color.FromArgb(230, 244, 208));

			public static readonly PaletteSwatch Lime200 = new PaletteSwatch("Lime 200", System.Drawing.Color.FromArgb(220, 238, 179));

			public static readonly PaletteSwatch Lime300 = new PaletteSwatch("Lime 300", System.Drawing.Color.FromArgb(212, 225, 147));

			public static readonly PaletteSwatch Lime400 = new PaletteSwatch("Lime 400", System.Drawing.Color.FromArgb(197, 220, 121));

			public static readonly PaletteSwatch Lime500 = new PaletteSwatch("Lime 500", System.Drawing.Color.FromArgb(175, 222, 71));

			public static readonly PaletteSwatch Lime600 = new PaletteSwatch("Lime 600", System.Drawing.Color.FromArgb(165, 214, 58));

			public static readonly PaletteSwatch Lime700 = new PaletteSwatch("Lime 700", System.Drawing.Color.FromArgb(145, 198, 36));

			public static readonly PaletteSwatch Lime800 = new PaletteSwatch("Lime 800", System.Drawing.Color.FromArgb(124, 179, 27));

			public static readonly PaletteSwatch Lime900 = new PaletteSwatch("Lime 900", System.Drawing.Color.FromArgb(104, 159, 15));

			public static readonly PaletteSwatch LimeA100 = new PaletteSwatch("Lime A100", System.Drawing.Color.FromArgb(244, 255, 129));

			public static readonly PaletteSwatch LimeA200 = new PaletteSwatch("Lime A200", System.Drawing.Color.FromArgb(230, 255, 0));

			public static readonly PaletteSwatch LimeA400 = new PaletteSwatch("Lime A400", System.Drawing.Color.FromArgb(198, 255, 0));

			public static readonly PaletteSwatch LimeA700 = new PaletteSwatch("Lime A700", System.Drawing.Color.FromArgb(174, 234, 0));

			public static readonly PaletteSwatch Yellow50 = new PaletteSwatch("Yellow 50", System.Drawing.Color.FromArgb(255, 253, 231));

			public static readonly PaletteSwatch Yellow100 = new PaletteSwatch("Yellow 100", System.Drawing.Color.FromArgb(255, 249, 196));

			public static readonly PaletteSwatch Yellow200 = new PaletteSwatch("Yellow 200", System.Drawing.Color.FromArgb(255, 245, 157));

			public static readonly PaletteSwatch Yellow300 = new PaletteSwatch("Yellow 300", System.Drawing.Color.FromArgb(255, 241, 118));

			public static readonly PaletteSwatch Yellow400 = new PaletteSwatch("Yellow 400", System.Drawing.Color.FromArgb(255, 238, 88));

			public static readonly PaletteSwatch Yellow500 = new PaletteSwatch("Yellow 500", System.Drawing.Color.FromArgb(255, 235, 59));

			public static readonly PaletteSwatch Yellow600 = new PaletteSwatch("Yellow 600", System.Drawing.Color.FromArgb(253, 216, 53));

			public static readonly PaletteSwatch Yellow700 = new PaletteSwatch("Yellow 700", System.Drawing.Color.FromArgb(251, 192, 45));

			public static readonly PaletteSwatch Yellow800 = new PaletteSwatch("Yellow 800", System.Drawing.Color.FromArgb(249, 168, 37));

			public static readonly PaletteSwatch Yellow900 = new PaletteSwatch("Yellow 900", System.Drawing.Color.FromArgb(245, 127, 23));

			public static readonly PaletteSwatch YellowA100 = new PaletteSwatch("Yellow A100", System.Drawing.Color.FromArgb(255, 255, 141));

			public static readonly PaletteSwatch YellowA200 = new PaletteSwatch("Yellow A200", System.Drawing.Color.FromArgb(255, 255, 0));

			public static readonly PaletteSwatch YellowA400 = new PaletteSwatch("Yellow A400", System.Drawing.Color.FromArgb(255, 234, 0));

			public static readonly PaletteSwatch YellowA700 = new PaletteSwatch("Yellow A700", System.Drawing.Color.FromArgb(255, 214, 0));

			public static readonly PaletteSwatch Amber50 = new PaletteSwatch("Amber 50", System.Drawing.Color.FromArgb(255, 248, 225));

			public static readonly PaletteSwatch Amber100 = new PaletteSwatch("Amber 100", System.Drawing.Color.FromArgb(255, 236, 179));

			public static readonly PaletteSwatch Amber200 = new PaletteSwatch("Amber 200", System.Drawing.Color.FromArgb(255, 224, 130));

			public static readonly PaletteSwatch Amber300 = new PaletteSwatch("Amber 300", System.Drawing.Color.FromArgb(255, 213, 79));

			public static readonly PaletteSwatch Amber400 = new PaletteSwatch("Amber 400", System.Drawing.Color.FromArgb(255, 202, 40));

			public static readonly PaletteSwatch Amber500 = new PaletteSwatch("Amber 500", System.Drawing.Color.FromArgb(255, 193, 7));

			public static readonly PaletteSwatch Amber600 = new PaletteSwatch("Amber 600", System.Drawing.Color.FromArgb(255, 179, 0));

			public static readonly PaletteSwatch Amber700 = new PaletteSwatch("Amber 700", System.Drawing.Color.FromArgb(255, 160, 0));

			public static readonly PaletteSwatch Amber800 = new PaletteSwatch("Amber 800", System.Drawing.Color.FromArgb(255, 143, 0));

			public static readonly PaletteSwatch Amber900 = new PaletteSwatch("Amber 900", System.Drawing.Color.FromArgb(255, 111, 0));

			public static readonly PaletteSwatch AmberA100 = new PaletteSwatch("Amber A100", System.Drawing.Color.FromArgb(255, 255, 209));

			public static readonly PaletteSwatch AmberA200 = new PaletteSwatch("Amber A200", System.Drawing.Color.FromArgb(255, 255, 171));

			public static readonly PaletteSwatch AmberA400 = new PaletteSwatch("Amber A400", System.Drawing.Color.FromArgb(255, 255, 145));

			public static readonly PaletteSwatch AmberA700 = new PaletteSwatch("Amber A700", System.Drawing.Color.FromArgb(255, 255, 109));

			public static readonly PaletteSwatch Orange50 = new PaletteSwatch("Orange 50", System.Drawing.Color.FromArgb(255, 243, 224));

			public static readonly PaletteSwatch Orange100 = new PaletteSwatch("Orange 100", System.Drawing.Color.FromArgb(255, 224, 178));

			public static readonly PaletteSwatch Orange200 = new PaletteSwatch("Orange 200", System.Drawing.Color.FromArgb(255, 204, 128));

			public static readonly PaletteSwatch Orange300 = new PaletteSwatch("Orange 300", System.Drawing.Color.FromArgb(255, 183, 77));

			public static readonly PaletteSwatch Orange400 = new PaletteSwatch("Orange 400", System.Drawing.Color.FromArgb(255, 167, 38));

			public static readonly PaletteSwatch Orange500 = new PaletteSwatch("Orange 500", System.Drawing.Color.FromArgb(255, 152, 0));

			public static readonly PaletteSwatch Orange600 = new PaletteSwatch("Orange 600", System.Drawing.Color.FromArgb(251, 140, 0));

			public static readonly PaletteSwatch Orange700 = new PaletteSwatch("Orange 700", System.Drawing.Color.FromArgb(245, 124, 0));

			public static readonly PaletteSwatch Orange800 = new PaletteSwatch("Orange 800", System.Drawing.Color.FromArgb(239, 108, 0));

			public static readonly PaletteSwatch Orange900 = new PaletteSwatch("Orange 900", System.Drawing.Color.FromArgb(230, 81, 0));

			public static readonly PaletteSwatch OrangeA100 = new PaletteSwatch("Orange A100", System.Drawing.Color.FromArgb(255, 209, 128));

			public static readonly PaletteSwatch OrangeA200 = new PaletteSwatch("Orange A200", System.Drawing.Color.FromArgb(255, 171, 64));

			public static readonly PaletteSwatch OrangeA400 = new PaletteSwatch("Orange A400", System.Drawing.Color.FromArgb(255, 145, 0));

			public static readonly PaletteSwatch OrangeA700 = new PaletteSwatch("Orange A700", System.Drawing.Color.FromArgb(255, 109, 0));

			public static readonly PaletteSwatch DeepOrange50 = new PaletteSwatch("Deep Orange 50", System.Drawing.Color.FromArgb(251, 233, 231));

			public static readonly PaletteSwatch DeepOrange100 = new PaletteSwatch("Deep Orange 100", System.Drawing.Color.FromArgb(255, 204, 188));

			public static readonly PaletteSwatch DeepOrange200 = new PaletteSwatch("Deep Orange 200", System.Drawing.Color.FromArgb(255, 171, 145));

			public static readonly PaletteSwatch DeepOrange300 = new PaletteSwatch("Deep Orange 300", System.Drawing.Color.FromArgb(255, 138, 101));

			public static readonly PaletteSwatch DeepOrange400 = new PaletteSwatch("Deep Orange 400", System.Drawing.Color.FromArgb(255, 112, 67));

			public static readonly PaletteSwatch DeepOrange500 = new PaletteSwatch("Deep Orange 500", System.Drawing.Color.FromArgb(255, 87, 34));

			public static readonly PaletteSwatch DeepOrange600 = new PaletteSwatch("Deep Orange 600", System.Drawing.Color.FromArgb(244, 81, 30));

			public static readonly PaletteSwatch DeepOrange700 = new PaletteSwatch("Deep Orange 700", System.Drawing.Color.FromArgb(230, 74, 25));

			public static readonly PaletteSwatch DeepOrange800 = new PaletteSwatch("Deep Orange 800", System.Drawing.Color.FromArgb(216, 67, 21));

			public static readonly PaletteSwatch DeepOrange900 = new PaletteSwatch("Deep Orange 900", System.Drawing.Color.FromArgb(191, 54, 12));

			public static readonly PaletteSwatch DeepOrangeA100 = new PaletteSwatch("Deep Orange A100", System.Drawing.Color.FromArgb(255, 158, 128));

			public static readonly PaletteSwatch DeepOrangeA200 = new PaletteSwatch("Deep Orange A200", System.Drawing.Color.FromArgb(255, 110, 64));

			public static readonly PaletteSwatch DeepOrangeA400 = new PaletteSwatch("Deep Orange A400", System.Drawing.Color.FromArgb(255, 61, 0));

			public static readonly PaletteSwatch DeepOrangeA700 = new PaletteSwatch("Deep Orange A700", System.Drawing.Color.FromArgb(221, 44, 0));

			public static readonly PaletteSwatch Brown50 = new PaletteSwatch("Brown 50", System.Drawing.Color.FromArgb(239, 235, 233));

			public static readonly PaletteSwatch Brown100 = new PaletteSwatch("Brown 100", System.Drawing.Color.FromArgb(215, 204, 200));

			public static readonly PaletteSwatch Brown200 = new PaletteSwatch("Brown 200", System.Drawing.Color.FromArgb(188, 170, 164));

			public static readonly PaletteSwatch Brown300 = new PaletteSwatch("Brown 300", System.Drawing.Color.FromArgb(161, 136, 127));

			public static readonly PaletteSwatch Brown400 = new PaletteSwatch("Brown 400", System.Drawing.Color.FromArgb(141, 110, 99));

			public static readonly PaletteSwatch Brown500 = new PaletteSwatch("Brown 500", System.Drawing.Color.FromArgb(121, 85, 72));

			public static readonly PaletteSwatch Brown600 = new PaletteSwatch("Brown 600", System.Drawing.Color.FromArgb(109, 76, 65));

			public static readonly PaletteSwatch Brown700 = new PaletteSwatch("Brown 700", System.Drawing.Color.FromArgb(93, 64, 55));

			public static readonly PaletteSwatch Brown800 = new PaletteSwatch("Brown 800", System.Drawing.Color.FromArgb(78, 52, 46));

			public static readonly PaletteSwatch Brown900 = new PaletteSwatch("Brown 900", System.Drawing.Color.FromArgb(62, 39, 35));

			public static readonly PaletteSwatch Grey50 = new PaletteSwatch("Grey 50", System.Drawing.Color.FromArgb(250, 250, 250));

			public static readonly PaletteSwatch Grey100 = new PaletteSwatch("Grey 100", System.Drawing.Color.FromArgb(245, 245, 245));

			public static readonly PaletteSwatch Grey200 = new PaletteSwatch("Grey 200", System.Drawing.Color.FromArgb(238, 238, 238));

			public static readonly PaletteSwatch Grey300 = new PaletteSwatch("Grey 300", System.Drawing.Color.FromArgb(224, 224, 224));

			public static readonly PaletteSwatch Grey400 = new PaletteSwatch("Grey 400", System.Drawing.Color.FromArgb(189, 189, 189));

			public static readonly PaletteSwatch Grey500 = new PaletteSwatch("Grey 500", System.Drawing.Color.FromArgb(158, 158, 158));

			public static readonly PaletteSwatch Grey600 = new PaletteSwatch("Grey 600", System.Drawing.Color.FromArgb(117, 117, 117));

			public static readonly PaletteSwatch Grey700 = new PaletteSwatch("Grey 700", System.Drawing.Color.FromArgb(97, 97, 97));

			public static readonly PaletteSwatch Grey800 = new PaletteSwatch("Grey 800", System.Drawing.Color.FromArgb(66, 66, 66));

			public static readonly PaletteSwatch Grey900 = new PaletteSwatch("Grey 900", System.Drawing.Color.FromArgb(33, 33, 33));

			public static readonly PaletteSwatch BlueGrey50 = new PaletteSwatch("Blue Grey 50", System.Drawing.Color.FromArgb(236, 239, 241));

			public static readonly PaletteSwatch BlueGrey100 = new PaletteSwatch("Blue Grey 100", System.Drawing.Color.FromArgb(207, 216, 220));

			public static readonly PaletteSwatch BlueGrey200 = new PaletteSwatch("Blue Grey 200", System.Drawing.Color.FromArgb(176, 190, 197));

			public static readonly PaletteSwatch BlueGrey300 = new PaletteSwatch("Blue Grey 300", System.Drawing.Color.FromArgb(144, 164, 174));

			public static readonly PaletteSwatch BlueGrey400 = new PaletteSwatch("Blue Grey 400", System.Drawing.Color.FromArgb(120, 144, 156));

			public static readonly PaletteSwatch BlueGrey500 = new PaletteSwatch("Blue Grey 500", System.Drawing.Color.FromArgb(96, 125, 139));

			public static readonly PaletteSwatch BlueGrey600 = new PaletteSwatch("Blue Grey 600", System.Drawing.Color.FromArgb(84, 110, 122));

			public static readonly PaletteSwatch BlueGrey700 = new PaletteSwatch("Blue Grey 700", System.Drawing.Color.FromArgb(69, 90, 100));

			public static readonly PaletteSwatch BlueGrey800 = new PaletteSwatch("Blue Grey 800", System.Drawing.Color.FromArgb(55, 71, 79));

			public static readonly PaletteSwatch BlueGrey900 = new PaletteSwatch("Blue Grey 900", System.Drawing.Color.FromArgb(38, 50, 56));

			public static readonly Dictionary<string, List<PaletteSwatch>> All = new Dictionary<string, List<PaletteSwatch>>
			{
				{
					"Red",
					new List<PaletteSwatch>
					{
						Red50, Red100, Red200, Red300, Red400, Red500, Red600, Red700, Red800, Red900,
						RedA100, RedA200, RedA400, RedA700
					}
				},
				{
					"Pink",
					new List<PaletteSwatch>
					{
						Pink50, Pink100, Pink200, Pink300, Pink400, Pink500, Pink600, Pink700, Pink800, Pink900,
						PinkA100, PinkA200, PinkA400, PinkA700
					}
				},
				{
					"Purple",
					new List<PaletteSwatch>
					{
						Purple50, Purple100, Purple200, Purple300, Purple400, Purple500, Purple600, Purple700, Purple800, Purple900,
						PurpleA100, PurpleA200, PurpleA400, PurpleA700
					}
				},
				{
					"Deep Purple",
					new List<PaletteSwatch>
					{
						DeepPurple50, DeepPurple100, DeepPurple200, DeepPurple300, DeepPurple400, DeepPurple500, DeepPurple600, DeepPurple700, DeepPurple800, DeepPurple900,
						DeepPurpleA100, DeepPurpleA200, DeepPurpleA400, DeepPurpleA700
					}
				},
				{
					"Indigo",
					new List<PaletteSwatch>
					{
						Indigo50, Indigo100, Indigo200, Indigo300, Indigo400, Indigo500, Indigo600, Indigo700, Indigo800, Indigo900,
						IndigoA100, IndigoA200, IndigoA400, IndigoA700
					}
				},
				{
					"Blue",
					new List<PaletteSwatch>
					{
						Blue50, Blue100, Blue200, Blue300, Blue400, Blue500, Blue600, Blue700, Blue800, Blue900,
						BlueA100, BlueA200, BlueA400, BlueA700
					}
				},
				{
					"Light Blue",
					new List<PaletteSwatch>
					{
						LightBlue50, LightBlue100, LightBlue200, LightBlue300, LightBlue400, LightBlue500, LightBlue600, LightBlue700, LightBlue800, LightBlue900,
						LightBlueA100, LightBlueA200, LightBlueA400, LightBlueA700
					}
				},
				{
					"Cyan",
					new List<PaletteSwatch>
					{
						Cyan50, Cyan100, Cyan200, Cyan300, Cyan400, Cyan500, Cyan600, Cyan700, Cyan800, Cyan900,
						CyanA100, CyanA200, CyanA400, CyanA700
					}
				},
				{
					"Teal",
					new List<PaletteSwatch>
					{
						Teal50, Teal100, Teal200, Teal300, Teal400, Teal500, Teal600, Teal700, Teal800, Teal900,
						TealA100, TealA200, TealA400, TealA700
					}
				},
				{
					"Green",
					new List<PaletteSwatch>
					{
						Green50, Green100, Green200, Green300, Green400, Green500, Green600, Green700, Green800, Green900,
						GreenA100, GreenA200, GreenA400, GreenA700
					}
				},
				{
					"Light Green",
					new List<PaletteSwatch>
					{
						LightGreen50, LightGreen100, LightGreen200, LightGreen300, LightGreen400, LightGreen500, LightGreen600, LightGreen700, LightGreen800, LightGreen900,
						LightGreenA100, LightGreenA200, LightGreenA400, LightGreenA700
					}
				},
				{
					"Lime",
					new List<PaletteSwatch>
					{
						Lime50, Lime100, Lime200, Lime300, Lime400, Lime500, Lime600, Lime700, Lime800, Lime900,
						LimeA100, LimeA200, LimeA400, LimeA700
					}
				},
				{
					"Yellow",
					new List<PaletteSwatch>
					{
						Yellow50, Yellow100, Yellow200, Yellow300, Yellow400, Yellow500, Yellow600, Yellow700, Yellow800, Yellow900,
						YellowA100, YellowA200, YellowA400, YellowA700
					}
				},
				{
					"Amber",
					new List<PaletteSwatch>
					{
						Amber50, Amber100, Amber200, Amber300, Amber400, Amber500, Amber600, Amber700, Amber800, Amber900,
						AmberA100, AmberA200, AmberA400, AmberA700
					}
				},
				{
					"Orange",
					new List<PaletteSwatch>
					{
						Orange50, Orange100, Orange200, Orange300, Orange400, Orange500, Orange600, Orange700, Orange800, Orange900,
						OrangeA100, OrangeA200, OrangeA400, OrangeA700
					}
				},
				{
					"Deep Orange",
					new List<PaletteSwatch>
					{
						DeepOrange50, DeepOrange100, DeepOrange200, DeepOrange300, DeepOrange400, DeepOrange500, DeepOrange600, DeepOrange700, DeepOrange800, DeepOrange900,
						DeepOrangeA100, DeepOrangeA200, DeepOrangeA400, DeepOrangeA700
					}
				},
				{
					"Brown",
					new List<PaletteSwatch> { Brown50, Brown100, Brown200, Brown300, Brown400, Brown500, Brown600, Brown700, Brown800, Brown900 }
				},
				{
					"Grey",
					new List<PaletteSwatch> { Grey50, Grey100, Grey200, Grey300, Grey400, Grey500, Grey600, Grey700, Grey800, Grey900 }
				},
				{
					"Blue Grey",
					new List<PaletteSwatch> { BlueGrey50, BlueGrey100, BlueGrey200, BlueGrey300, BlueGrey400, BlueGrey500, BlueGrey600, BlueGrey700, BlueGrey800, BlueGrey900 }
				}
			};
		}
	}

	private static class LoggerImpl
	{
		private const uint BUFFER_SIZE = 65536u;

		private const int MAX_LOG_ENTRIES = 500;

		private static readonly LinkedList<(DateTime Date, string Description, int Count)> _logEntries = new LinkedList<(DateTime, string, int)>();

		private static string _lastEntry = string.Empty;

		private static int _repeatCount = 1;

		private static readonly object _logLock = new object();

		public static void AddLogEntry(string entry)
		{
			lock (_logLock)
			{
				if (_logEntries.Count > 0 && entry == _logEntries.Last.Value.Description)
				{
					(DateTime, string, int) value = _logEntries.Last.Value;
					_logEntries.RemoveLast();
					_logEntries.AddLast((DateTime.Now, entry, value.Item3 + 1));
				}
				else
				{
					_repeatCount = 1;
					_lastEntry = entry;
					_logEntries.AddLast((DateTime.Now, entry, 1));
				}
				while (_logEntries.Count > 0 && _logEntries.Count > 500)
				{
					_logEntries.RemoveFirst();
				}
			}
		}

		public static void Clear()
		{
			lock (_logLock)
			{
				_logEntries.Clear();
				_lastEntry = string.Empty;
				_repeatCount = 1;
			}
		}

		public static void Render(bool newestFirst = false)
		{
			List<(DateTime, string, int)> list;
			lock (_logLock)
			{
				list = (newestFirst ? _logEntries.Reverse().ToList() : _logEntries.ToList());
			}
			if (!Window.Begin(PluginName + "Log", ref Settings.ShowLog, new Window.Options
			{
				Title = PluginName + " DBug Log",
				Resizable = true
			}))
			{
				return;
			}
			ImGui.GetCursorScreenPos();
			ImGui.Indent(2f);
			ImGui.PushStyleVar((ImGuiStyleVar)14, new Vector2(1f, 0f));
			if (Button.Draw(PluginName + "LogClear", new Button.Options
			{
				Label = "Clear",
				Width = 80,
				Height = 22,
				Tooltip = new Tooltip.Options("Clear Log History")
			}))
			{
				Clear();
			}
			ImGui.SameLine();
			LogHeader?.Invoke(80, 22);
			ImGui.PopStyleVar();
			if (ImGui.GetContentRegionAvail().X > 5f)
			{
				Bar.Draw(new Bar.Options
				{
					Width = -2,
					Height = 22
				});
			}
			ImGui.Dummy(new Vector2(0f, 2f));
			ImGui.Indent(1f);
			string uniqueID = PluginName + "LogPanel";
			Panel.Begin(uniqueID, new Panel.Options
			{
				Width = -3,
				Height = -3
			});
			StringBuilder stringBuilder = new StringBuilder(65536);
			foreach (var item4 in list)
			{
				DateTime item = item4.Item1;
				string item2 = item4.Item2;
				int item3 = item4.Item3;
				string value = ((item3 > 1) ? $"[{item3}] {item2}" : item2);
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
				handler.AppendFormatted(item, "HH:mm:ss.fff");
				handler.AppendLiteral(": ");
				handler.AppendFormatted(value);
				stringBuilder2.AppendLine(ref handler);
			}
			string text = stringBuilder.ToString();
			Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
			ImGui.InputTextMultiline(PluginName + "LogText", ref text, 65536u, new Vector2(contentRegionAvail.X - 4f, contentRegionAvail.Y - 4f), (ImGuiInputTextFlags)512);
			Panel.End(uniqueID);
			ImGui.Unindent(3f);
			Window.End();
		}
	}

	private static class MonitorImpl
	{
		private class MonitoredVariable
		{
			public DateTime DateChanged;

			public DateTime DateUpdated;

			public string Category;

			public string Name;

			public object Value;

			public string FilePath;

			public string? FileName;

			public string? ShortFilePath;

			public int Line;

			public string Member;

			public List<(DateTime Time, object Value)> ChangeHistory = new List<(DateTime, object)>();
		}

		private static SortedDictionary<string, SortedDictionary<string, MonitoredVariable>> monitoredVariables = new SortedDictionary<string, SortedDictionary<string, MonitoredVariable>>();

		private static int windowHeight = 400;

		public static void AddMonitoredVariable(string category, string name, object variable, string filePath, int line, string member)
		{
			if (!Settings.MonitorPanelsCollapsed.ContainsKey(category))
			{
				Settings.MonitorPanelsCollapsed[category] = false;
			}
			if (!monitoredVariables.TryGetValue(category, out SortedDictionary<string, MonitoredVariable> value))
			{
				value = new SortedDictionary<string, MonitoredVariable>();
				monitoredVariables[category] = value;
			}
			if (!value.TryGetValue(name, out var value2))
			{
				string shortFilePath = null;
				string text = null;
				if (!string.IsNullOrEmpty(filePath))
				{
					string text2 = Path.GetDirectoryName(filePath)?.Split(Path.DirectorySeparatorChar).Last();
					text = Path.GetFileName(filePath);
					shortFilePath = ((text2 != null) ? $"{text2}{Path.DirectorySeparatorChar}{text}" : text);
				}
				value2 = new MonitoredVariable
				{
					Category = category,
					Name = name,
					Value = variable,
					DateChanged = DateTime.Now,
					FilePath = filePath,
					Line = line,
					Member = member,
					ShortFilePath = shortFilePath,
					FileName = text,
					ChangeHistory = new List<(DateTime, object)>()
				};
				value[name] = value2;
				return;
			}
			value2.DateUpdated = DateTime.Now;
			if ((value2.Value == null) ? (variable != null) : (!object.Equals(value2.Value, variable)))
			{
				if (value2.Value != null)
				{
					value2.ChangeHistory.Add((value2.DateChanged, value2.Value));
					if (value2.ChangeHistory.Count > 10)
					{
						value2.ChangeHistory.RemoveAt(0);
					}
				}
				value2.DateChanged = DateTime.Now;
			}
			value2.Value = variable;
		}

		private static IRenderableText FormatVariable(object variable)
		{
			if (variable == null)
			{
				return new ColoredText(Colors.TextRed.ToColorCode() + "null");
			}
			if (!(variable is int value))
			{
				if (!(variable is float value2))
				{
					if (!(variable is double value3))
					{
						if (!(variable is bool value4))
						{
							if (!(variable is Enum value5))
							{
								if (!(variable is Vector2 vector))
								{
									if (!(variable is string text))
									{
										if (!(variable is IEnumerable<object> source))
										{
											if (variable is DateTime value6)
											{
												return new ColoredText($"{Colors.TextSky.ToColorCode()}{value6:yyyy-MM-dd HH:mm:ss}");
											}
											return new PlainText(variable.ToString());
										}
										return new ColoredText($"{Colors.InputText.ToColorCode()}List[{source.Count()}]");
									}
									if (string.IsNullOrWhiteSpace(text))
									{
										return new ColoredText(Colors.TextRed.ToColorCode() + "empty string");
									}
									return new PlainText(text);
								}
								return new ColoredText($"<{Colors.TextYellow.ToColorCode()}{vector.X}|r, {Colors.TextYellow.ToColorCode()}{vector.Y}|r>");
							}
							return new ColoredText($"{Colors.TextPurple.ToColorCode()}{value5}");
						}
						return new ColoredText($"{Colors.TextPurple.ToColorCode()}{value4}");
					}
					return new ColoredText($"{Colors.TextYellow.ToColorCode()}{value3}");
				}
				return new ColoredText($"{Colors.TextYellow.ToColorCode()}{value2}");
			}
			return new ColoredText($"{Colors.TextYellow.ToColorCode()}{value}");
		}

		public static void Render(bool newestFirst = false)
		{
			if (!Window.Begin(PluginName + "DBuggerMonitor", ref Settings.ShowMonitor, new Window.Options
			{
				Title = PluginName + " DBug Monitor",
				Resizable = true,
				MinWidth = 200f,
				LockHeight = windowHeight
			}))
			{
				return;
			}
			windowHeight = 20;
			Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
			ImGui.Indent(3f);
			foreach (KeyValuePair<string, SortedDictionary<string, MonitoredVariable>> monitoredVariable in monitoredVariables)
			{
				bool collapsed = Settings.MonitorPanelsCollapsed[monitoredVariable.Key];
				string uniqueID = PluginName + "DBuggerMonitor" + monitoredVariable.Key + "Panel";
				if (CollapsingPanel.Begin(uniqueID, ref collapsed, new CollapsingPanel.Options
				{
					Label = (monitoredVariable.Key ?? ""),
					Width = -3
				}))
				{
					bool flag = true;
					foreach (KeyValuePair<string, MonitoredVariable> item2 in monitoredVariable.Value)
					{
						if (!flag)
						{
							ImGui.Dummy(new Vector2(0f, 3f));
						}
						MonitoredVariable value = item2.Value;
						Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
						int num = (int)(contentRegionAvail.X * 0.6f);
						float num2 = contentRegionAvail.X - (float)num;
						Label.Draw(value.Name, new Label.Options
						{
							Width = (int)num2,
							Height = 20
						});
						ImGui.SameLine();
						IRenderableText text = FormatVariable(value.Value);
						Label.Draw(text, new Label.Options
						{
							Width = num - 3,
							Height = 20,
							DrawBG = true,
							PadLeft = 3
						});
						if (ImGui.IsItemHovered())
						{
							if (ImGui.IsMouseClicked((ImGuiMouseButton)0))
							{
								ImGui.SetClipboardText(value.Value.ToString());
							}
							List<Tooltip.Line> obj = new List<Tooltip.Line>
							{
								new Tooltip.DoubleLine
								{
									LeftText = value.Name,
									LeftColor = Colors.TextonControl,
									RightText = ((value.Value != null) ? value.Value.GetType().ToString() : "null")
								},
								new Tooltip.Separator()
							};
							Tooltip.DoubleLine obj2 = new Tooltip.DoubleLine
							{
								LeftText = "File:"
							};
							obj2.RightText = (value.ShortFilePath ?? value.FileName ?? value.FilePath) ?? "";
							obj.Add(obj2);
							obj.Add(new Tooltip.DoubleLine
							{
								LeftText = "Line:",
								RightText = $"{value.Line}"
							});
							obj.Add(new Tooltip.DoubleLine
							{
								LeftText = "Member:",
								RightText = (value.Member ?? "")
							});
							obj.Add(new Tooltip.Separator());
							obj.Add(new Tooltip.DoubleLine
							{
								LeftText = "Updated:",
								RightText = $"{value.DateUpdated:HH:mm:ss.fff}"
							});
							obj.Add(new Tooltip.DoubleLine
							{
								LeftText = "Changed:",
								RightText = $"{value.DateChanged:HH:mm:ss.fff}"
							});
							obj.Add(new Tooltip.Separator());
							List<Tooltip.Line> list = obj;
							if (value.ChangeHistory.Count > 0)
							{
								foreach (var item3 in value.ChangeHistory)
								{
									DateTime item = item3.Time;
									text = FormatVariable(item3.Value);
									list.Add(new Tooltip.DoubleLine
									{
										LeftText = item.ToString("HH:mm:ss.fff"),
										RightText = text.ToString()
									});
								}
								list.Add(new Tooltip.Separator());
							}
							list.Add(new Tooltip.DoubleLine
							{
								LeftText = "LeftClick:",
								RightText = "Copy to clipboard"
							});
							Tooltip.Draw(new Tooltip.Options
							{
								Lines = list
							});
						}
						flag = false;
					}
					ImGui.Dummy(new Vector2(0f, 3f));
					CollapsingPanel.End(uniqueID);
				}
				ImGui.Dummy(new Vector2(0f, 3f));
				Settings.MonitorPanelsCollapsed[monitoredVariable.Key] = collapsed;
			}
			ImGui.Unindent(3f);
			windowHeight += (int)(ImGui.GetCursorScreenPos().Y - cursorScreenPos.Y);
			Window.End();
		}
	}

	private static class StyleDemo
	{
		private class buttonStyle
		{
			public string Name;

			public System.Drawing.Color BackgroundColor;

			public System.Drawing.Color TextColor;
		}

		private class labelStyle
		{
			public string Name;

			public System.Drawing.Color TextColor;
		}

		private static float windowHeight = 20f;

		private static Vector2 controlSize = new Vector2(100f, 22f);

		private static int controlWidth = (int)controlSize.X;

		private static int controlHeight = (int)controlSize.Y;

		private static List<buttonStyle> DefaultButtons = new List<buttonStyle>
		{
			new buttonStyle
			{
				Name = "Red",
				BackgroundColor = Colors.ControlRed,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Orange",
				BackgroundColor = Colors.ControlOrange,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Amber",
				BackgroundColor = Colors.ControlAmber,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Yellow",
				BackgroundColor = Colors.ControlYellow,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Lime",
				BackgroundColor = Colors.ControlLime,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Green",
				BackgroundColor = Colors.ControlGreen,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Emerald",
				BackgroundColor = Colors.ControlEmerald,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Teal",
				BackgroundColor = Colors.ControlTeal,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Cyan",
				BackgroundColor = Colors.ControlCyan,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Sky",
				BackgroundColor = Colors.ControlSky,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Blue",
				BackgroundColor = Colors.ControlBlue,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Indigo",
				BackgroundColor = Colors.ControlIndigo,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Violet",
				BackgroundColor = Colors.ControlViolet,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Purple",
				BackgroundColor = Colors.ControlPurple,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Fuchsia",
				BackgroundColor = Colors.ControlFuchsia,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Pink",
				BackgroundColor = Colors.ControlPink,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Rose",
				BackgroundColor = Colors.ControlRose,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Slate",
				BackgroundColor = Colors.ControlSlate,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Gray",
				BackgroundColor = Colors.ControlGray,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Zinc",
				BackgroundColor = Colors.ControlZinc,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Neutral",
				BackgroundColor = Colors.ControlNeutral,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Stone",
				BackgroundColor = Colors.ControlStone,
				TextColor = Colors.TextOnColor
			},
			new buttonStyle
			{
				Name = "Default",
				BackgroundColor = Colors.Button,
				TextColor = Colors.ButtonText
			},
			new buttonStyle
			{
				Name = "Checked",
				BackgroundColor = Colors.ButtonChecked,
				TextColor = Colors.ButtonTextChecked
			}
		};

		private static List<labelStyle> DefaultTextColors = new List<labelStyle>
		{
			new labelStyle
			{
				Name = "Red",
				TextColor = Colors.TextRed
			},
			new labelStyle
			{
				Name = "Orange",
				TextColor = Colors.TextOrange
			},
			new labelStyle
			{
				Name = "Amber",
				TextColor = Colors.TextAmber
			},
			new labelStyle
			{
				Name = "Yellow",
				TextColor = Colors.TextYellow
			},
			new labelStyle
			{
				Name = "Lime",
				TextColor = Colors.TextLime
			},
			new labelStyle
			{
				Name = "Green",
				TextColor = Colors.TextGreen
			},
			new labelStyle
			{
				Name = "Emerald",
				TextColor = Colors.TextEmerald
			},
			new labelStyle
			{
				Name = "Teal",
				TextColor = Colors.TextTeal
			},
			new labelStyle
			{
				Name = "Cyan",
				TextColor = Colors.TextCyan
			},
			new labelStyle
			{
				Name = "Sky",
				TextColor = Colors.TextSky
			},
			new labelStyle
			{
				Name = "Blue",
				TextColor = Colors.TextBlue
			},
			new labelStyle
			{
				Name = "Indigo",
				TextColor = Colors.TextIndigo
			},
			new labelStyle
			{
				Name = "Violet",
				TextColor = Colors.TextViolet
			},
			new labelStyle
			{
				Name = "Purple",
				TextColor = Colors.TextPurple
			},
			new labelStyle
			{
				Name = "Fuchsia",
				TextColor = Colors.TextFuchsia
			},
			new labelStyle
			{
				Name = "Pink",
				TextColor = Colors.TextPink
			},
			new labelStyle
			{
				Name = "Rose",
				TextColor = Colors.TextRose
			},
			new labelStyle
			{
				Name = "Slate",
				TextColor = Colors.TextSlate
			},
			new labelStyle
			{
				Name = "Stone",
				TextColor = Colors.TextStone
			},
			new labelStyle
			{
				Name = "Colors.Text",
				TextColor = Colors.Text
			},
			new labelStyle
			{
				Name = "Colors.InputText",
				TextColor = Colors.InputText
			}
		};

		public static void Render()
		{
			Window.Options options = new Window.Options
			{
				Title = PluginName + " UI Coloring",
				Resizable = true,
				MinHeight = 780f,
				MinWidth = 900f,
				TitleTextColor = Settings.TitleTextColor
			};
			Panel.Options options2 = new Panel.Options
			{
				Width = -3,
				Height = -3,
				Color = Settings.PanelColor
			};
			string text = PluginName + "LogPanel";
			ColorSelect.Options options3 = new ColorSelect.Options
			{
				Width = controlHeight,
				Height = controlHeight
			};
			Button.Options options4 = new Button.Options
			{
				Width = controlWidth,
				Height = controlHeight
			};
			Slider.Options options5 = new Slider.Options
			{
				Width = controlWidth,
				Height = controlHeight,
				Min = 0f,
				Max = 100f
			};
			if (!Window.Begin(PluginName + "UIColoring", ref Settings.ShowStyleDemo, options))
			{
				return;
			}
			ImGui.GetCursorScreenPos();
			windowHeight = options.TitleBarHeight;
			ImGui.Indent(3f);
			Panel.Begin(text, options2);
			ImGui.PushStyleVar((ImGuiStyleVar)14, new Vector2(3f, 3f));
			string popupId = text + "menuID";
			options4.Label = "Menu";
			if (Button.Draw(text + "menu", options4))
			{
				Menu.Open(popupId, new List<Menu.Item>
				{
					new Menu.Item
					{
						Label = "Top Action",
						OnClick = delegate
						{
							Log("Top clicked");
						}
					},
					new Menu.Item
					{
						Label = "Submenu 1",
						SubMenu = new List<Menu.Item>
						{
							new Menu.Item
							{
								Label = "Sub Action 1",
								OnClick = delegate
								{
									Log("Sub Action 1 clicked");
								}
							},
							new Menu.Item
							{
								Label = "Sub Action 2",
								OnClick = delegate
								{
									Log("Sub Action 2 clicked");
								}
							},
							new Menu.Item
							{
								Label = "Deep Submenu",
								SubMenu = new List<Menu.Item>
								{
									new Menu.Item
									{
										Label = "Deep Action 1",
										OnClick = delegate
										{
											Log("Deep Action 1 clicked");
										}
									},
									new Menu.Item
									{
										Label = "Deep Action 2",
										OnClick = delegate
										{
											Log("Deep Action 2 clicked");
										}
									},
									new Menu.Item
									{
										Label = "Deeper Submenu",
										SubMenu = new List<Menu.Item>
										{
											new Menu.Item
											{
												Label = "Deeper Action 1",
												OnClick = delegate
												{
													Log("Deeper Action 1 clicked");
												}
											},
											new Menu.Item
											{
												Label = "Deeper Action 2",
												OnClick = delegate
												{
													Log("Deeper Action 2 clicked");
												}
											},
											new Menu.Item
											{
												Label = "Deeper Action 3",
												OnClick = delegate
												{
													Log("Deeper Action 3 clicked");
												}
											}
										}
									}
								}
							},
							new Menu.Item
							{
								Label = "Sub Action 3",
								OnClick = delegate
								{
									Log("Sub Action 3 clicked");
								}
							}
						}
					},
					new Menu.Item
					{
						Label = "Submenu 2",
						SubMenu = new List<Menu.Item>
						{
							new Menu.Item
							{
								Label = "Sub2 Action 1",
								OnClick = delegate
								{
									Log("Sub2 Action 1 clicked");
								}
							},
							new Menu.Item
							{
								Label = "Sub2 Action 2",
								OnClick = delegate
								{
									Log("Sub2 Action 2 clicked");
								}
							},
							new Menu.Item
							{
								Label = "Sub2 Deep Submenu",
								SubMenu = new List<Menu.Item>
								{
									new Menu.Item
									{
										Label = "Sub2 Deep Action 1",
										OnClick = delegate
										{
											Log("Sub2 Deep Action 1 clicked");
										}
									},
									new Menu.Item
									{
										Label = "Sub2 Deep Action 2",
										OnClick = delegate
										{
											Log("Sub2 Deep Action 2 clicked");
										}
									}
								}
							}
						}
					},
					new Menu.Item
					{
						Label = "Plain Item",
						OnClick = delegate
						{
							Log("Plain clicked");
						}
					},
					new Menu.Item
					{
						Label = "Checkable Item",
						OnClick = delegate
						{
							Log("Checkable clicked");
						}
					},
					new Menu.Item
					{
						Label = "Disabled Item",
						Enabled = false,
						OnClick = delegate
						{
							Log("Checkable clicked");
						}
					},
					new Menu.Item
					{
						Separator = true,
						SeparatorColor = Colors.TextOrange
					},
					new Menu.Item
					{
						Label = "Plain Item",
						OnClick = delegate
						{
							Log("Plain clicked");
						}
					},
					new Menu.Item
					{
						Label = "Plain Item",
						OnClick = delegate
						{
							Log("Plain clicked");
						}
					},
					new Menu.Item
					{
						Label = "Plain Item",
						OnClick = delegate
						{
							Log("Plain clicked");
						}
					},
					new Menu.Item
					{
						Separator = true
					},
					new Menu.Item
					{
						Label = "Plain Item",
						OnClick = delegate
						{
							Log("Plain clicked");
						}
					},
					new Menu.Item
					{
						Label = "Big Submenu",
						SubMenu = new List<Menu.Item>
						{
							new Menu.Item
							{
								Label = "Big Action 1",
								OnClick = delegate
								{
									Log("Big Action 1 clicked");
								}
							},
							new Menu.Item
							{
								Label = "Big Action 2",
								Checked = true,
								OnClick = delegate
								{
									Log("Big Action 2 clicked");
								}
							},
							new Menu.Item
							{
								Label = "Big Action 3",
								Checked = true,
								OnClick = delegate
								{
									Log("Big Action 3 clicked");
								}
							},
							new Menu.Item
							{
								Label = "Big Action 4",
								OnClick = delegate
								{
									Log("Big Action 4 clicked");
								}
							},
							new Menu.Item
							{
								Label = "Big Action 5",
								Checked = true,
								OnClick = delegate
								{
									Log("Big Action 5 clicked");
								}
							},
							new Menu.Item
							{
								Label = "Big Deep Submenu",
								SubMenu = new List<Menu.Item>
								{
									new Menu.Item
									{
										Label = "Big Deep Action 1",
										OnClick = delegate
										{
											Log("Big Deep Action 1 clicked");
										}
									},
									new Menu.Item
									{
										Label = "Big Deep Action 2",
										OnClick = delegate
										{
											Log("Big Deep Action 2 clicked");
										}
									},
									new Menu.Item
									{
										Label = "Big Deep Action 3",
										OnClick = delegate
										{
											Log("Big Deep Action 3 clicked");
										}
									},
									new Menu.Item
									{
										Label = "Big Deep Action 4",
										OnClick = delegate
										{
											Log("Big Deep Action 4 clicked");
										}
									}
								}
							}
						}
					},
					new Menu.Item
					{
						Label = "Final Item",
						OnClick = delegate
						{
							Log("Final clicked");
						}
					}
				});
			}
			Menu.Draw(popupId);
			ImGui.SameLine();
			ColorSelect.Draw(PluginName + "TitleTextColor", "TitleTextColor", ref Settings.TitleTextColor, options3);
			ImGui.SameLine();
			ColorSelect.Draw(PluginName + "PanelColor", "PanelColor", ref Settings.PanelColor, options3);
			string text2 = "";
			string text3 = "";
			for (int num = 0; num < DefaultTextColors.Count; num++)
			{
				labelStyle labelStyle = DefaultTextColors[num];
				if (num >= Settings.textColors.Count)
				{
					Settings.textColors.Add(Colors.InputText);
				}
				text3 = text3 + Settings.textColors[num].ToColorCode() + labelStyle.Name + " ";
				text2 = text2 + labelStyle.TextColor.ToColorCode() + labelStyle.Name + " ";
			}
			ColoredText text4 = new ColoredText(text2);
			ColoredText text5 = new ColoredText(text3);
			Label.Draw(text4, new Label.Options
			{
				Width = -3,
				Height = controlHeight,
				PadLeft = 3
			});
			Label.Draw(text5, new Label.Options
			{
				Width = -3,
				Height = controlHeight,
				PadLeft = 3
			});
			Label.Draw(text4, new Label.Options
			{
				Width = -3,
				Height = controlHeight,
				DrawBG = true,
				PadLeft = 3
			});
			Label.Draw(text5, new Label.Options
			{
				Width = -3,
				Height = controlHeight,
				DrawBG = true,
				PadLeft = 3
			});
			for (int num2 = 0; num2 < DefaultTextColors.Count; num2++)
			{
				if (num2 > 0)
				{
					ImGui.SameLine();
				}
				labelStyle labelStyle2 = DefaultTextColors[num2];
				System.Drawing.Color color = Settings.textColors[num2];
				ColorSelect.Draw(text + "textColorSwatch" + labelStyle2.Name, labelStyle2.Name + " Text Color", ref color, options3);
				Settings.textColors[num2] = color;
			}
			ImGui.Dummy(new Vector2(controlWidth, controlHeight));
			options4.Label = "Default";
			options4.Tooltip = new Tooltip.Options("Default Button");
			int num3 = 0;
			foreach (buttonStyle defaultButton in DefaultButtons)
			{
				options4.Color = defaultButton.BackgroundColor;
				options4.TextColor = defaultButton.TextColor;
				options4.Label = defaultButton.Name ?? "";
				options4.Tooltip = new Tooltip.Options
				{
					Lines = new List<Tooltip.Line>
					{
						new Tooltip.Title
						{
							Text = "Predefined Styles"
						},
						new Tooltip.Separator(),
						new Tooltip.DoubleLine
						{
							LeftText = "Color",
							RightText = $"{defaultButton.BackgroundColor}"
						},
						new Tooltip.DoubleLine
						{
							LeftText = "TextColor",
							RightText = $"{defaultButton.TextColor}"
						}
					}
				};
				Button.Draw(text + "button" + defaultButton.Name, options4);
				ImGui.SameLine();
				if (num3 >= Settings.buttonColors.Count)
				{
					Settings.buttonColors.Add(Colors.Button);
				}
				if (num3 >= Settings.buttonTextColors.Count)
				{
					Settings.buttonTextColors.Add(Colors.TextOnColor);
				}
				options4.Color = Settings.buttonColors[num3];
				options4.TextColor = Settings.buttonTextColors[num3];
				options4.Label = $"Button {num3}";
				options4.Tooltip = new Tooltip.Options($"Button {num3} Test");
				Button.Draw($"{text}button{num3}", options4);
				ImGui.SameLine();
				System.Drawing.Color color2 = Settings.buttonColors[num3];
				ColorSelect.Draw($"{text}button{num3}_Swatch", $"Button {num3} Color", ref color2, options3);
				Settings.buttonColors[num3] = color2;
				ImGui.SameLine();
				color2 = Settings.buttonTextColors[num3];
				ColorSelect.Draw($"{text}button{num3}_TextSwatch", $"Button {num3} Text Color", ref color2, options3);
				Settings.buttonTextColors[num3] = color2;
				if (num3 == 0)
				{
					ImGui.SameLine();
					ImGui.Dummy(new Vector2(controlWidth, controlHeight));
					ImGui.SameLine();
					Slider.Draw(PluginName + "DefaultSlider", ref Settings.PanelSliderTest, options5);
					ImGui.SameLine();
					options5.BackgroundColor = Settings.sliderColor;
					options5.TextColor = Settings.sliderTextColor;
					Slider.Draw(PluginName + "Slider", ref Settings.PanelSliderTest, options5);
					ImGui.SameLine();
					ColorSelect.Draw(text + "Slider_Swatch", "Slider Color", ref Settings.sliderColor, options3);
					ImGui.SameLine();
					ColorSelect.Draw(text + "Slider_TextSwatch", "Slider Text Color", ref Settings.sliderTextColor, options3);
				}
				num3++;
			}
			ImGui.Dummy(new Vector2(0f, 2f));
			ImGui.PopStyleVar();
			Panel.End(text);
			ImGui.Unindent(3f);
			Window.End();
		}
	}

	public static class Deferred
	{
		private static readonly Queue<Action> _queue = new Queue<Action>();

		private static readonly object _lock = new object();

		public static int Count
		{
			get
			{
				lock (_lock)
				{
					return _queue.Count;
				}
			}
		}

		public static void Enqueue(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			lock (_lock)
			{
				_queue.Enqueue(action);
			}
		}

		public static void ExecuteAll()
		{
			List<Action> list;
			lock (_lock)
			{
				if (_queue.Count == 0)
				{
					return;
				}
				list = new List<Action>(_queue);
				_queue.Clear();
			}
			foreach (Action item in list)
			{
				try
				{
					item();
				}
				catch (Exception value)
				{
					Log($"[Deferred] Error executing action: {value}");
				}
			}
		}

		public static void Clear()
		{
			lock (_lock)
			{
				_queue.Clear();
			}
		}
	}

	public class Keyboard
	{
		public enum Keys
		{
			Back = 8,
			Tab = 9,
			LineFeed = 10,
			Clear = 12,
			Return = 13,
			Enter = 13,
			ShiftKey = 16,
			ControlKey = 17,
			Menu = 18,
			Pause = 19,
			Escape = 27,
			Space = 32,
			PageUp = 33,
			Prior = 33,
			PageDown = 34,
			Next = 34,
			End = 35,
			Home = 36,
			Left = 37,
			Up = 38,
			Right = 39,
			Down = 40,
			Select = 41,
			Print = 42,
			Execute = 43,
			Snapshot = 44,
			PrintScreen = 44,
			Insert = 45,
			Delete = 46,
			Help = 47,
			D0 = 48,
			D1 = 49,
			D2 = 50,
			D3 = 51,
			D4 = 52,
			D5 = 53,
			D6 = 54,
			D7 = 55,
			D8 = 56,
			D9 = 57,
			A = 65,
			B = 66,
			C = 67,
			D = 68,
			E = 69,
			F = 70,
			G = 71,
			H = 72,
			I = 73,
			J = 74,
			K = 75,
			L = 76,
			M = 77,
			N = 78,
			O = 79,
			P = 80,
			Q = 81,
			R = 82,
			S = 83,
			T = 84,
			U = 85,
			V = 86,
			W = 87,
			X = 88,
			Y = 89,
			Z = 90,
			NumPad0 = 96,
			NumPad1 = 97,
			NumPad2 = 98,
			NumPad3 = 99,
			NumPad4 = 100,
			NumPad5 = 101,
			NumPad6 = 102,
			NumPad7 = 103,
			NumPad8 = 104,
			NumPad9 = 105,
			Multiply = 106,
			Add = 107,
			Separator = 108,
			Subtract = 109,
			Decimal = 110,
			Divide = 111,
			F1 = 112,
			F2 = 113,
			F3 = 114,
			F4 = 115,
			F5 = 116,
			F6 = 117,
			F7 = 118,
			F8 = 119,
			F9 = 120,
			F10 = 121,
			F11 = 122,
			F12 = 123,
			F13 = 124,
			F14 = 125,
			F15 = 126,
			F16 = 127,
			F17 = 128,
			F18 = 129,
			F19 = 130,
			F20 = 131,
			F21 = 132,
			F22 = 133,
			F23 = 134,
			F24 = 135,
			LShiftKey = 160,
			RShiftKey = 161,
			LControlKey = 162,
			RControlKey = 163,
			LMenu = 164,
			RMenu = 165
		}

		private const int KEYEVENTF_EXTENDEDKEY = 1;

		private const int KEYEVENTF_KEYUP = 2;

		[DllImport("user32.dll")]
		private static extern short GetAsyncKeyState(int vKey);

		public static bool IsKeyDown(Keys key)
		{
			return (GetAsyncKeyState((int)key) & 0x8000) != 0;
		}

		[DllImport("user32.dll")]
		private static extern uint keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

		private static bool IsExtendedKey(Keys key)
		{
			if (key != Keys.Up && key != Keys.Down && key != Keys.Left && key != Keys.Right && key != Keys.Insert && key != Keys.Delete && key != Keys.Home && key != Keys.End && key != Keys.PageUp)
			{
				return key == Keys.PageDown;
			}
			return true;
		}

		public static void KeyDown(Keys key)
		{
			if (!IsKeyDown(key))
			{
				keybd_event((byte)key, 0, 1, 0);
			}
		}

		public static void KeyUp(Keys key)
		{
			int dwFlags = (IsExtendedKey(key) ? 3 : 2);
			if (IsKeyDown(key))
			{
				keybd_event((byte)key, 0, dwFlags, 0);
			}
			if (key == Keys.LShiftKey || key == Keys.RShiftKey || key == Keys.ShiftKey)
			{
				keybd_event(160, 0, 2, 0);
				keybd_event(161, 0, 2, 0);
				keybd_event(16, 0, 2, 0);
			}
			if (key == Keys.LControlKey || key == Keys.RControlKey || key == Keys.ControlKey)
			{
				keybd_event(162, 0, 2, 0);
				keybd_event(163, 0, 2, 0);
				keybd_event(17, 0, 2, 0);
			}
			if (key == Keys.LMenu || key == Keys.RMenu || key == Keys.Menu)
			{
				keybd_event(164, 0, 2, 0);
				keybd_event(165, 0, 2, 0);
				keybd_event(18, 0, 2, 0);
			}
		}

		public static void ReleaseAllKeys()
		{
			foreach (Keys value in Enum.GetValues(typeof(Keys)))
			{
				KeyUp(value);
			}
		}

		public static void SendKeys(string text)
		{
			foreach (char c in text)
			{
				bool flag = char.IsUpper(c) || char.IsPunctuation(c) || char.IsSymbol(c);
				Keys key;
				if (char.IsLetterOrDigit(c))
				{
					key = (Keys)char.ToUpper(c);
				}
				else if (c != '\n')
				{
					if (c != '\r')
					{
						if (c != ' ')
						{
							continue;
						}
						key = Keys.Space;
					}
					else
					{
						key = Keys.Return;
					}
				}
				else
				{
					key = Keys.Return;
				}
				if (flag)
				{
					KeyDown(Keys.ShiftKey);
				}
				KeyDown(key);
				KeyUp(key);
				if (flag)
				{
					KeyUp(Keys.ShiftKey);
				}
			}
		}

		public static void SendKeyCombo(params Keys[] keys)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				KeyDown(keys[i]);
			}
			for (int num = keys.Length - 1; num >= 0; num--)
			{
				KeyUp(keys[num]);
			}
		}
	}

	public class Mouse
	{
		public enum MouseEvents
		{
			LeftDown = 2,
			LeftUp = 4,
			MiddleDown = 32,
			MiddleUp = 64,
			Move = 1,
			Absolute = 32768,
			RightDown = 8,
			RightUp = 16
		}

		public enum Buttons
		{
			Left = 1,
			Right = 2,
			Middle = 4,
			XButton1 = 8,
			XButton2 = 0x10
		}

		[DllImport("user32.dll")]
		public static extern bool BlockInput(bool block);

		[DllImport("user32.dll")]
		public static extern bool GetCursorPos(out Point point);

		public static Point GetCursorPosition()
		{
			GetCursorPos(out var point);
			return point;
		}

		[DllImport("user32.dll")]
		public static extern bool SetCursorPos(int x, int y);

		public static void moveMouse(Vector2 pos)
		{
			SetCursorPos((int)pos.X, (int)pos.Y);
		}

		public static void moveMouse(Point pos)
		{
			SetCursorPos(pos.X, pos.Y);
		}

		[DllImport("user32.dll")]
		private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

		public static void LeftDown()
		{
			mouse_event(2, 0, 0, 0, 0);
		}

		public static void LeftUp()
		{
			mouse_event(4, 0, 0, 0, 0);
		}

		public static void RightDown()
		{
			mouse_event(8, 0, 0, 0, 0);
		}

		public static void RightUp()
		{
			mouse_event(16, 0, 0, 0, 0);
		}

		[DllImport("user32.dll")]
		private static extern short GetAsyncKeyState(int vKey);

		public static bool IsLeftButtonDown()
		{
			return (GetAsyncKeyState(1) & 0x8000) != 0;
		}

		public static bool IsLeftButtonPressed()
		{
			return (GetAsyncKeyState(1) & 1) != 0;
		}

		public static bool IsRightButtonDown()
		{
			return (GetAsyncKeyState(2) & 0x8000) != 0;
		}

		public static bool IsRightButtonPressed()
		{
			return (GetAsyncKeyState(2) & 1) != 0;
		}

		public static bool IsMiddleButtonDown()
		{
			return (GetAsyncKeyState(4) & 0x8000) != 0;
		}

		public static bool IsAnyButtonDown()
		{
			if ((GetAsyncKeyState(1) & 0x8000) == 0 && (GetAsyncKeyState(2) & 0x8000) == 0)
			{
				return (GetAsyncKeyState(4) & 0x8000) != 0;
			}
			return true;
		}

		public static bool IsMouseButtonDown(Buttons button)
		{
			return (GetAsyncKeyState((int)button) & 0x8000) != 0;
		}
	}

	private const float CameraAngle = (float)Math.PI * 43f / 200f;

	private static readonly float CameraAngleCos = MathF.Cos((float)Math.PI * 43f / 200f);

	private static readonly float CameraAngleSin = MathF.Sin((float)Math.PI * 43f / 200f);

	public const int TileToGridConversion = 23;

	public const int TileToWorldConversion = 250;

	public const float WorldToGridConversion = 0.092f;

	private const float ScaleFactor = 2.3633678f;

	private static DXTSettings? _settings;

	public static LargeMapUiElement? LargeMap => Core.States.InGameStateObject.GameUi.LargeMap;

	public static MapUiElement? MiniMap => Core.States.InGameStateObject.GameUi.MiniMap;

	public static bool IsMinimapVisible
	{
		get
		{
			if (MiniMap != null)
			{
				return MiniMap.IsVisible;
			}
			return false;
		}
	}

	public static bool IsLargeMapVisible
	{
		get
		{
			if (LargeMap != null)
			{
				return LargeMap.IsVisible;
			}
			return false;
		}
	}

	public static float ActiveMapScale
	{
		get
		{
			if (IsLargeMapVisible)
			{
				LargeMapUiElement? largeMap = LargeMap;
                if (largeMap != null) {
                    float diagonal = largeMap.Size.Length();
                    // Radar default multiplier is 0.1738f. 
                    // Formula: (Diagonal / 240) * Multiplier * Zoom
                    return (diagonal / 240f) * 0.1738f * largeMap.Zoom;
                }
				return 1f;
			}
			MapUiElement? miniMap = MiniMap;
            if (miniMap != null) {
                float diagonal = miniMap.Size.Length();
                return (diagonal / 240f) * miniMap.Zoom;
            }
			return 1f;
		}
	}

	public static Vector2 ActiveMapCenter
	{
		get
		{
			if (IsLargeMapVisible)
			{
				Vector2? vector = LargeMap?.Center;
				if (vector.HasValue)
				{
					if (Settings.LargeMapCenterFix)
					{
						vector = new Vector2(vector.Value.X * Settings.LargeMapCenterMultiplierX, vector.Value.Y);
					}
					return (vector + LargeMap?.Shift + LargeMap?.DefaultShift) ?? Vector2.Zero;
				}
				return Vector2.Zero;
			}
			if (IsMinimapVisible)
			{
				Vector2? vector2 = MiniMap?.Postion;
				MapUiElement? miniMap = MiniMap;
				return (vector2 + ((miniMap != null) ? new Vector2?(miniMap.Size / 2f) : ((Vector2?)null)) + MiniMap?.DefaultShift + MiniMap?.Shift) ?? Vector2.Zero;
			}
			return Vector2.Zero;
		}
	}

	public static Vector2 ActiveMapSize
	{
		get
		{
			if (IsLargeMapVisible)
			{
				return LargeMap?.Size ?? Vector2.Zero;
			}
			if (IsMinimapVisible)
			{
				return MiniMap?.Size ?? Vector2.Zero;
			}
			return Vector2.Zero;
		}
	}

	public static Vector2 ActiveMapPosition
	{
		get
		{
			if (IsLargeMapVisible)
			{
				return LargeMap?.Postion ?? Vector2.Zero;
			}
			if (IsMinimapVisible)
			{
				return MiniMap?.Postion ?? Vector2.Zero;
			}
			return Vector2.Zero;
		}
	}

	public static string PluginName { get; private set; } = "PluginName";

	public static string PluginDirectory { get; private set; } = "PluginDirectory";

	public static DXTSettings Settings
	{
		get
		{
			if (_settings != null)
			{
				return _settings;
			}
			return GetSettingsWithCallerInfo("Settings", "D:\\Codebase\\CommonLibraries\\DieselExileTools_Common\\DXTCommon.cs", 17);
		}
		set
		{
			_settings = value;
		}
	}

	private static FloatingToolbar.Options? ToolbarOptions { get; set; }

	public static Action<int, int>? LogHeader { get; set; }

	public static void InitialiseGameHelper()
	{
	}

	public static Vector2 GridToMap(float gridPosX, float gridPosY, float worldPosZ, bool round = false)
	{
		worldPosZ *= 0.092f;
		Vector2 result = ActiveMapCenter + ActiveMapScale * new Vector2((gridPosX - gridPosY) * CameraAngleCos, (worldPosZ - (gridPosX + gridPosY)) * CameraAngleSin);
		if (round)
		{
			return new Vector2(MathF.Round(result.X), MathF.Round(result.Y));
		}
		return result;
	}

	public static float GridRadiusToMap(float gridRadius)
	{
		return Math.Abs(ActiveMapScale * (gridRadius * CameraAngleCos));
	}

	private static DXTSettings GetSettingsWithCallerInfo([CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
	{
		Log($"WARNING: DXT.Settings accessed before initialization. by: {memberName} in {Path.GetFileName(filePath)}:{lineNumber}", whenVisibleOnly: false);
		return new DXTSettings();
	}

	public static void AddToolbarButtons(IEnumerable<FloatingToolbar.Tool> tools)
	{
		if (ToolbarOptions == null)
		{
			InitialiseToolbar();
		}
		int num = Math.Max(0, ToolbarOptions.Tools.Count - 1);
		foreach (FloatingToolbar.Tool tool in tools)
		{
			ToolbarOptions.Tools.Insert(num, tool);
			num++;
		}
	}

	public static void Initialise(Config config)
	{
		PluginName = config.PluginName;
		PluginDirectory = config.PluginDirectory;
		Settings = config.Settings;
		InitialiseToolbar();
		Monitor("DXT", "Initialised", true, whenVisibleOnly: false, "D:\\Codebase\\CommonLibraries\\DieselExileTools_Common\\DXTCommon.cs", 59, "Initialise");
		Log("DieselExileTools for " + PluginName + " initialized", whenVisibleOnly: false);
	}

	public static void InitialiseToolbar()
	{
		if (ToolbarOptions == null)
		{
			ToolbarOptions = new FloatingToolbar.Options
			{
				Tools = new List<FloatingToolbar.Tool>
				{
					new FloatingToolbar.Label
					{
						Text = PluginName + " DXT"
					},
					new FloatingToolbar.Button
					{
						Label = "Log",
						SetChecked = delegate(bool state)
						{
							Settings.ShowLog = state;
						},
						GetChecked = () => Settings.ShowLog
					},
					new FloatingToolbar.Button
					{
						Label = "Monitor",
						SetChecked = delegate(bool state)
						{
							Settings.ShowMonitor = state;
						},
						GetChecked = () => Settings.ShowMonitor
					}
				}
			};
			ToolbarOptions.Tools.Add(new FloatingToolbar.Button
			{
				Label = "Style",
				SetChecked = delegate(bool state)
				{
					Settings.ShowStyleDemo = state;
				},
				GetChecked = () => Settings.ShowStyleDemo
			});
		}
		ToolbarOptions.Tools.Add(new FloatingToolbar.Button
		{
			Width = 20,
			Height = 20,
			Color = Colors.ButtonClose,
			OnClick = delegate
			{
				Settings.ShowTools = false;
			},
			Tooltip = new Tooltip.Options("Close Toolbar")
		});
	}

	public static void Tick()
	{
		Deferred.ExecuteAll();
	}

	public static void Render()
	{
		if (Settings.ShowTools)
		{
			if (ToolbarOptions != null)
			{
				FloatingToolbar.Draw(PluginName + "DXTToolbar", ToolbarOptions);
			}
			MonitorImpl.Render();
			LoggerImpl.Render();
			StyleDemo.Render();
		}
	}

	public static bool CheckboxWithTooltip(string label, string tooltip, ref bool value)
	{
		bool result = ImGui.Checkbox(label, ref value);
		if (ImGui.IsItemHovered())
		{
			ImGui.BeginTooltip();
			ImGui.Text(tooltip);
			ImGui.EndTooltip();
		}
		return result;
	}

	public static void Log(string message, bool whenVisibleOnly = true)
	{
		if (!whenVisibleOnly || Settings.ShowLog)
		{
			LoggerImpl.AddLogEntry(message);
		}
	}

	public static void ClearLog()
	{
		LoggerImpl.Clear();
	}

	public static void Monitor(string category, string name, object variable, bool whenVisibleOnly = true, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
	{
		if (!whenVisibleOnly || Settings.ShowMonitor)
		{
			MonitorImpl.AddMonitoredVariable(category, name, variable, file, line, member);
		}
	}
}
