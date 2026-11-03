import Foundation

/// 汇率以 1 USD 为基准
struct RateCache: Codable {
    var rates: [String: Double]
    var updatedUnix: Int64
    var source: String
    /// 本机最后一次成功获取的时间，用于判断何时需要再次刷新
    var fetchedAt: Date?
}

/// 与 Windows 版相同的免费、无需 API Key 的汇率源；主源失败时依次尝试备用源
enum RateService {
    private static let session: URLSession = {
        let c = URLSessionConfiguration.ephemeral
        c.timeoutIntervalForRequest = 12
        c.requestCachePolicy = .reloadIgnoringLocalCacheData
        c.httpAdditionalHeaders = ["User-Agent": "RateConverter-iOS/1.0"]
        return URLSession(configuration: c)
    }()

    static func fetch() async throws -> RateCache {
        var last: Error
        do { return try await fromErApi() } catch { last = error }
        for url in [
            "https://latest.currency-api.pages.dev/v1/currencies/usd.min.json",
            "https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies/usd.min.json",
        ] {
            do { return try await fromCurrencyApi(url) } catch { last = error }
        }
        throw last
    }

    struct Failure: LocalizedError {
        let message: String
        var errorDescription: String? { message }
    }

    private static func getJson(_ url: String) async throws -> [String: Any] {
        let (data, resp) = try await session.data(from: URL(string: url)!)
        if let h = resp as? HTTPURLResponse, !(200..<300).contains(h.statusCode) {
            throw Failure(message: "HTTP \(h.statusCode)")
        }
        guard let obj = try JSONSerialization.jsonObject(with: data) as? [String: Any] else {
            throw Failure(message: "数据格式错误")
        }
        return obj
    }

    private static func toDouble(_ o: Any?) -> Double? {
        if let n = o as? NSNumber { return n.doubleValue }
        if let s = o as? String { return Double(s) }
        return nil
    }

    /// https://open.er-api.com （ExchangeRate-API 开放接口，约 160 种货币）
    private static func fromErApi() async throws -> RateCache {
        let root = try await getJson("https://open.er-api.com/v6/latest/USD")
        guard (root["result"] as? String) == "success", let raw = root["rates"] as? [String: Any] else {
            throw Failure(message: "ExchangeRate-API 返回失败")
        }
        var rates: [String: Double] = [:]
        for (k, v) in raw { if let d = toDouble(v) { rates[k.uppercased()] = d } }
        let updated = Int64(toDouble(root["time_last_update_unix"]) ?? Date().timeIntervalSince1970)
        return try finish(rates, updated, "ExchangeRate-API")
    }

    /// fawazahmed0/currency-api，数据中混有加密货币，只保留已知法币
    private static func fromCurrencyApi(_ url: String) async throws -> RateCache {
        let root = try await getJson(url)
        guard let raw = root["usd"] as? [String: Any] else { throw Failure(message: "Currency-API 返回失败") }
        var rates: [String: Double] = [:]
        for (k, v) in raw {
            let code = k.uppercased()
            if Currencies.isKnown(code), let d = toDouble(v) { rates[code] = d }
        }
        let f = DateFormatter()
        f.locale = Locale(identifier: "en_US_POSIX")
        f.timeZone = TimeZone(identifier: "UTC")
        f.dateFormat = "yyyy-MM-dd"
        let date = (root["date"] as? String).flatMap(f.date(from:)) ?? Date()
        return try finish(rates, Int64(date.timeIntervalSince1970), "Currency-API")
    }

    private static func finish(_ r: [String: Double], _ updated: Int64, _ source: String) throws -> RateCache {
        var rates = r
        rates["USD"] = 1
        guard rates.count >= 10, rates["CNY"] != nil else { throw Failure(message: "汇率数据不完整") }
        return RateCache(rates: rates, updatedUnix: updated, source: source, fetchedAt: Date())
    }
}
