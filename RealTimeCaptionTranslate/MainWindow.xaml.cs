using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RealTimeCaptionTranslate
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private RealTimeCaptionCapture Capture
        {
            get; set;
        }

        public MainWindow()
        {
            InitializeComponent();
            Capture = new RealTimeCaptionCapture();
            Capture.Start();
            Capture.TextReceived += OnTextReceived;
        }

        private void OnTextReceived(object sender, TextReceivedEventArgs args)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(()=> { Caption.Text = args.Text; }));
        }
    }
}
