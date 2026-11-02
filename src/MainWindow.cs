using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace RateConverter
{
    class CurrencyRow
    {
        public string Code;
        public Border Root;
        public TextBox Box;
        public TextBlock RateText;
        public Button RemoveBtn;
    }

    public class MainWindow : Window
    {
        readonly AppSettings settings;
        RateCache cache;
        string lastError;
        bool fetching;
        bool suppress;

        readonly List<CurrencyRow> rows = new List<CurrencyRow>();
        StackPanel rowsPanel;
        TextBlock statusText;
        Button refreshBtn;
        ToggleButton pinBtn;
        RotateTransform refreshSpin;

        public MainWindow()
        {
            settings = Store.Load<AppSettings>("settings.json") ?? new AppSettings();
            if (settings.Currencies == null || settings.Currencies.Count == 0)
                settings.Currencies = new List<string>(Currencies.Defaults);
            if (string.IsNullOrEmpty(settings.BaseCode) || !settings.Currencies.Contains(settings.BaseCode))
            {
                settings.BaseCode = settings.Currencies[0];
                settings.BaseText = "100";
            }
            cache = Store.Load<RateCache>("rates.json");
            if (cache != null && cache.Rates == null) cache = null;

            Title = "汇率换算";
            Width = settings.Width >= 340 ? settings.Width : 400;
            Height = settings.Height >= 360 ? settings.Height : 600;
            MinWidth = 340;
            MinHeight = 360;
            FontFamily = new FontFamily("Microsoft YaHei UI, Segoe UI");
            Background = Ui.Page;
            UseLayoutRounding = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            RestorePosition();

            Content = BuildLayout();
            Topmost = settings.Topmost;
            pinBtn.IsChecked = settings.Topmost;

            foreach (var code in settings.Currencies) CreateRow(code);
            var baseRow = FindRow(settings.BaseCode);
            SetText(baseRow, settings.BaseText ?? "");
            Recalc(baseRow);
            UpdateRateTexts();
            UpdateStatus();

            Loaded += async delegate
            {
                FocusRow(baseRow);
                await RefreshRates();
            };
            // 免费数据源每天更新，每 30 分钟检查一次足够及时
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(30) };
            timer.Tick += async delegate { await RefreshRates(); };
            timer.Start();
            Closing += delegate { SaveSettings(); };
            // 切到后台时归还启动期间用过的临时内存
            Deactivated += delegate { TrimMemory(); };
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool SetProcessWorkingSetSize(IntPtr process, IntPtr min, IntPtr max);

        static void TrimMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, (IntPtr)(-1), (IntPtr)(-1));
        }

        UIElement BuildLayout()
        {
            var root = new DockPanel();

            // 顶栏：标题 + 置顶 + 刷新
            var header = new Grid { Margin = new Thickness(20, 14, 14, 6) };
            header.ColumnDefinitions.Add(new ColumnDefinition());
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            header.Children.Add(new TextBlock
            {
                Text = "汇率换算",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = Ui.Text,
                VerticalAlignment = VerticalAlignment.Center
            });

            pinBtn = new ToggleButton { Content = Ui.GlyphPin, ToolTip = "窗口置顶", Style = (Style)FindResource("IconBtn") };
            pinBtn.Click += delegate
            {
                Topmost = pinBtn.IsChecked == true;
                settings.Topmost = Topmost;
                SaveSettings();
            };
            Grid.SetColumn(pinBtn, 1);
            header.Children.Add(pinBtn);

            refreshSpin = new RotateTransform();
            var refreshIcon = new TextBlock
            {
                Text = Ui.GlyphRefresh,
                RenderTransform = refreshSpin,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            refreshBtn = new Button { Content = refreshIcon, ToolTip = "刷新汇率", Style = (Style)FindResource("IconBtn"), Margin = new Thickness(4, 0, 0, 0) };
            refreshBtn.Click += async delegate { await RefreshRates(); };
            Grid.SetColumn(refreshBtn, 2);
            header.Children.Add(refreshBtn);
            DockPanel.SetDock(header, Dock.Top);
            root.Children.Add(header);

            // 底栏：数据来源与更新时间
            statusText = new TextBlock
            {
                FontSize = 11.5,
                Foreground = Ui.Muted,
                Margin = new Thickness(20, 6, 20, 12),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            DockPanel.SetDock(statusText, Dock.Bottom);
            root.Children.Add(statusText);

            // 中间：货币卡片列表 + 添加按钮
            rowsPanel = new StackPanel();
            var addContent = new StackPanel { Orientation = Orientation.Horizontal };
            addContent.Children.Add(new TextBlock { Text = Ui.GlyphAdd, FontFamily = new FontFamily(Ui.IconFont), FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 1, 8, 0) });
            addContent.Children.Add(new TextBlock { Text = "添加货币", VerticalAlignment = VerticalAlignment.Center });
            var addBtn = new Button { Content = addContent, Style = (Style)FindResource("AddBtn") };
            addBtn.Click += delegate { ShowAddDialog(); };

            var list = new StackPanel { Margin = new Thickness(16, 6, 16, 6) };
            list.Children.Add(rowsPanel);
            list.Children.Add(addBtn);
            root.Children.Add(new ScrollViewer
            {
                Content = list,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Focusable = false
            });
            return root;
        }

        CurrencyRow CreateRow(string code)
        {
            var row = new CurrencyRow { Code = code };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });

            var symbol = Currencies.Symbol(code);
            var badge = new Border
            {
                Width = 36,
                Height = 36,
                CornerRadius = new CornerRadius(18),
                Background = Currencies.Color(code),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text = symbol,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = symbol.Length >= 3 ? 10.5 : symbol.Length == 2 ? 12.5 : 15,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            grid.Children.Add(badge);

            var names = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
            names.Children.Add(new TextBlock { Text = code, FontSize = 15, FontWeight = FontWeights.SemiBold, Foreground = Ui.Text });
            names.Children.Add(new TextBlock { Text = Currencies.Name(code), FontSize = 12, Foreground = Ui.Sub, Margin = new Thickness(0, 1, 0, 0) });
            Grid.SetColumn(names, 1);
            grid.Children.Add(names);

            row.Box = new TextBox { Style = (Style)FindResource("AmountBox") };
            row.RateText = new TextBlock { FontSize = 11, Foreground = Ui.Muted, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 1, 0, 0) };
            var right = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            right.Children.Add(row.Box);
            right.Children.Add(row.RateText);
            Grid.SetColumn(right, 2);
            grid.Children.Add(right);

            row.RemoveBtn = new Button
            {
                Content = Ui.GlyphClose,
                ToolTip = "移除",
                Style = (Style)FindResource("IconBtn"),
                Width = 24,
                Height = 24,
                FontSize = 10,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Hidden
            };
            row.RemoveBtn.Click += delegate { RemoveRow(row); };
            Grid.SetColumn(row.RemoveBtn, 3);
            grid.Children.Add(row.RemoveBtn);

            row.Root = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(12),
                BorderThickness = new Thickness(1.5),
                BorderBrush = Ui.Line,
                Padding = new Thickness(14, 10, 6, 10),
                Margin = new Thickness(0, 0, 0, 10),
                Cursor = Cursors.IBeam,
                Child = grid
            };

            row.Root.MouseEnter += delegate { if (rows.Count > 1) row.RemoveBtn.Visibility = Visibility.Visible; };
            row.Root.MouseLeave += delegate { row.RemoveBtn.Visibility = Visibility.Hidden; };
            row.Root.MouseLeftButtonDown += delegate { FocusRow(row); };

            // 首次点击输入框时全选，方便直接输入新数字
            row.Box.PreviewMouseLeftButtonDown += (s, e) =>
            {
                if (!row.Box.IsKeyboardFocusWithin) { FocusRow(row); e.Handled = true; }
            };
            row.Box.GotKeyboardFocus += delegate { row.Root.BorderBrush = Ui.Accent; row.Box.SelectAll(); };
            row.Box.LostKeyboardFocus += delegate { row.Root.BorderBrush = Ui.Line; Commit(row); };
            row.Box.TextChanged += delegate { if (!suppress) Recalc(row); };
            row.Box.PreviewKeyDown += (s, e) => OnBoxKey(row, e);

            rows.Add(row);
            rowsPanel.Children.Add(row.Root);
            return row;
        }

        void OnBoxKey(CurrencyRow row, KeyEventArgs e)
        {
            int i = rows.IndexOf(row);
            if (e.Key == Key.Enter)
            {
                Commit(row);
                row.Box.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == Key.Down || (e.Key == Key.Tab && Keyboard.Modifiers == ModifierKeys.None))
            {
                FocusRow(rows[(i + 1) % rows.Count]);
                e.Handled = true;
            }
            else if (e.Key == Key.Up || (e.Key == Key.Tab && Keyboard.Modifiers == ModifierKeys.Shift))
            {
                FocusRow(rows[(i - 1 + rows.Count) % rows.Count]);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                row.Box.Clear();
                e.Handled = true;
            }
        }

        void FocusRow(CurrencyRow row)
        {
            if (row == null) return;
            row.Box.Focus();
            row.Box.SelectAll();
        }

        CurrencyRow FindRow(string code)
        {
            return rows.FirstOrDefault(r => r.Code == code) ?? rows.FirstOrDefault();
        }

        void SetText(CurrencyRow row, string text)
        {
            suppress = true;
            row.Box.Text = text;
            suppress = false;
        }

        // 用户输入的表达式（如 100*3）在回车或离开时替换成计算结果
        void Commit(CurrencyRow row)
        {
            double v;
            if (Calc.IsExpression(row.Box.Text) && Calc.TryEval(row.Box.Text, out v))
            {
                SetText(row, Calc.FormatExact(v));
                Recalc(row);
            }
        }

        double? Convert(double amount, string from, string to)
        {
            if (from == to) return amount;
            double rf, rt;
            if (cache == null || !cache.Rates.TryGetValue(from, out rf) || !cache.Rates.TryGetValue(to, out rt) || rf <= 0)
                return null;
            return amount / rf * rt;
        }

        // 以 src 这一行的数值为准，重算其他所有行
        void Recalc(CurrencyRow src)
        {
            if (src == null) return;
            settings.BaseCode = src.Code;
            settings.BaseText = src.Box.Text;

            bool empty = Calc.Normalize(src.Box.Text).Length == 0;
            double v;
            bool ok = Calc.TryEval(src.Box.Text, out v);
            if (!empty && !ok) return; // 输入到一半（如 “12+”）时保持原样

            foreach (var r in rows)
            {
                if (r == src) continue;
                if (empty) { SetText(r, ""); continue; }
                var c = Convert(v, src.Code, r.Code);
                SetText(r, c.HasValue ? Calc.Format(c.Value, r.Code) : "—");
            }
        }

        void UpdateRateTexts()
        {
            foreach (var r in rows)
            {
                var refCode = r.Code == "CNY" ? "USD" : "CNY";
                var c = Convert(1, r.Code, refCode);
                r.RateText.Text = c.HasValue ? "1 " + r.Code + " = " + Calc.FormatRate(c.Value) + " " + refCode : "";
            }
        }

        void UpdateStatus()
        {
            if (fetching)
            {
                statusText.Text = "正在获取最新汇率…";
                statusText.Foreground = Ui.Muted;
                return;
            }
            if (cache == null)
            {
                statusText.Text = "无法获取汇率，请检查网络后点右上角刷新";
                statusText.Foreground = Ui.Error;
                statusText.ToolTip = lastError;
                return;
            }
            var time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(cache.UpdatedUnix).ToLocalTime();
            var text = "汇率更新于 " + time.ToString("yyyy-MM-dd HH:mm") + " · 来源 " + cache.Source;
            if (lastError != null)
            {
                statusText.Text = "联网失败，使用缓存汇率（" + time.ToString("yyyy-MM-dd HH:mm") + "）";
                statusText.Foreground = Ui.Warn;
                statusText.ToolTip = lastError;
            }
            else
            {
                statusText.Text = text;
                statusText.Foreground = Ui.Muted;
                statusText.ToolTip = text + "\n免费数据源每日更新，仅供参考";
            }
        }

        async Task RefreshRates()
        {
            if (fetching) return;
            fetching = true;
            refreshBtn.IsEnabled = false;
            var spin = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(0.9)) { RepeatBehavior = RepeatBehavior.Forever };
            refreshSpin.BeginAnimation(RotateTransform.AngleProperty, spin);
            UpdateStatus();
            try
            {
                cache = await RateService.FetchAsync();
                lastError = null;
                Store.Save("rates.json", cache);
            }
            catch (Exception ex)
            {
                lastError = ex.GetBaseException().Message;
            }
            finally
            {
                fetching = false;
                refreshBtn.IsEnabled = true;
                refreshSpin.BeginAnimation(RotateTransform.AngleProperty, null);
            }
            var focused = rows.FirstOrDefault(r => r.Box.IsKeyboardFocusWithin);
            Recalc(focused ?? FindRow(settings.BaseCode));
            UpdateRateTexts();
            UpdateStatus();
            if (!IsActive) TrimMemory();
        }

        void ShowAddDialog()
        {
            var existing = new HashSet<string>(rows.Select(r => r.Code));
            IEnumerable<string> pool = cache != null ? (IEnumerable<string>)cache.Rates.Keys : Currencies.Ordered;
            var available = pool.Where(c => !existing.Contains(c))
                .OrderBy(c => Currencies.IsKnown(c) ? Currencies.Ordered.IndexOf(c) : int.MaxValue)
                .ThenBy(c => c)
                .ToList();
            var dlg = new AddCurrencyWindow(this, available);
            if (dlg.ShowDialog() != true || dlg.Selected == null) return;

            var baseRow = FindRow(settings.BaseCode);
            var row = CreateRow(dlg.Selected);
            settings.Currencies.Add(dlg.Selected);
            Recalc(baseRow);
            UpdateRateTexts();
            SaveSettings();
            row.Root.BringIntoView();
        }

        void RemoveRow(CurrencyRow row)
        {
            if (rows.Count <= 1) return;
            rows.Remove(row);
            rowsPanel.Children.Remove(row.Root);
            settings.Currencies.Remove(row.Code);
            if (settings.BaseCode == row.Code)
            {
                settings.BaseCode = rows[0].Code;
                settings.BaseText = rows[0].Box.Text;
            }
            SaveSettings();
        }

        void RestorePosition()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (!settings.HasPosition) return;
            var l = settings.Left;
            var t = settings.Top;
            if (l < SystemParameters.VirtualScreenLeft || t < SystemParameters.VirtualScreenTop ||
                l + 100 > SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth ||
                t + 100 > SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight)
                return;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = l;
            Top = t;
        }

        void SaveSettings()
        {
            if (WindowState == WindowState.Normal && IsLoaded)
            {
                settings.HasPosition = true;
                settings.Left = Left;
                settings.Top = Top;
                settings.Width = ActualWidth;
                settings.Height = ActualHeight;
            }
            Store.Save("settings.json", settings);
        }
    }
}
