using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using VideoConversionApp.ViewModels;
using VideoConversionApp.ViewModels.Components;

namespace VideoConversionApp.Views;

public partial class MediaSelectionView : UserControl
{
    
    private MediaSelectionViewModel? MediaSelectionViewModel => DataContext as MediaSelectionViewModel;
    public MediaSelectionView()
    {
        InitializeComponent();
    }

}