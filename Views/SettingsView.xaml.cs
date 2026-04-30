using Ink_Canvas.Plugins;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SamplePlugin.Views
{
    public partial class SettingsView : UserControl
    {
        private IInkCanvasService _inkCanvasService;
        private IPluginHost _host;

        public SettingsView(IInkCanvasService inkCanvasService, IPluginHost host)
        {
            InitializeComponent();
            _inkCanvasService = inkCanvasService;
            _host = host;
            
            OpenWhiteboardButton.Click += OpenWhiteboardButton_Click;
            CloseWhiteboardButton.Click += CloseWhiteboardButton_Click;
        }

        private void OpenWhiteboardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_inkCanvasService == null)
            {
                UpdateStatus("❌ 无法获取白板服务", Colors.Red);
                _host?.LogError("无法获取白板服务");
                return;
            }

            try
            {
                UpdateStatus("⏳ 正在打开白板...", Colors.Orange);
                _inkCanvasService.OpenWhiteboard();
                UpdateStatus("✅ 白板已成功打开！", Colors.Green);
                _host?.Log("白板已通过插件打开");
            }
            catch (System.Exception ex)
            {
                UpdateStatus(string.Format("❌ 打开白板失败: {0}", ex.Message), Colors.Red);
                _host?.LogError("打开白板失败", ex);
            }
        }

        private void CloseWhiteboardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_inkCanvasService == null)
            {
                UpdateStatus("❌ 无法获取白板服务", Colors.Red);
                _host?.LogError("无法获取白板服务");
                return;
            }

            try
            {
                UpdateStatus("⏳ 正在关闭白板...", Colors.Orange);
                _inkCanvasService.CloseWhiteboard();
                UpdateStatus("✅ 白板已成功关闭！", Colors.Green);
                _host?.Log("白板已通过插件关闭");
            }
            catch (System.Exception ex)
            {
                UpdateStatus(string.Format("❌ 关闭白板失败: {0}", ex.Message), Colors.Red);
                _host?.LogError("关闭白板失败", ex);
            }
        }

        private void UpdateStatus(string text, Color color)
        {
            StatusText.Text = text;
            StatusText.Foreground = new SolidColorBrush(color);
        }
    }
}
