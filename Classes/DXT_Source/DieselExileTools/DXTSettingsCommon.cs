using System.Collections.Generic;
using System.Drawing;

namespace DieselExileTools;

public class DXTSettingsCommon
{
	public bool ShowTools;

	public bool ShowMonitor;

	public Dictionary<string, bool> MonitorPanelsCollapsed = new Dictionary<string, bool>();

	public bool ShowLog;

	public bool ShowStyleDemo;

	public List<Color> buttonColors = new List<Color>
	{
		DXT.Colors.ControlRed,
		DXT.Colors.ControlOrange,
		DXT.Colors.ControlAmber,
		DXT.Colors.ControlYellow,
		DXT.Colors.ControlLime,
		DXT.Colors.ControlGreen,
		DXT.Colors.ControlEmerald,
		DXT.Colors.ControlTeal,
		DXT.Colors.ControlCyan,
		DXT.Colors.ControlSky,
		DXT.Colors.ControlBlue,
		DXT.Colors.ControlIndigo,
		DXT.Colors.ControlViolet,
		DXT.Colors.ControlPurple,
		DXT.Colors.ControlFuchsia,
		DXT.Colors.ControlPink,
		DXT.Colors.ControlRose,
		DXT.Colors.ControlSlate,
		DXT.Colors.ControlGray,
		DXT.Colors.ControlZinc,
		DXT.Colors.ControlNeutral,
		DXT.Colors.ControlStone,
		DXT.Colors.Button,
		DXT.Colors.ButtonChecked
	};

	public List<Color> buttonTextColors = new List<Color> { DXT.Colors.TextonControl };

	public List<Color> textColors = new List<Color>
	{
		DXT.Colors.TextRed,
		DXT.Colors.TextOrange,
		DXT.Colors.TextAmber,
		DXT.Colors.TextYellow,
		DXT.Colors.TextLime,
		DXT.Colors.TextGreen,
		DXT.Colors.TextEmerald,
		DXT.Colors.TextTeal,
		DXT.Colors.TextCyan,
		DXT.Colors.TextSky,
		DXT.Colors.TextBlue,
		DXT.Colors.TextIndigo,
		DXT.Colors.TextViolet,
		DXT.Colors.TextPurple,
		DXT.Colors.TextFuchsia,
		DXT.Colors.TextPink,
		DXT.Colors.TextRose,
		DXT.Colors.TextSlate,
		DXT.Colors.TextStone,
		DXT.Colors.Text,
		DXT.Colors.InputText
	};

	public Color[] TextColors = new Color[10]
	{
		DXT.Colors.TextonControl,
		DXT.Colors.TextonControl,
		DXT.Colors.TextonControl,
		DXT.Colors.TextonControl,
		DXT.Colors.TextonControl,
		DXT.Colors.TextonControl,
		DXT.Colors.TextonControl,
		DXT.Colors.TextonControl,
		DXT.Colors.TextonControl,
		DXT.Colors.TextonControl
	};

	public Color TitleTextColor = DXT.Colors.TextonControl;

	public Color PanelColor = DXT.Colors.Panel;

	public Color sliderColor = DXT.Colors.Input;

	public Color sliderTextColor = DXT.Colors.InputText;

	public int PanelSliderTest = 50;
}
