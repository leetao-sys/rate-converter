# 汇率换算 · iOS 版

与 Windows 版功能一致的 iPhone / iPad 汇率换算小工具，用 SwiftUI 编写，安装包约 1 MB。

- 默认显示 **人民币 CNY、港币 HKD、日元 JPY、美元 USD**，点“添加货币”可搜索添加其他约 160 种货币
- 修改任意一种货币的金额，其余货币立即同步换算
- 键盘上方有 `+ − × ÷ =` 按钮，可直接输入 `100×3+50` 这样的算式；也支持千分位、全角数字
- 向左滑动可删除货币，点左上角“编辑”可拖动排序
- 打开 App 时自动获取最新汇率，之后在前台每 30 分钟检查一次，也可下拉或点右上角刷新；断网时使用上次缓存的汇率
- 自动适配深色模式，记住上次的货币列表和金额

汇率来源与 Windows 版相同：[ExchangeRate-API 开放接口](https://www.exchangerate-api.com/docs/free)（主源）+ [fawazahmed0/currency-api](https://github.com/fawazahmed0/exchange-api)（备用），免费、每日更新，仅供参考。

## 不需要 Mac：用 GitHub 云端编译

仓库里的 `.github/workflows/ios.yml` 会在 GitHub 提供的云端 Mac 上自动编译（公开仓库免费），每次修改 `ios/` 目录都会重新编译一次，生成一个**未签名**的安装包 `RateConverter-unsigned.ipa`。

下载方法：

1. 打开仓库的 **Actions** 页面 → 点最新一次成功（绿色 ✓）的 “iOS build”
2. 页面底部 **Artifacts** 里点 `RateConverter-unsigned-ipa` 下载（需要登录 GitHub），解压得到 `.ipa`

如果推送一个以 `ios-v` 开头的标签（如 `ios-v1.0.0`），还会自动发布到 **Releases** 页面，下载更方便。

## 安装到 iPhone

苹果不允许直接安装未签名的 App，需要用你的 Apple ID 签名一次。按是否付费有两种方式：

### 方式一：免费 Apple ID + Windows 电脑侧载（推荐先用这个）

只需要你平时用的 Apple ID，不用花钱。

1. 在 Windows 上安装 **iTunes** 和 **iCloud**（请从苹果官网下载“非 Microsoft Store 版”，Sideloadly 需要它们的驱动）
2. 安装 [Sideloadly](https://sideloadly.io/)（免费）。也可以用国内常见的爱思助手的“IPA 签名”功能，原理相同
3. 用数据线连接 iPhone，手机上点“信任此电脑”
4. 打开 Sideloadly，把 `RateConverter-unsigned.ipa` 拖进去，填写你的 Apple ID，点 **Start**，按提示输入密码和双重认证验证码
5. 在 iPhone 上：
   - **设置 → 隐私与安全性 → 开发者模式** 打开并重启（iOS 16 及以上需要）
   - **设置 → 通用 → VPN 与设备管理**，找到你的 Apple ID，点“信任”
6. 桌面上出现“汇率换算”图标，即可使用

注意：免费 Apple ID 签名的 App **7 天后会失效**（打不开，但数据不会丢），需要用 Sideloadly 再装一次；Sideloadly 开启“自动刷新”并保持电脑开机、和手机在同一 Wi‑Fi 下可自动续签。一个免费 Apple ID 最多同时侧载 3 个 App。

### 方式二：付费苹果开发者账号（每年 ¥688 / $99）

在 [developer.apple.com](https://developer.apple.com/programs/) 注册 Apple Developer Program 后：

- 用同样的方法侧载，签名有效期变为 **1 年**；或者
- 通过 **TestFlight** 安装：电脑不用连接手机，在 iPhone 上装 TestFlight App 后像普通 App 一样安装和更新，每个版本有效 90 天，还可以邀请家人朋友一起用。这需要在 GitHub 仓库里配置开发者账号的签名证书和 App Store Connect API 密钥，之后可以让 GitHub Actions 自动上传

## 在 Mac 上开发（可选）

```bash
brew install xcodegen
cd ios
xcodegen generate     # 生成 RateConverter.xcodeproj
open RateConverter.xcodeproj
```

修改图标：编辑并运行 `python3 ios/tools/make_icon.py`（需要 Pillow）。

## 目录

| 路径 | 说明 |
| --- | --- |
| `RateConverter/ContentView.swift` | 主界面：货币列表、键盘运算按钮、刷新 |
| `RateConverter/AddCurrencyView.swift` | 添加货币（可搜索） |
| `RateConverter/AppModel.swift` | 换算逻辑、设置保存、自动刷新 |
| `RateConverter/RateService.swift` | 获取汇率（主源 + 备用源） |
| `RateConverter/Currencies.swift` | 货币中文名、符号、小数位、配色 |
| `RateConverter/Calc.swift` | 输入解析（含四则运算）与数字格式化 |
| `RateConverterTests/` | 单元测试 |
| `project.yml` | XcodeGen 工程配置 |
