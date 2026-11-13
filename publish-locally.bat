@echo off
rem Publishes the 64-bit WireFlow Designer, built in Release mode, to c:\Programs\Optimax\WireFlow.
rem The output is framework-dependent: the machine running it needs the .NET 10 Desktop Runtime (x64).
rem The Designer is published into an empty staging folder first, and replaces the published one only when that
rem succeeds, so a failed build leaves the previous version in place.
setlocal
set "PUBLISH_DIR=c:\Programs\Optimax\WireFlow"
set "STAGING_DIR=%PUBLISH_DIR%.new"
set "OLD_DIR=%PUBLISH_DIR%.old"

rem dotnet publish copies a file only when it is newer than the one already in the folder, so after a package
rem downgrade the old, newer DLLs would stay. Publish into an empty folder instead.
if exist "%STAGING_DIR%\" rmdir /s /q "%STAGING_DIR%"
if exist "%OLD_DIR%\" rmdir /s /q "%OLD_DIR%"
if exist "%STAGING_DIR%\" (
    echo Could not empty %STAGING_DIR%.
    exit /b 1
)

dotnet publish "%~dp0WireFlow.Designer\WireFlow.Designer.csproj" -c Release -r win-x64 --self-contained false -o "%STAGING_DIR%"
if errorlevel 1 (
    rmdir /s /q "%STAGING_DIR%" 2>nul
    echo Publishing WireFlow Designer failed. %PUBLISH_DIR% is unchanged.
    exit /b 1
)

rem Swap the folders. Moving the published folder fails while the published Designer is running.
if exist "%PUBLISH_DIR%\" (
    move "%PUBLISH_DIR%" "%OLD_DIR%" >nul
    if errorlevel 1 (
        echo Could not replace %PUBLISH_DIR%. Close WireFlow Designer and try again.
        echo The new build is in %STAGING_DIR%.
        exit /b 1
    )
)
move "%STAGING_DIR%" "%PUBLISH_DIR%" >nul
if errorlevel 1 (
    move "%OLD_DIR%" "%PUBLISH_DIR%" >nul
    echo Could not move the new build into %PUBLISH_DIR%. The previous version is restored.
    exit /b 1
)
if exist "%OLD_DIR%\" rmdir /s /q "%OLD_DIR%"

echo WireFlow Designer published to %PUBLISH_DIR%.
timeout 10
