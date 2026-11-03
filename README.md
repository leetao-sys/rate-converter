# 汇率换算器

一个轻量的 Windows 桌面汇率换算小工具。另有 [iOS 版](ios/README.md)（iPhone / iPad）。

- 默认显示 **人民币 CNY、港币 HKD、日元 JPY、美元 USD**，可随时添加其他约 160 种货币，鼠标移到卡片上点 ✕ 可移除
- 修改任意一种货币的金额，其余货币立即同步换算
- 输入框支持千分位、全角数字和四则运算（如 `100*3+50`，回车得出结果）
- ↑ / ↓ / Tab 在货币之间切换，Esc 清空
- 窗口可置顶，记住上次的货币列表、金额和窗口位置
- 启动时自动获取最新汇率，之后每 30 分钟检查一次；断网时使用上次缓存的汇率
<img width="582" height="891" alt="image" src="https://github.com/user-attachments/assets/b67fe638-19c2-4c95-84bb-bc7851a10cbc" />



## 运行

双击 `dist\RateConverter.exe` 即可。只需要 Windows 10/11 自带的 .NET Framework 4.8，无需安装任何东西。

程序约 90 KB，后台时内存占用约 10–25 MB。

## 汇率数据来源

均为免费、无需 API Key 的公开接口：

1. [ExchangeRate-API 开放接口](https://www.exchangerate-api.com/docs/free)（主源）
2. [fawazahmed0/currency-api](https://github.com/fawazahmed0/exchange-api)（备用）

免费数据源每天更新一次，适合日常参考，不适合用于交易报价。

## 从源码构建

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

使用 Windows 自带的 C# 编译器（`%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe`），生成 `dist\RateConverter.exe`。

修改图标后运行 `tools\make-icon.ps1` 重新生成 `assets\app.ico`。

## 目录

| 路径 | 说明 |
| --- | --- |
| `src\MainWindow.cs` | 主界面与换算逻辑 |
| `src\AddCurrencyWindow.cs` | 添加货币对话框 |
| `src\RateService.cs` | 获取汇率（主源 + 备用源） |
| `src\Currencies.cs` | 货币中文名、符号、小数位 |
| `src\Calc.cs` | 输入解析与数字格式化 |
| `src\Store.cs` | 设置与汇率缓存（保存在 `%APPDATA%\RateConverter`） |
| `src\Ui.cs` | 配色与控件样式 |
