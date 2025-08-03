using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;

namespace VideoConversionApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        Title = "MAX Video Converter - v" + GetType().Assembly.GetName().Version;
        InitializeComponent();
    }

}