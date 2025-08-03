using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using VideoConversionApp.ViewModels;

namespace VideoConversionApp.Views;

public partial class RenderQueueView : UserControl
{
    public RenderQueueView()
    {
        InitializeComponent();
    }

    private void Visual_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        //(DataContext as RenderQueueViewModel)?.SyncRenderQueue();
    }
}