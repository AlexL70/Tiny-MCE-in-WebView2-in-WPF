using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace WebView2_Example
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace('\\', '/');
            var uri = $"file:///{path}/Editor.html";
            addressBar.Text = uri;
            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Navigate(uri);
            }
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Navigate(addressBar.Text);
            }
        }
    }
}
