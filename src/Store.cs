using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace RateConverter
{
    public class AppSettings
    {
        public List<string> Currencies { get; set; }
        public string BaseCode { get; set; }
        public string BaseText { get; set; }
        public bool Topmost { get; set; }
        public bool HasPosition { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    // 汇率以 1 USD 为基准
    public class RateCache
    {
        public Dictionary<string, double> Rates { get; set; }
        public long UpdatedUnix { get; set; }
        public string Source { get; set; }
    }

    // 设置与汇率缓存保存在 %APPDATA%\RateConverter
    static class Store
    {
        static readonly string Dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RateConverter");

        public static T Load<T>(string name) where T : class
        {
            try
            {
                var path = Path.Combine(Dir, name);
                if (!File.Exists(path)) return null;
                return new JavaScriptSerializer().Deserialize<T>(File.ReadAllText(path, Encoding.UTF8));
            }
            catch
            {
                return null;
            }
        }

        public static void Save(string name, object value)
        {
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllText(Path.Combine(Dir, name), new JavaScriptSerializer().Serialize(value), Encoding.UTF8);
            }
            catch
            {
                // 保存失败不影响使用
            }
        }
    }
}
