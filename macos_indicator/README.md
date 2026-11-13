# Pointer: Windows 输入法中英状态指示器 (macOS 26 纯绿色重构版)

<p align="center">
  <strong>基于 macOS 26 晶透美学重构 · 100% 纯绿色免安装 · 零依赖零残留 · 毫秒级光标与鼠标跟随</strong>
  <br />
  <em>A minimalist, ultra-lightweight Windows IME indicator inspired by macOS 26 design aesthetics.</em>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011-0078D4?style=flat-square&logo=windows" alt="Platform" />
  <img src="https://img.shields.io/badge/Size-28%20KB-success?style=flat-square" alt="Size" />
  <img src="https://img.shields.io/badge/Design-macOS%2026%20Aesthetics-black?style=flat-square&logo=apple" alt="macOS 26" />
  <img src="https://img.shields.io/badge/Mode-100%25%20Pure%20Green-brightgreen?style=flat-square" alt="Pure Green" />
  <img src="https://img.shields.io/badge/Zero%20Dependency-Framework%20Built--in-blue?style=flat-square" alt="Zero Dependency" />
</p>

---

## 🌟 为什么重构 Pointer？

原版 [Crucialmer/Pointer](https://github.com/Crucialmer/Pointer) 是一个非常优秀的极简输入法状态指示工具，但原版存在两处核心进阶需求：
1. **纯绿色化规范缺失**：虽然读取了本地配置，但存在快捷方式漂移工作目录隐患，且缺乏规范的绿色自启与清理机制；
2. **视觉设计粗糙**：原版仅绘制了简单的单色实心圆点，显示与隐藏为瞬间硬切换（频闪感强烈），缺乏高 DPI 渲染细腻度与现代桌面美感。

**Pointer (macOS 26 纯绿重构版)** 彻底解决了以上痛点，打造了极致纯粹、灵动且精致的体验。

---

## ✨ 核心特性

### 1. 🌿 100% 纯绿色免安装与零残留 (Pure Portable)
- **单一可执行文件仅 ~28 KB**：无需任何安装程序，无需额外下载庞大的运行时，解压/拷贝即用。
- **配置与数据严格局限**：产生的配置文件（`config.ini`）**严格且仅保存在程序所在文件夹**。
- **绝对不污染系统**：不往 `%APPDATA%`、`%LOCALAPPDATA%`、`Temp` 丢弃任何配置文件或缓存碎片。
- **绿色自启动**：可在托盘菜单中一键开关开机启动，由当前用户注册表独立接管（随用随走，关闭即可 100% 擦除干净，不留尾巴）。

### 2. 🎨 macOS 26 晶透灵动美学重构 (Liquid Glass Capsule)
- **双模态自由切换**：
  - **模式一：macOS 晶透微标 (`badge`)**：在光标旁呈现精致的连续平滑圆角胶囊（Squircle），显示清晰锐利的 **「中」** 或 **「A」** 微标。包含 1px 钛晶内高光轮廓边框与柔和的多阶高斯漫反射环境投影。
  - **模式二：极光呼吸微点 (`dot`)**：在光标旁显示带有双层外光晕的灵动圆点，极度克制但不失高级感。
- **系统深浅色模式自适应 (Auto Light/Dark Mode)**：
  - 自动感知 Windows 10/11 的深色与浅色主题，毫秒级自适应调色盘：
    - ☀️ **浅色模式**：Apple Amber 暖日橙（中文） + Apple Azure 深蔚蓝（英文），白底文档下清晰分明；
    - 🌙 **深色模式**：Apple Laser Gold 激光琥珀黄（中文） + Apple Neon Cyan 激光冰青蓝（英文），暗色编辑器中通透莹润。
- **呼吸式平滑淡入淡出 (Smooth Alpha Transitions)**：
  - 彻底抛弃原版生硬的显示/隐藏频闪跳跃，采用毫秒级 Alpha 缓动插值，指示器在移动和显隐时光泽平滑流淌。

### 3. ⚡ 硬件级 1ms 调度与双模追踪 (Caret + Cursor)
- **1ms 高精度调度**：调用 `timeBeginPeriod(1)` 突破 Windows 默认 15.6ms 时钟限制，在高刷新率屏幕（120Hz/144Hz/240Hz）上呈现丝滑的无延迟跟随。
- **双引擎光标检测**：
  - 优先检测文本光标（Win32 Caret + MSAA OBJID_CARET，覆盖记事本、Word、Chrome、VS Code 等主流编辑器）；
  - 在无法获取文本光标时，智能回退为鼠标指针（I-Beam 工字型与常规指针）智能伴随。
- **顶级高 DPI 感知 (PerMonitorV2)**：在 4K/8K 屏幕与多显示器不同缩放比例间自由拖动，微标始终保持数学级高清锐利，绝不发虚。

---

## 📂 项目结构

```text
pointer-macos/
├── Pointer.exe              # 📦 纯绿色单文件可执行程序 (仅约 28 KB)
├── config.ini               # ⚙️ 纯绿色本地配置文件 (自动生成于当前目录)
├── build.bat                # 🛠️ 一键原生编译脚本 (双击 1 秒极速生成)
├── src/                     # 💻 完整源码目录
│   ├── Program.cs           # 程序入口、绿色工作目录锁定、单实例守护与调度
│   ├── Config.cs            # 本地 INI 配置读写与动态热重载
│   ├── ThemeManager.cs      # Windows 系统深浅色主题感知
│   ├── ImeDetector.cs       # Win32/Imm32 毫秒级输入法状态检测
│   ├── CaretDetector.cs     # 文本输入插入符 (Win32 + MSAA) 探测
│   ├── CursorDetector.cs    # 鼠标光标形状检测
│   ├── MacCapsuleOverlay.cs # macOS 26 晶透胶囊分层渲染引擎
│   └── TrayIconManager.cs   # 系统托盘管理与 macOS 风格右键菜单
└── README.md                # 📖 项目说明与使用指南
```

---

## 🚀 快速上手与验证

### 1. 运行体验
1. 直接双击运行 [Pointer.exe](file:///C:/Users/Agent/Documents/AntigravityWorkspace/pointer-macos/Pointer.exe)。
2. 在任意文本框（如记事本、浏览器地址栏、VS Code、搜索栏等）点击输入，即可在光标右下角看到 **macOS 26 风格晶透指示微标**（「中」/「A」）。
3. 切换中英文状态（按 `Shift` 或 Caps Lock），指示微标将以呼吸微动效自适应切换色彩与徽标。

### 2. 托盘菜单交互
右键点击 Windows 任务栏右下角蓝色圆角「P」托盘图标，可呼出控制菜单：
- **🎨 显示形态**：在「macOS 晶透微标「中/A」」与「极光呼吸微点」之间一键切换；
- **🌓 色彩模式**：自由选择「跟随系统自适应」、「浅色模式」或「深色模式」；
- **🎯 追踪模式**：支持「智能双追踪」、「仅文本光标」或「仅鼠标指针」；
- **🔤 英文状态显示**：可自由勾选是否在英文模式下显示（关闭后仅在中文模式提示）；
- **⚙️ 编辑配置文件**：一键记事本打开同目录下的 [config.ini](file:///C:/Users/Agent/Documents/AntigravityWorkspace/pointer-macos/config.ini)。

---

## 🛠️ 本地编译构建 (1 秒极速重编译)

本项目基于 Windows 10/11 内置的原生 C# 编译器开发，无需安装任何额外 IDE 或 SDK。
直接双击运行 [build.bat](file:///C:/Users/Agent/Documents/AntigravityWorkspace/pointer-macos/build.bat) 即可在 1 秒内完成编译并生成单一免安装的 `Pointer.exe`。

---

## 📄 开源许可证

本项目基于 [MIT License](https://opensource.org/licenses/MIT) 开源。
欢迎自由修改、分发及二次创作！
