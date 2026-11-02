using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace RateConverter
{
    // 解析输入（支持全角字符、千分位和 + - * / 四则运算）并格式化金额
    static class Calc
    {
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        static readonly Regex ExprChars = new Regex(@"^[0-9.+\-*/()]+$");

        public static string Normalize(string s)
        {
            var sb = new StringBuilder();
            foreach (char ch in s ?? "")
            {
                char c = ch;
                if (c >= '０' && c <= '９') c = (char)('0' + (c - '０'));
                else if (c == '．' || c == '。') c = '.';
                else if (c == '＋') c = '+';
                else if (c == '－' || c == '—') c = '-';
                else if (c == '×' || c == '＊' || c == 'x' || c == 'X') c = '*';
                else if (c == '÷' || c == '／') c = '/';
                else if (c == '（') c = '(';
                else if (c == '）') c = ')';
                else if (c == ',' || c == '，' || char.IsWhiteSpace(c)) continue;
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static bool IsExpression(string s)
        {
            var n = Normalize(s);
            return n.Length > 1 && n.Substring(1).IndexOfAny(new[] { '+', '-', '*', '/', '(', ')' }) >= 0;
        }

        public static bool TryEval(string s, out double v)
        {
            v = 0;
            var n = Normalize(s);
            if (n.Length == 0) return false;
            if (double.TryParse(n, NumberStyles.Float, Inv, out v)) return IsFinite(v);
            if (!ExprChars.IsMatch(n)) return false;
            try
            {
                var r = new DataTable().Compute(n, null);
                v = Convert.ToDouble(r, Inv);
                return IsFinite(v);
            }
            catch
            {
                return false;
            }
        }

        static bool IsFinite(double v) { return !double.IsNaN(v) && !double.IsInfinity(v); }

        // 按货币习惯显示：日元等不带小数，其余两位；小于 1 的数保留足够的有效数字
        public static string Format(double v, string code)
        {
            double a = Math.Abs(v);
            if (a != 0 && a < 1)
            {
                int dec = Math.Min(8, Math.Max(2, (int)Math.Ceiling(-Math.Log10(a)) + 3));
                return TrimZeros(v.ToString("N" + dec, Inv), 2);
            }
            return v.ToString("N" + Currencies.Decimals(code), Inv);
        }

        // 计算结果（表达式求值后）尽量保留用户输入的精度
        public static string FormatExact(double v)
        {
            return v.ToString("#,0.######", Inv);
        }

        // 汇率保留 4 位有效数字左右
        public static string FormatRate(double v)
        {
            double a = Math.Abs(v);
            if (a == 0) return "0";
            int dec = a >= 100 ? 2 : Math.Min(8, Math.Max(4, (int)Math.Ceiling(-Math.Log10(a)) + 3));
            return TrimZeros(v.ToString("N" + dec, Inv), 2);
        }

        static string TrimZeros(string s, int keep)
        {
            int dot = s.IndexOf('.');
            if (dot < 0) return s;
            int end = s.Length;
            while (end > dot + 1 + keep && s[end - 1] == '0') end--;
            return s.Substring(0, end);
        }
    }
}
