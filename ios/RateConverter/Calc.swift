import Foundation

/// 解析输入（支持全角字符、千分位和 + - × ÷ 四则运算）并格式化金额，规则与 Windows 版一致
enum Calc {
    static func normalize(_ s: String) -> String {
        var out = ""
        for ch in s {
            switch ch {
            case "０"..."９":
                let v = ch.unicodeScalars.first!.value - ("０" as Unicode.Scalar).value
                out.append(Character(Unicode.Scalar(UInt32(48) + v)!))
            case "．", "。": out.append(".")
            case "＋": out.append("+")
            case "－", "—", "−": out.append("-")
            case "×", "＊", "x", "X": out.append("*")
            case "÷", "／": out.append("/")
            case "（": out.append("(")
            case "）": out.append(")")
            case ",", "，": continue
            default:
                if ch.isWhitespace { continue }
                out.append(ch)
            }
        }
        return out
    }

    /// 是否是表达式（第一个字符之后还有运算符），如 “100*3”
    static func isExpression(_ s: String) -> Bool {
        let n = normalize(s)
        guard n.count > 1 else { return false }
        return n.dropFirst().contains { "+-*/()".contains($0) }
    }

    /// 求值；输入不完整（如 “12+”）或非法时返回 nil
    static func eval(_ s: String) -> Double? {
        let n = normalize(s)
        if n.isEmpty { return nil }
        var p = Parser(chars: Array(n))
        guard let v = p.parseExpr(), p.pos == p.chars.count, v.isFinite else { return nil }
        return v
    }

    // 递归下降：expr = term (('+'|'-') term)* ; term = factor (('*'|'/') factor)* ; factor = ('+'|'-') factor | number | '(' expr ')'
    private struct Parser {
        let chars: [Character]
        var pos = 0

        mutating func parseExpr() -> Double? {
            guard var v = parseTerm() else { return nil }
            while pos < chars.count, chars[pos] == "+" || chars[pos] == "-" {
                let op = chars[pos]; pos += 1
                guard let r = parseTerm() else { return nil }
                v = op == "+" ? v + r : v - r
            }
            return v
        }

        mutating func parseTerm() -> Double? {
            guard var v = parseFactor() else { return nil }
            while pos < chars.count, chars[pos] == "*" || chars[pos] == "/" {
                let op = chars[pos]; pos += 1
                guard let r = parseFactor() else { return nil }
                v = op == "*" ? v * r : v / r
            }
            return v
        }

        mutating func parseFactor() -> Double? {
            guard pos < chars.count else { return nil }
            let c = chars[pos]
            if c == "-" || c == "+" {
                pos += 1
                guard let v = parseFactor() else { return nil }
                return c == "-" ? -v : v
            }
            if c == "(" {
                pos += 1
                guard let v = parseExpr(), pos < chars.count, chars[pos] == ")" else { return nil }
                pos += 1
                return v
            }
            let start = pos
            while pos < chars.count, chars[pos].isASCII, chars[pos].isNumber || chars[pos] == "." { pos += 1 }
            // 科学计数法（如 1e5）
            if pos < chars.count, chars[pos] == "e" || chars[pos] == "E" {
                var q = pos + 1
                if q < chars.count, chars[q] == "+" || chars[q] == "-" { q += 1 }
                if q < chars.count, chars[q].isASCII, chars[q].isNumber {
                    pos = q
                    while pos < chars.count, chars[pos].isASCII, chars[pos].isNumber { pos += 1 }
                }
            }
            guard pos > start else { return nil }
            return Double(String(chars[start..<pos]))
        }
    }

    private static func formatter(_ minFrac: Int, _ maxFrac: Int) -> NumberFormatter {
        let f = NumberFormatter()
        f.locale = Locale(identifier: "en_US_POSIX")
        f.numberStyle = .decimal
        f.usesGroupingSeparator = true
        f.groupingSeparator = ","
        f.groupingSize = 3
        f.decimalSeparator = "."
        f.roundingMode = .halfUp
        f.minimumFractionDigits = minFrac
        f.maximumFractionDigits = maxFrac
        return f
    }

    private static var cache: [Int: NumberFormatter] = [:]
    private static func fmt(_ minFrac: Int, _ maxFrac: Int, _ v: Double) -> String {
        let key = minFrac * 100 + maxFrac
        let f = cache[key] ?? formatter(minFrac, maxFrac)
        cache[key] = f
        return f.string(from: NSNumber(value: v)) ?? String(v)
    }

    /// 按货币习惯显示：日元等不带小数，其余两位；小于 1 的数保留足够的有效数字
    static func format(_ v: Double, code: String) -> String {
        let a = abs(v)
        if a != 0 && a < 1 {
            let dec = min(8, max(2, Int((-log10(a)).rounded(.up)) + 3))
            return fmt(2, dec, v)
        }
        let d = Currencies.decimals(code)
        return fmt(d, d, v)
    }

    /// 表达式求值后尽量保留精度
    static func formatExact(_ v: Double) -> String { fmt(0, 6, v) }

    /// 汇率保留 4 位有效数字左右
    static func formatRate(_ v: Double) -> String {
        let a = abs(v)
        if a == 0 { return "0" }
        let dec = a >= 100 ? 2 : min(8, max(4, Int((-log10(a)).rounded(.up)) + 3))
        return fmt(2, dec, v)
    }
}
