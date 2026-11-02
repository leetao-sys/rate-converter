using System;
using System.Net;
using System.Threading;
using System.Windows;

[assembly: System.Reflection.AssemblyTitle("汇率换算")]
[assembly: System.Reflection.AssemblyProduct("RateConverter")]
[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.0.0.0")]

namespace RateConverter
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            // 只允许运行一个实例
            bool created;
            using (var mutex = new Mutex(true, "RateConverter.SingleInstance", out created))
            {
                if (!created) return;
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                // 界面很简单，软件渲染即可，可省下显卡驱动占用的几十 MB 内存
                System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

                var app = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };
                app.Resources.MergedDictionaries.Add(Ui.Styles());
                app.Run(new MainWindow());
            }
        }
    }
}
