using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using VideoConversionApp.ViewModels;

namespace VideoConversionApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        Title = "MAX Video Converter - v" + GetType().Assembly.GetName().Version;
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // Crude, but it shall do for now.
        // Should have a nicer messagebox for errors, and generally better handling of this, but
        // again right now, it shall suffice.
        
        var vm = DataContext as MainWindowViewModel;
        if (vm?.MisconfigurationMessages.Count > 0)
        {
            var errorPopup = new ErrorPopupWindow();
            errorPopup.MessageText.Text = string.Join("\n", vm.MisconfigurationMessages);
            errorPopup.Show(this);
        }

        Console.Write("Loaded");
    }
}