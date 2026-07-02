using MudBlazor;

namespace CrossCutting.MudThemes;

public class Frontend
{
    public static readonly MudTheme FrontendThemeDesktop = new()
    {
        Palette = new()
        {
            Primary = "rgb(0, 128, 246)",
            Secondary = "rgb(255, 64, 129)",
            AppbarBackground = "#ffffff",
            Background = "#f8f7fa",
        },
    };
}