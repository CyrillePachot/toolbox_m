using System.Windows.Media;

namespace toolbox.Utilities;

public static class Colors
{
    public static readonly Brush BgColorEverWhite = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff"));
    public static readonly Brush BgColorEverBlack = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000"));
    public static readonly Brush BgColorTransparent = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#transparent"));
    public static readonly Brush BgColorCurrent = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#currentColor"));
    public static readonly Brush BgColorPrimary = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27509b"));
    public static readonly Brush BgColorAccent = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0077db"));
    public static readonly Brush BgColorInfo = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4271D6"));
    public static readonly Brush BgColorWarning = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9520"));
    public static readonly Brush BgColorDanger = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC0033"));
    public static readonly Brush BgColorSuccess = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0C8A0A"));
    public static readonly Brush BgColorGrey1 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fafafa"));
    public static readonly Brush BgColorGrey2 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f5f5f5"));
    public static readonly Brush BgColorGrey3 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#eeeeee"));
    public static readonly Brush BgColorGrey4 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e0e0e0"));
    public static readonly Brush BgColorGrey5 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#bdbdbd"));
    public static readonly Brush BgColorData1 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27509B"));
    public static readonly Brush BgColorData2 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fdea33"));
    public static readonly Brush BgColorData3 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#45b9a5"));
    public static readonly Brush BgColorData4 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff9100"));
    public static readonly Brush BgColorData5 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#40aafa"));
    public static readonly Brush BgColorData6 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6f2cac"));
}