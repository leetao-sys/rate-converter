import SwiftUI

/// 货币的中文名、符号、小数位与配色（与 Windows 版一致）
enum Currencies {
    static let defaults = ["CNY", "HKD", "JPY", "USD"]

    /// 顺序即“添加货币”列表中的推荐顺序
    private static let table: [String] = [
        "CNY|人民币|¥", "HKD|港币|HK$", "JPY|日元|円", "USD|美元|$",
        "EUR|欧元|€", "GBP|英镑|£", "KRW|韩元|₩", "TWD|新台币|NT$",
        "MOP|澳门元|MOP", "SGD|新加坡元|S$", "AUD|澳大利亚元|A$", "CAD|加拿大元|C$",
        "NZD|新西兰元|NZ$", "CHF|瑞士法郎|Fr", "THB|泰铢|฿", "MYR|马来西亚林吉特|RM",
        "IDR|印尼盾|Rp", "PHP|菲律宾比索|₱", "VND|越南盾|₫", "INR|印度卢比|₹",
        "RUB|俄罗斯卢布|₽", "AED|阿联酋迪拉姆|", "SAR|沙特里亚尔|", "TRY|土耳其里拉|₺",
        "BRL|巴西雷亚尔|R$", "MXN|墨西哥比索|Mex$", "ZAR|南非兰特|R", "SEK|瑞典克朗|kr",
        "NOK|挪威克朗|kr", "DKK|丹麦克朗|kr", "PLN|波兰兹罗提|zł", "CZK|捷克克朗|Kč",
        "HUF|匈牙利福林|Ft", "ILS|以色列新谢克尔|₪", "KZT|哈萨克斯坦坚戈|₸", "PKR|巴基斯坦卢比|",
        "LKR|斯里兰卡卢比|", "BDT|孟加拉塔卡|৳", "NPR|尼泊尔卢比|", "MMK|缅甸元|",
        "KHR|柬埔寨瑞尔|៛", "LAK|老挝基普|₭", "MNT|蒙古图格里克|₮", "EGP|埃及镑|",
        "NGN|尼日利亚奈拉|₦", "KES|肯尼亚先令|", "ARS|阿根廷比索|", "CLP|智利比索|",
        "COP|哥伦比亚比索|", "PEN|秘鲁索尔|", "UAH|乌克兰格里夫纳|₴", "RON|罗马尼亚列伊|",
        "BGN|保加利亚列弗|", "ISK|冰岛克朗|", "QAR|卡塔尔里亚尔|", "KWD|科威特第纳尔|",
        "BHD|巴林第纳尔|", "OMR|阿曼里亚尔|", "JOD|约旦第纳尔|", "MAD|摩洛哥迪拉姆|",
        "IRR|伊朗里亚尔|", "IQD|伊拉克第纳尔|", "UZS|乌兹别克斯坦苏姆|", "BND|文莱元|",
        "FJD|斐济元|", "RSD|塞尔维亚第纳尔|", "GEL|格鲁吉亚拉里|₾", "AMD|亚美尼亚德拉姆|",
        "AZN|阿塞拜疆马纳特|₼", "BYN|白俄罗斯卢布|", "TZS|坦桑尼亚先令|", "UGX|乌干达先令|",
        "ETB|埃塞俄比亚比尔|", "GHS|加纳塞地|₵", "DZD|阿尔及利亚第纳尔|", "TND|突尼斯第纳尔|",
        "VES|委内瑞拉玻利瓦尔|", "UYU|乌拉圭比索|", "BOB|玻利维亚诺|", "PYG|巴拉圭瓜拉尼|₲",
        "CRC|哥斯达黎加科朗|₡", "DOP|多米尼加比索|", "JMD|牙买加元|", "TTD|特立尼达和多巴哥元|",
        "XAF|中非法郎|", "XOF|西非法郎|", "MVR|马尔代夫拉菲亚|", "AFN|阿富汗尼|",
        "KGS|吉尔吉斯斯坦索姆|", "TJS|塔吉克斯坦索莫尼|", "TMT|土库曼斯坦马纳特|", "MKD|北马其顿代纳尔|",
        "ALL|阿尔巴尼亚列克|", "BAM|波黑可兑换马克|", "MDL|摩尔多瓦列伊|", "LBP|黎巴嫩镑|",
        "SYP|叙利亚镑|", "YER|也门里亚尔|", "LYD|利比亚第纳尔|", "SDG|苏丹镑|",
        "ZMW|赞比亚克瓦查|", "MZN|莫桑比克梅蒂卡尔|", "AOA|安哥拉宽扎|", "NAD|纳米比亚元|",
        "BWP|博茨瓦纳普拉|", "MUR|毛里求斯卢比|", "SCR|塞舌尔卢比|", "PGK|巴布亚新几内亚基那|",
        "XPF|太平洋法郎|", "BSD|巴哈马元|", "BBD|巴巴多斯元|", "BZD|伯利兹元|",
        "BMD|百慕大元|", "KYD|开曼群岛元|", "ANG|荷属安的列斯盾|", "AWG|阿鲁巴弗罗林|",
        "HNL|洪都拉斯伦皮拉|", "GTQ|危地马拉格查尔|", "NIO|尼加拉瓜科多巴|", "PAB|巴拿马巴波亚|",
        "HTG|海地古德|", "CUP|古巴比索|", "XCD|东加勒比元|", "RWF|卢旺达法郎|",
        "BIF|布隆迪法郎|", "CDF|刚果法郎|", "MGA|马达加斯加阿里亚里|", "MWK|马拉维克瓦查|",
        "SLE|塞拉利昂利昂|", "GMD|冈比亚达拉西|", "GNF|几内亚法郎|", "LRD|利比里亚元|",
        "SOS|索马里先令|", "DJF|吉布提法郎|", "ERN|厄立特里亚纳克法|", "SZL|斯威士兰里兰吉尼|",
        "LSL|莱索托洛蒂|", "CVE|佛得角埃斯库多|", "STN|圣多美多布拉|", "KMF|科摩罗法郎|",
        "MRU|毛里塔尼亚乌吉亚|", "BTN|不丹努尔特鲁姆|", "WST|萨摩亚塔拉|", "TOP|汤加潘加|",
        "VUV|瓦努阿图瓦图|", "SBD|所罗门群岛元|", "GYD|圭亚那元|", "SRD|苏里南元|",
        "FKP|福克兰群岛镑|", "GIP|直布罗陀镑|", "SHP|圣赫勒拿镑|", "IMP|马恩岛镑|",
        "JEP|泽西岛镑|", "GGP|根西岛镑|", "FOK|法罗群岛克朗|", "TVD|图瓦卢元|",
        "KID|基里巴斯元|", "XDR|特别提款权|", "ZWL|津巴布韦元|",
    ]

