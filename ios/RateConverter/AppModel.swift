import Foundation

/// 界面状态与换算逻辑。修改任意一种货币的金额后，以它为基准重算其他所有货币
@MainActor
final class AppModel: ObservableObject {
    @Published private(set) var codes: [String]
    @Published private(set) var texts: [String: String] = [:]
    @Published private(set) var cache: RateCache?
    @Published private(set) var fetching = false
    @Published private(set) var lastError: String?

    private(set) var baseCode: String
    private let defaults = UserDefaults.standard

    /// 免费数据源每天更新，超过 30 分钟再检查一次足够及时
    static let refreshInterval: TimeInterval = 30 * 60

    init() {
        var codes = defaults.stringArray(forKey: "currencies") ?? []
        if codes.isEmpty { codes = Currencies.defaults }
        self.codes = codes
        var base = defaults.string(forKey: "baseCode") ?? ""
        var baseText = defaults.string(forKey: "baseText") ?? "100"
        if !codes.contains(base) {
            base = codes[0]
            baseText = "100"
        }
        baseCode = base
        if let data = defaults.data(forKey: "rates"), let c = try? JSONDecoder().decode(RateCache.self, from: data) {
            cache = c
        }
        texts[base] = baseText
        recalc(from: base)
    }

    func text(_ code: String) -> String { texts[code] ?? "" }

    /// 用户在某一行输入
    func edit(_ code: String, _ text: String) {
        guard texts[code] != text else { return }
        texts[code] = text
        recalc(from: code)
    }

    /// 输入的表达式（如 100*3）在点“=”或离开输入框时替换成计算结果
    func commit(_ code: String) {
        let t = text(code)
        if Calc.isExpression(t), let v = Calc.eval(t) {
            texts[code] = Calc.formatExact(v)
            recalc(from: code)
        }
    }

    func convert(_ amount: Double, from: String, to: String) -> Double? {
        if from == to { return amount }
        guard let r = cache?.rates, let rf = r[from], let rt = r[to], rf > 0 else { return nil }
        return amount / rf * rt
    }

    /// 以 src 这一行的数值为准，重算其他所有行
    func recalc(from src: String) {
        guard codes.contains(src) else { return }
        baseCode = src
        let t = text(src)
        defaults.set(src, forKey: "baseCode")
        defaults.set(t, forKey: "baseText")

        let empty = Calc.normalize(t).isEmpty
        let v = Calc.eval(t)
        if !empty && v == nil { return } // 输入到一半（如 “12+”）时保持原样

        var next = texts
        for c in codes where c != src {
            if empty { next[c] = ""; continue }
            if let r = convert(v!, from: src, to: c) {
                next[c] = Calc.format(r, code: c)
            } else {
                next[c] = "—"
            }
        }
        texts = next
    }

    /// 卡片下方的参考汇率，如 “1 HKD = 0.9134 CNY”
    func rateText(_ code: String) -> String {
        let ref = code == "CNY" ? "USD" : "CNY"
        guard let c = convert(1, from: code, to: ref) else { return "" }
        return "1 \(code) = \(Calc.formatRate(c)) \(ref)"
    }

    // MARK: - 货币列表

    var available: [String] {
        let existing = Set(codes)
        let pool = cache.map { Array($0.rates.keys) } ?? Currencies.ordered
        return pool.filter { !existing.contains($0) }.sorted { Currencies.sortKey($0) < Currencies.sortKey($1) }
    }

    func add(_ code: String) {
        guard !codes.contains(code) else { return }
        codes.append(code)
        saveCodes()
        recalc(from: baseCode)
    }

    func remove(at offsets: IndexSet) {
        guard codes.count - offsets.count >= 1 else { return }
        let removed = offsets.map { codes[$0] }
        codes.remove(atOffsets: offsets)
        for c in removed { texts[c] = nil }
        saveCodes()
        if !codes.contains(baseCode) {
            let b = codes[0]
            recalc(from: b)
        }
    }

    func move(from: IndexSet, to: Int) {
        codes.move(fromOffsets: from, toOffset: to)
        saveCodes()
    }

    private func saveCodes() { defaults.set(codes, forKey: "currencies") }

    // MARK: - 汇率

    func refreshIfStale() async {
        if let at = cache?.fetchedAt, Date().timeIntervalSince(at) < Self.refreshInterval, lastError == nil { return }
        await refresh()
    }

    func refresh() async {
        if fetching { return }
        fetching = true
        do {
            let c = try await RateService.fetch()
            cache = c
            lastError = nil
            if let data = try? JSONEncoder().encode(c) { defaults.set(data, forKey: "rates") }
        } catch {
            lastError = error.localizedDescription
        }
        fetching = false
        recalc(from: baseCode)
    }

    var statusText: String {
        if fetching { return "正在获取最新汇率…" }
        guard let c = cache else { return "无法获取汇率，请检查网络后下拉刷新" }
        let f = DateFormatter()
        f.dateFormat = "yyyy-MM-dd HH:mm"
        let time = f.string(from: Date(timeIntervalSince1970: TimeInterval(c.updatedUnix)))
        if lastError != nil { return "联网失败，使用缓存汇率（\(time)）" }
        return "汇率更新于 \(time) · 来源 \(c.source)\n免费数据源每日更新，仅供参考"
    }

    var statusIsWarning: Bool { !fetching && (cache == nil || lastError != nil) }
}
