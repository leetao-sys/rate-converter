using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace RateConverter
{
    // 免费、无需 API Key 的汇率源；主源失败时依次尝试备用源
    static class RateService
    {
        static readonly HttpClient http = CreateClient();

        static HttpClient CreateClient()
        {
            var c = new HttpClient { Timeout = TimeSpan.FromSeconds(12) };
            c.DefaultRequestHeaders.UserAgent.ParseAdd("RateConverter/1.0");
            return c;
        }

        public static async Task<RateCache> FetchAsync()
        {
            Exception last = null;
            try { return await FromErApi(); }
            catch (Exception e) { last = e; }
            foreach (var url in new[]
            {
                "https://latest.currency-api.pages.dev/v1/currencies/usd.min.json",
                "https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies/usd.min.json"
            })
            {
                try { return await FromCurrencyApi(url); }
                catch (Exception e) { last = e; }
            }
            throw last;
        }

        static async Task<Dictionary<string, object>> GetJson(string url)
        {
            var bytes = await http.GetByteArrayAsync(url);
            var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            return js.Deserialize<Dictionary<string, object>>(Encoding.UTF8.GetString(bytes));
        }

        static double ToDouble(object o) { return Convert.ToDouble(o, CultureInfo.InvariantCulture); }

        // https://open.er-api.com （ExchangeRate-API 开放接口，约 160 种货币）
        static async Task<RateCache> FromErApi()
        {
            var root = await GetJson("https://open.er-api.com/v6/latest/USD");
            if (!"success".Equals(root["result"])) throw new Exception("ExchangeRate-API 返回失败");
            var rates = new Dictionary<string, double>();
            foreach (var kv in (Dictionary<string, object>)root["rates"])
                rates[kv.Key.ToUpperInvariant()] = ToDouble(kv.Value);
            return Finish(rates, Convert.ToInt64(root["time_last_update_unix"]), "ExchangeRate-API");
        }

        // fawazahmed0/currency-api，数据中混有加密货币，只保留已知法币
        static async Task<RateCache> FromCurrencyApi(string url)
        {
            var root = await GetJson(url);
            var rates = new Dictionary<string, double>();
            foreach (var kv in (Dictionary<string, object>)root["usd"])
            {
                var code = kv.Key.ToUpperInvariant();
                if (Currencies.IsKnown(code)) rates[code] = ToDouble(kv.Value);
            }
            var date = DateTime.ParseExact((string)root["date"], "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            return Finish(rates, ToUnix(date), "Currency-API");
        }

        static RateCache Finish(Dictionary<string, double> rates, long updated, string source)
        {
            rates["USD"] = 1;
            if (rates.Count < 10 || !rates.ContainsKey("CNY")) throw new Exception("汇率数据不完整");
            return new RateCache { Rates = rates, UpdatedUnix = updated, Source = source };
        }

        static long ToUnix(DateTime utc)
        {
            return (long)(utc - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
    }
}
