using Microsoft.Win32;
using ScaleIndicator;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViewModels;

namespace MedtronicLogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            IntializeEvents();
        }

        public void IntializeEvents()
        {
            if (DataContext is LogViewModel logViewModel)
            {
                logViewModel.TimelineControl = TimeLineGrid;
            }
        }
    }
}