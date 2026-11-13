# 输入指示器 (IME Indicator) 🎯

<p align="center">
  <strong>基于 macOS 26 晶透美学重构 · 100% 纯绿色免安装 · 零依赖零残留 · 毫秒级光标与鼠标跟随</strong>
  <br />
  <em>A minimalist, ultra-lightweight Windows IME indicator inspired by macOS 26 design aesthetics.</em>
</p>

<p align="center">
  <a href="https://github.com/Crucialmer/Pointer/releases"><img src="https://img.shields.io/github/v/release/Crucialmer/Pointer?color=007AFF&label=Release" alt="Release"></a>
  <img src="https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D4" alt="Platform">
  <img src="https://img.shields.io/badge/Size-~28%20KB-success" alt="Size">
  <img src="https://img.shields.io/badge/Design-macOS%2026%20Aesthetics-black" alt="macOS 26">
  <img src="https://img.shields.io/badge/Portable-100%25%20Pure%20Green-success" alt="Pure Green">
  <img src="https://img.shields.io/badge/License-MIT-brightgreen" alt="License">
</p>

---

一个轻量级的 Windows 输入法中英状态实时提示工具。在文本光标和鼠标底部用精致彩色微胶囊指示中英状态，极尽简洁克制但有效的提示。

## 🌟 核心特性

- 🌿 **100% 纯绿色免安装**：单一可执行文件仅 **~28 KB**，无需额外运行时，解压即用；所有配置文件（`config.ini`）**严格且仅保存在程序同级目录**，零注册表垃圾、零系统目录文件外溢。
- 🎨 **macOS 26 晶透灵动美学**：
  - **macOS 晶透微标 (`badge`)**：连续平滑圆角胶囊（Squircle），显示清晰锐利的 **「中」** 或 **「A」** 微标，内嵌 1px 钛晶高光微亮边与多阶环境发光软阴影。
  - **极光呼吸微点 (`dot`)**：双层微光漫反射圆点，极度克制优雅。
  - **呼吸式平滑淡入淡出**：毫秒级 Alpha 缓动插值，彻底消除原版光标移动时的瞬间生硬频闪。
- 🌓 **跟随系统深浅色自适应 (Auto Light/Dark Mode)**：
  - 毫秒级感知 Windows 10/11 系统的深色与浅色模式并实时切换调色盘：
    - ☀️ **浅色模式**：Apple Amber 暖日橙（中文） + Apple Azure 雅致深蓝（英文）；
    - 🌙 **深色模式**：Apple Laser Gold 激光琥珀黄（中文） + Apple Neon Cyan 激光冰青蓝（英文）。
- ⚡ **1ms 硬件级调度与双模追踪**：
  - 突破 Windows 默认 15.6ms 时钟限制，在 120Hz/144Hz/240Hz 高刷屏上呈现丝滑的无延迟跟随。
  - 优先检测文本光标（Win32 Caret + MSAA OBJID_CARET，覆盖记事本、Word、Chrome、VS Code 等主流编辑器）；在无法获取文本光标时，智能回退为鼠标指针跟随。
- 📐 **顶级高 DPI 感知 (PerMonitorV2)**：在 4K/8K 屏幕及多显示器不同缩放比例间自由拖动，微标始终保持数学级高清锐利，绝不发虚。

---

## 📂 项目结构

本仓库包含多种实现与架构：

- **[macos_indicator/](./macos_indicator/) (最新推荐 · macOS 26 纯绿重构版)**:
  - 基于 Windows 原生 API 与 GDI+ 预乘 Alpha 抗锯齿渲染。
  - **单文件免安装运行**：单一可执行程序仅 **~28 KB**，零运行时依赖。
  - **macOS 26 动态胶囊**：支持「中/A」文字微标与极光圆点双形态、深浅色模式自适应与平滑淡入淡出。
  - **系统托盘**：macOS 风格右键菜单，支持形态切换、主题切换、配置编辑与一键绿色自启动。

- **[rust_indicator/](./rust_indicator/) (Rust 原生版)**:
  - 使用 Rust + Win32 API 开发。
  - 单文件运行（约 300KB），支持后台运行与右键菜单。

- **[python_indicator/](./python_indicator/) (Python 参考版)**:
  - 原始的 Python + ctypes 原型实现。

---

## 🚀 直接下载运行 (快速验证)

前往 **[Releases 页面](https://github.com/Crucialmer/Pointer/releases)** 下载最新发布的绿色单文件压缩包，解压后双击 `Pointer.exe` 即可使用。

---

## 🛠️ 自行编译 (macos_indicator)

直接双击运行 `macos_indicator/build.bat` 即可在 1 秒内完成单文件编译。

---

## 📄 开源许可证

本项目基于 [MIT License](LICENSE) 开源。