    private static let info: [String: (name: String, symbol: String)] = {
        var d: [String: (name: String, symbol: String)] = [:]
        for line in table {
            let p = line.split(separator: "|", omittingEmptySubsequences: false).map(String.init)
            d[p[0]] = (p[1], p[2])
        }
        return d
    }()

    static let ordered: [String] = table.map { String($0.prefix(while: { $0 != "|" })) }
    private static let orderIndex: [String: Int] = Dictionary(uniqueKeysWithValues: ordered.enumerated().map { ($1, $0) })

    private static let zeroDecimals: Set<String> = [
        "JPY", "KRW", "VND", "IDR", "CLP", "ISK", "PYG", "UGX", "KHR", "LAK", "IRR", "MMK", "UZS", "GNF"
    ]

    private static let fixedColors: [String: UInt32] = [
        "CNY": 0xE5484D, "HKD": 0x8E4EC6, "JPY": 0xF76B15, "USD": 0x30A46C, "EUR": 0x3E63DD, "GBP": 0x0D74CE
    ]

    private static let palette: [UInt32] = [
        0x12A594, 0xD6409F, 0x6E56CF, 0xE54666, 0x0090FF, 0xAD7F58, 0x46A758, 0xFFA01C, 0x5B5BD6, 0x00A2C7
    ]

    static func isKnown(_ code: String) -> Bool { info[code] != nil }

    static func name(_ code: String) -> String { info[code]?.name ?? code }

    static func symbol(_ code: String) -> String {
        if let s = info[code]?.symbol, !s.isEmpty { return s }
        return String(code.prefix(2))
    }

    static func decimals(_ code: String) -> Int { zeroDecimals.contains(code) ? 0 : 2 }

    /// 已知货币按推荐顺序排在前面，其余按代码字母序
    static func sortKey(_ code: String) -> (Int, String) { (orderIndex[code] ?? Int.max, code) }

    static func color(_ code: String) -> Color {
        var hex = fixedColors[code]
        if hex == nil {
            var h: Int32 = 0
            for u in code.utf16 { h = h &* 31 &+ Int32(u) }
            hex = palette[Int(abs(Int64(h)) % Int64(palette.count))]
        }
        let v = hex!
        return Color(red: Double((v >> 16) & 0xFF) / 255, green: Double((v >> 8) & 0xFF) / 255, blue: Double(v & 0xFF) / 255)
    }
}
