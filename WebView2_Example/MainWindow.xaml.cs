using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using WebView2_Example.CES;

namespace WebView2_Example
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string template = null;

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
                webView.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
                webView.CoreWebView2.Navigate(uri);
            }
        }

        private void CoreWebView2_DOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            webView.CoreWebView2.ExecuteScriptAsync($@"
                        let module = new emailSendingModule();
                        module.emailTemplateEditor('{template ?? "Hello Tiny MCE!"}');
                        window.mcemodule = module;
            ");
            template = null;
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Navigate(addressBar.Text);
            }
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                template = File.ReadAllText(openFileDialog.FileName);
                if (webView != null && webView.CoreWebView2 != null)
                {
                    _ = InitializeAsync();
                }
            }
        }

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (webView != null && webView.CoreWebView2 != null)
            {
                var text = await webView.CoreWebView2.ExecuteScriptAsync("window.mcemodule.getText()");
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                if (text?.Length > 0 && saveFileDialog.ShowDialog() == true)
                {
                    var cleaner = new CorrespondenceTemplateCleaner();
                    var cleaned = cleaner.CleanMail(processStringFromJS(text));
                    File.WriteAllText(saveFileDialog.FileName, cleaned);
                }
            }
        }

        private string processStringFromJS(string s)
        {
            var unescaped = Regex.Unescape(s);
            unescaped = unescaped.StartsWith("\"") ? unescaped.Substring(1) : unescaped;
            unescaped = unescaped.EndsWith("\"") ? unescaped.Substring(0, unescaped.Length - 1) : unescaped;
            return unescaped;
        }
    }
}
