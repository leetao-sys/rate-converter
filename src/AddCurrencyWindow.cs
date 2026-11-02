using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RateConverter
{
    // 搜索并选择要添加的货币
    public class AddCurrencyWindow : Window
    {
        public string Selected;

        readonly List<string> all;
        readonly TextBox search;
        readonly ListBox list;

        public AddCurrencyWindow(Window owner, List<string> available)
        {
            Owner = owner;
            all = available;
            Title = "添加货币";
            Width = 340;
            Height = 480;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Topmost = owner.Topmost;
            FontFamily = owner.FontFamily;
            Background = Ui.Page;
            UseLayoutRounding = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);

            var root = new DockPanel { Margin = new Thickness(14) };

            search = new TextBox { Style = (Style)FindResource("SearchBox") };
            var placeholder = new TextBlock
            {
                Text = "搜索代码或中文名，如 EUR、欧元",
                Foreground = Ui.Muted,
                FontSize = 13,
                IsHitTestVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(34, 0, 0, 0)
            };
            var icon = new TextBlock
            {
                Text = Ui.GlyphSearch,
                FontFamily = new FontFamily(Ui.IconFont),
                Foreground = Ui.Muted,
                FontSize = 13,
                IsHitTestVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0)
            };
            var searchWrap = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            searchWrap.Children.Add(search);
            searchWrap.Children.Add(icon);
            searchWrap.Children.Add(placeholder);
            DockPanel.SetDock(searchWrap, Dock.Top);
            root.Children.Add(searchWrap);

            list = new ListBox
            {
                BorderThickness = new Thickness(0),
                Background = Brushes.White,
                Padding = new Thickness(6),
                ItemContainerStyle = (Style)FindResource("PickItem")
            };
            ScrollViewer.SetHorizontalScrollBarVisibility(list, System.Windows.Controls.ScrollBarVisibility.Disabled);
            var listWrap = new Border { CornerRadius = new CornerRadius(12), Background = Brushes.White, Padding = new Thickness(0, 6, 0, 6), Child = list };
            root.Children.Add(listWrap);
            Content = root;

            search.TextChanged += delegate
            {
                placeholder.Visibility = search.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
                Fill();
            };
            search.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter) { Accept(); e.Handled = true; }
                else if (e.Key == Key.Down && list.Items.Count > 0)
                {
                    list.SelectedIndex = System.Math.Min(list.SelectedIndex + 1, list.Items.Count - 1);
                    list.ScrollIntoView(list.SelectedItem);
                    e.Handled = true;
                }
                else if (e.Key == Key.Up && list.Items.Count > 0)
                {
                    list.SelectedIndex = System.Math.Max(list.SelectedIndex - 1, 0);
                    list.ScrollIntoView(list.SelectedItem);
                    e.Handled = true;
                }
            };
            list.MouseDoubleClick += delegate { Accept(); };
            list.KeyDown += (s, e) => { if (e.Key == Key.Enter) Accept(); };
            PreviewKeyDown += (s, e) => { if (e.Key == Key.Escape) Close(); };

            Fill();
            Loaded += delegate { search.Focus(); };
        }

        void Fill()
        {
            var q = search.Text.Trim();
            var upper = q.ToUpperInvariant();
            list.Items.Clear();
            foreach (var code in all.Where(c => q.Length == 0 || c.Contains(upper) || Currencies.Name(c).Contains(q)))
            {
                var item = new StackPanel { Orientation = Orientation.Horizontal };
                item.Children.Add(new TextBlock { Text = code, Width = 52, FontSize = 14, FontWeight = FontWeights.SemiBold, Foreground = Ui.Text });
                item.Children.Add(new TextBlock { Text = Currencies.Name(code), FontSize = 13.5, Foreground = Ui.Sub });
                list.Items.Add(new ListBoxItem { Content = item, Tag = code });
            }
            if (list.Items.Count > 0) list.SelectedIndex = 0;
        }

        void Accept()
        {
            var item = list.SelectedItem as ListBoxItem;
            if (item == null) return;
            Selected = (string)item.Tag;
            DialogResult = true;
        }
    }
}
