@echo off
chcp 65001 >nul
echo [Pointer] 正在构建 macOS 26 纯绿色指示器单文件...

set CSC_PATH=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe

if not exist "%CSC_PATH%" (
    echo [错误] 未找到系统内置 C# 编译器: %CSC_PATH%
    pause
    exit /b 1
)

"%CSC_PATH%" /nologo /target:winexe /platform:anycpu /optimize+ /out:"%~dp0Pointer.exe" /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:Accessibility.dll "%~dp0src\*.cs"

if %ERRORLEVEL% equ 0 (
    echo [成功] Pointer.exe 构建完成！体积仅约 28KB，纯绿色免安装单文件。
) else (
    echo [失败] 构建过程发生错误，错误码: %ERRORLEVEL%
)

pause
