using MudBlazor;

namespace Buenaventura.Client.Layout;

public static class Themes
{
    public static PaletteLight Light()
    {
        return new PaletteLight()
        {
            Black = "#110e2d",
            AppbarText = "#FFFFFF",
            AppbarBackground = "#9c6644",
            DrawerBackground = "#F0E7E1",
            DrawerText = "#4C3A2D",
            DrawerIcon = "#3A2920",
            GrayLight = "#e8e8e8",
            GrayLighter = "#f9f9f9",
            Primary = "#9c6644",
            Secondary = "#447a9c",
            Tertiary = "#eed9c4",
            Info = "#3a88d1",
            Success = "#5a8f55",
            Warning = "#d8a039",
            Error = "#c24a4a",
            Dark = "2b2b2b",
            Surface = "faf7f4",
            Background = "#f3ece7",
            ActionDefault = "#ffffff",
        };
    }
    
    public static PaletteDark Dark()
    {
        return new PaletteDark
        {
            Primary = "#9c6644",
            Secondary = "#447a9c",
            Tertiary = "#d4a373",
            Info = "#4f9dde",

            Surface = "#2c2c2c",
            Background = "#1a1412",
            BackgroundGray = "#151521",
            AppbarText = "#ffffff",
            AppbarBackground = "#9c6644",
            DrawerBackground = "#231b18",
            ActionDefault = "#ffffff",
            ActionDisabled = "#9999994d",
            ActionDisabledBackground = "#605f6d4d",
            TextPrimary = "#b2b0bf",
            TextSecondary = "#92929f",
            TextDisabled = "#ffffff33",
            DrawerIcon = "#f1eae3",
            DrawerText = "#c9bfb6",
            GrayLight = "#2a2833",
            GrayLighter = "#1e1e2d",
            Success = "#5a8f55",
            Warning = "#e1b94c",
            Error = "#b14e4e",
            LinesDefault = "#33323e",
            TableLines = "#33323e",
            Divider = "#292838",
            OverlayLight = "#1e1e2d80",
        };
    }
}