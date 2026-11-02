using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace RateConverter
{
    // 配色与控件样式
    static class Ui
    {
        public static readonly Brush Accent = Brush("#3B6FE0");
        public static readonly Brush Text = Brush("#1F2937");
        public static readonly Brush Sub = Brush("#6B7280");
        public static readonly Brush Muted = Brush("#9CA3AF");
        public static readonly Brush Line = Brush("#E5E8EF");
        public static readonly Brush Page = Brush("#F3F5F9");
        public static readonly Brush Warn = Brush("#C2410C");
        public static readonly Brush Error = Brush("#DC2626");

        public const string IconFont = "Segoe Fluent Icons, Segoe MDL2 Assets";
        public const string GlyphRefresh = "";
        public const string GlyphPin = "";
        public const string GlyphClose = "";
        public const string GlyphAdd = "";
        public const string GlyphSearch = "";

        public static Brush Brush(string hex)
        {
            var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            b.Freeze();
            return b;
        }

        public static ResourceDictionary Styles()
        {
            return (ResourceDictionary)XamlReader.Parse(Xaml);
        }

        const string Xaml = @"
<ResourceDictionary xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Style x:Key='IconBtn' TargetType='ButtonBase'>
    <Setter Property='Background' Value='Transparent'/>
    <Setter Property='Foreground' Value='#6B7280'/>
    <Setter Property='FontFamily' Value='Segoe Fluent Icons, Segoe MDL2 Assets'/>
    <Setter Property='FontSize' Value='14'/>
    <Setter Property='Width' Value='32'/>
    <Setter Property='Height' Value='32'/>
    <Setter Property='Cursor' Value='Hand'/>
    <Setter Property='Focusable' Value='False'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='ButtonBase'>
          <Border x:Name='bd' Background='{TemplateBinding Background}' CornerRadius='8'>
            <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
          </Border>
          <ControlTemplate.Triggers>
            <Trigger Property='IsMouseOver' Value='True'>
              <Setter TargetName='bd' Property='Background' Value='#E6EAF2'/>
              <Setter Property='Foreground' Value='#1F2937'/>
            </Trigger>
            <Trigger Property='IsPressed' Value='True'>
              <Setter TargetName='bd' Property='Background' Value='#D9DFEB'/>
            </Trigger>
            <Trigger Property='ToggleButton.IsChecked' Value='True'>
              <Setter TargetName='bd' Property='Background' Value='#E3EBFD'/>
              <Setter Property='Foreground' Value='#3B6FE0'/>
            </Trigger>
            <Trigger Property='IsEnabled' Value='False'>
              <Setter Property='Opacity' Value='0.6'/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <Style x:Key='AddBtn' TargetType='Button'>
    <Setter Property='Foreground' Value='#6B7280'/>
    <Setter Property='FontSize' Value='14'/>
    <Setter Property='Height' Value='48'/>
    <Setter Property='Cursor' Value='Hand'/>
    <Setter Property='Focusable' Value='False'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='Button'>
          <Grid Background='Transparent'>
            <Rectangle x:Name='rc' RadiusX='12' RadiusY='12' Stroke='#C9D0DD' StrokeThickness='1.5' StrokeDashArray='4 3'/>
            <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
          </Grid>
          <ControlTemplate.Triggers>
            <Trigger Property='IsMouseOver' Value='True'>
              <Setter TargetName='rc' Property='Stroke' Value='#3B6FE0'/>
              <Setter TargetName='rc' Property='Fill' Value='#EEF3FE'/>
              <Setter Property='Foreground' Value='#3B6FE0'/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <Style x:Key='AmountBox' TargetType='TextBox'>
    <Setter Property='BorderThickness' Value='0'/>
    <Setter Property='Background' Value='Transparent'/>
    <Setter Property='FontFamily' Value='Segoe UI'/>
    <Setter Property='FontWeight' Value='SemiBold'/>
    <Setter Property='FontSize' Value='22'/>
    <Setter Property='Foreground' Value='#111827'/>
    <Setter Property='CaretBrush' Value='#3B6FE0'/>
    <Setter Property='SelectionBrush' Value='#3B6FE0'/>
    <Setter Property='TextAlignment' Value='Right'/>
    <Setter Property='Padding' Value='0'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='TextBox'>
          <ScrollViewer x:Name='PART_ContentHost' Focusable='False'
                        HorizontalScrollBarVisibility='Hidden' VerticalScrollBarVisibility='Hidden'/>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <Style x:Key='SearchBox' TargetType='TextBox'>
    <Setter Property='FontSize' Value='14'/>
    <Setter Property='Padding' Value='32,8,10,8'/>
    <Setter Property='Foreground' Value='#1F2937'/>
    <Setter Property='CaretBrush' Value='#3B6FE0'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='TextBox'>
          <Border x:Name='bd' Background='White' BorderBrush='#D5DAE3' BorderThickness='1.5' CornerRadius='10'>
            <ScrollViewer x:Name='PART_ContentHost' Focusable='False' Margin='{TemplateBinding Padding}'
                          HorizontalScrollBarVisibility='Hidden' VerticalScrollBarVisibility='Hidden'/>
          </Border>
          <ControlTemplate.Triggers>
            <Trigger Property='IsKeyboardFocused' Value='True'>
              <Setter TargetName='bd' Property='BorderBrush' Value='#3B6FE0'/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <Style x:Key='PickItem' TargetType='ListBoxItem'>
    <Setter Property='Cursor' Value='Hand'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='ListBoxItem'>
          <Border x:Name='bd' Background='Transparent' CornerRadius='8' Padding='10,7'>
            <ContentPresenter/>
          </Border>
          <ControlTemplate.Triggers>
            <Trigger Property='IsMouseOver' Value='True'>
              <Setter TargetName='bd' Property='Background' Value='#EEF1F6'/>
            </Trigger>
            <Trigger Property='IsSelected' Value='True'>
              <Setter TargetName='bd' Property='Background' Value='#E3EBFD'/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <Style TargetType='ScrollBar'>
    <Setter Property='Width' Value='8'/>
    <Setter Property='MinWidth' Value='8'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='ScrollBar'>
          <Track x:Name='PART_Track' IsDirectionReversed='True'>
            <Track.Thumb>
              <Thumb>
                <Thumb.Template>
                  <ControlTemplate TargetType='Thumb'>
                    <Border Background='#C9CFDA' CornerRadius='4' Margin='1,0'/>
                  </ControlTemplate>
                </Thumb.Template>
              </Thumb>
            </Track.Thumb>
          </Track>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>
</ResourceDictionary>";
    }
}
