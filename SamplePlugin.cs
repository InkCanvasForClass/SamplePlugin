using Ink_Canvas.Controls;
using Ink_Canvas.Plugins;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SamplePlugin
{
    /// <summary>
    /// 示例插件 - 演示如何开发 Ink Canvas 插件，包括工具栏组件的动态注册。
    /// </summary>
    [PluginEntrance]
    public class SamplePlugin : PluginBase
    {
        // 元数据（Id/Name/Version/Description/Author）从 manifest.json 自动读取，无需在代码中重复定义

        private IInkCanvasService _inkCanvasService;
        private IAppRestartService _appRestartService;
        private Views.SettingsView _settingsView;
        private FrameworkElement _sampleButtonPopupContent;

        public override void Initialize(IPluginHost host, IServiceCollection services)
        {
            base.Initialize(host, services);
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

            // 注册示例工具栏组件
            RegisterSampleButton(host);
            RegisterSampleCustomControl(host);
        }

        private FrameworkElement CreateMenuContent(string menuStyle)
        {
            var innerText = new TextBlock
            {
                Text = "这是示例按钮的菜单内容",
                Margin = new Thickness(16),
                FontSize = 14
            };

            switch (menuStyle)
            {
                case "PopupTabShellContent":
                    var tabShell = new PopupTabShellContent
                    {
                        InnerContent = innerText
                    };
                    tabShell.TabBar.Tabs.Add(new PopupTabItem { Header = "Tab 1" });
                    tabShell.TabBar.Tabs.Add(new PopupTabItem { Header = "Tab 2" });
                    tabShell.TabBar.SelectedIndex = 0;
                    return tabShell;
                default:
                    return new PopupShellContent
                    {
                        Title = "示例菜单",
                        InnerContent = innerText
                    };
            }
        }

        /// <summary>
        /// 注册示例按钮 - 使用 ToolbarImageButton 的普通组件示例
        /// </summary>
        private void RegisterSampleButton(IPluginHost host)
        {
            var itemInfo = new PluginToolbarItemInfo
            {
                Id = "sample.button",
                DisplayName = "示例按钮",
                Description = "示例普通组件，点击打开菜单弹窗",
                IconGeometry = "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z",
                ViewFactory = () =>
                {
                    var btn = new ToolbarImageButton
                    {
                        Label = "示例按钮",
                        Tag = "ToolbarRegistryInjected"
                    };
                    // 设置图标
                    btn.Icon.Geometry = Geometry.Parse("M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z");
                    return btn;
                },
                PopupContentFactory = () =>
                {
                    _sampleButtonPopupContent = CreateMenuContent("PopupShellContent");
                    return _sampleButtonPopupContent;
                },
                ApplyOrientation = (view, orientation) =>
                {
                    if (view is ToolbarImageButton btn)
                    {
                        btn.ApplyOrientation(orientation == Orientation.Vertical);
                    }
                },
                ApplySettings = (view, settings) =>
                {
                    if (settings == null) return;
                    if (!settings.TryGetValue("menuStyle", out var val)) return;

                    var menuStyle = val?.ToString() ?? "PopupShellContent";

                    // 通过逻辑树找到父级 Popup 并替换内容
                    var popup = LogicalTreeHelper.GetParent(_sampleButtonPopupContent) as Popup;
                    if (popup != null)
                    {
                        var newContent = CreateMenuContent(menuStyle);
                        popup.Child = newContent;
                        _sampleButtonPopupContent = newContent;

                        // 重新绑定关闭按钮
                        if (newContent is PopupShellContent shell)
                            shell.CloseButtonControl.Click += (s, e) => popup.IsOpen = false;
                        else if (newContent is PopupTabShellContent tabShell)
                            tabShell.CloseButtonControl.Click += (s, e) => popup.IsOpen = false;
                    }
                },
                CustomSettings = new List<PluginToolbarSettingInfo>
                {
                    new PluginToolbarSettingInfo
                    {
                        Key = "menuStyle",
                        DisplayName = "菜单样式",
                        Description = "选择弹窗的菜单样式",
                        Type = PluginToolbarSettingType.ComboBox,
                        Options = new List<string> { "PopupShellContent", "PopupTabShellContent" },
                        DefaultValue = "PopupShellContent"
                    }
                }
            };

            host.RegisterToolbarItem(itemInfo);
        }

        /// <summary>
        /// 注册示例自定义控件 - 在工具栏中嵌入自定义控件
        /// </summary>
        private void RegisterSampleCustomControl(IPluginHost host)
        {
            var itemInfo = new PluginToolbarItemInfo
            {
                Id = "sample.customControl",
                DisplayName = "示例自定义控件",
                Description = "示例自定义控件插件，支持滑块、开关、复选框、下拉框等控制类型",
                IconGeometry = "M3 5h18v2H3V5zm0 6h18v2H3v-2zm0 6h12v2H3v-2z",
                ViewFactory = () =>
                {
                    var border = new Border
                    {
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(6, 2, 6, 2)
                    };
                    border.SetResourceReference(Border.BackgroundProperty, "FloatBarBackground");

                    var panel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    var label = new TextBlock
                    {
                        Text = "示例控件",
                        FontSize = 12,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 6, 0)
                    };
                    label.SetResourceReference(TextBlock.ForegroundProperty, "FloatBarForeground");
                    panel.Children.Add(label);

                    // 默认显示滑块
                    var slider = new Slider
                    {
                        Width = 80,
                        Minimum = 0,
                        Maximum = 100,
                        Value = 50,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    panel.Children.Add(slider);

                    border.Child = panel;
                    return border;
                },
                ApplyOrientation = (view, orientation) =>
                {
                    // 自定义控件支持方向切换
                },
                ApplySettings = (view, settings) =>
                {
                    // 根据控制类型设置渲染不同的控件
                    if (view is Border border && border.Child is StackPanel panel && panel.Children.Count > 1)
                    {
                        string controlType = null;
                        if (settings != null && settings.TryGetValue("controlType", out var val))
                            controlType = val?.ToString();

                        if (string.IsNullOrEmpty(controlType)) return;

                        // 移除除标签外的所有子控件
                        while (panel.Children.Count > 1)
                            panel.Children.RemoveAt(1);

                        FrameworkElement control = controlType switch
                        {
                            "ToggleSwitch" => new iNKORE.UI.WPF.Modern.Controls.ToggleSwitch
                            {
                                MinWidth = 0,
                                OnContent = "",
                                OffContent = "",
                                VerticalAlignment = VerticalAlignment.Center
                            },
                            "CheckBox" => new CheckBox
                            {
                                VerticalAlignment = VerticalAlignment.Center,
                                IsChecked = true
                            },
                            "ComboBox" => new ComboBox
                            {
                                VerticalAlignment = VerticalAlignment.Center,
                                Width = 80
                            },
                            _ => new Slider
                            {
                                Width = 80,
                                Minimum = 0,
                                Maximum = 100,
                                Value = 50,
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        };
                        panel.Children.Add(control);
                    }
                },
                CustomSettings = new List<PluginToolbarSettingInfo>
                {
                    new PluginToolbarSettingInfo
                    {
                        Key = "controlType",
                        DisplayName = "控制类型",
                        Description = "选择自定义控件的类型",
                        Type = PluginToolbarSettingType.ComboBox,
                        Options = new List<string> { "Slider", "ToggleSwitch", "CheckBox", "ComboBox" },
                        DefaultValue = "Slider"
                    }
                }
            };

            host.RegisterToolbarItem(itemInfo);
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
