using System.Windows;
using Pace.Engineer.App.ViewModels;

namespace Pace.Engineer.App;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}