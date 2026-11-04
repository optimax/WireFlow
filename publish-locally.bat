@echo off
rem Publishes the 64-bit WireFlow Designer, built in Release mode, to c:\Programs\Optimax\WireFlow.
rem The output is framework-dependent: the machine running it needs the .NET 10 Desktop Runtime (x64).
setlocal
set "PUBLISH_DIR=c:\Programs\Optimax\WireFlow"

rem dotnet publish copies a file only when it is newer than the one already in the folder, so after a package
rem downgrade the old, newer DLLs would stay. Start from an empty folder instead.
if exist "%PUBLISH_DIR%\" (
    rmdir /s /q "%PUBLISH_DIR%"
    if exist "%PUBLISH_DIR%\" (
        echo Could not empty %PUBLISH_DIR%. Close WireFlow Designer and try again.
        exit /b 1
    )
)

dotnet publish "%~dp0WireFlow.Designer\WireFlow.Designer.csproj" -c Release -r win-x64 --self-contained false -o "%PUBLISH_DIR%"
if errorlevel 1 (
    echo Publishing WireFlow Designer failed.
    exit /b 1
)

echo WireFlow Designer published to %PUBLISH_DIR%.
