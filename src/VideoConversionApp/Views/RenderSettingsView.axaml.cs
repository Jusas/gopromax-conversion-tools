using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using VideoConversionApp.ViewModels;

namespace VideoConversionApp.Views;

public partial class RenderSettingsView : UserControl
{
    public RenderSettingsView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        (DataContext as RenderSettingsViewModel)?.RefreshCodecLists();
    }
}