using Ink_Canvas.Plugins;
using System;
using System.Windows;

namespace SamplePlugin
{
    public class SamplePlugin : PluginBase
    {
        public override string Id => "SamplePlugin";
        public override string Name => "示例插件";
        public override string Version => "1.0.0";
        public override string Description => "一个功能丰富的示例插件，演示如何在设置页面中添加功能按钮";
        public override string Author => "示例作者";
        public override int Order => 0;

        private IInkCanvasService _inkCanvasService;
        private IAppRestartService _appRestartService;
        private Views.SettingsView _settingsView;

        public override void Initialize(IPluginHost host)
        {
            base.Initialize(host);
            Log(string.Format("{0} 已初始化", Name));

            _inkCanvasService = GetService<IInkCanvasService>();
            if (_inkCanvasService != null)
            {
                Log("已获取 InkCanvas 服务");
            }

            _appRestartService = GetService<IAppRestartService>();
            if (_appRestartService != null)
            {
                Log("已获取重启服务，当前运行状态: " + (_appRestartService.IsRunningAsAdmin ? "管理员" : "非管理员"));
            }

            _settingsView = new Views.SettingsView(_inkCanvasService, _appRestartService, host);
        }

        public override void Shutdown()
        {
            Log(string.Format("{0} 已关闭", Name));
        }

        public override object GetMainView()
        {
            return null;
        }

        public override object GetSettingsView()
        {
            return _settingsView;
        }
    }
}
