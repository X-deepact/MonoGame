@echo off
cd /d C:\dev\game\farmbelt
set projectPath=TheBloomInitiative.csproj
set gameName=TheBloomInitiative

:: Menu for build selection
:menu
cls
echo Build Options for %gameName%
echo =====================================================
echo 1. Build for Windows x64
echo 2. Build for macOS Intel (x64)
echo 3. Build for macOS Apple Silicon (ARM64)
echo 4. Build All Platforms
echo 5. Exit
echo =====================================================
set /p choice="Enter your choice (1-5): "

if "%choice%"=="5" exit
if "%choice%"=="1" goto build_win
if "%choice%"=="2" goto build_mac_intel
if "%choice%"=="3" goto build_mac_silicon
if "%choice%"=="4" goto build_all
echo Invalid choice. Please try again.
timeout /t 2 >nul
goto menu

:build_win
echo Building for Windows x64...
call :clean_specific win-x64
call :create_directories win-x64
call :build_windows
goto end

:build_mac_intel
echo Building for macOS Intel...
call :clean_specific osx-x64
call :create_directories osx-x64
call :build_mac_x64
goto end

:build_mac_silicon
echo Building for macOS Apple Silicon...
call :clean_specific osx-arm64
call :create_directories osx-arm64
call :build_mac_arm64
goto end

:build_all
echo Building for all platforms...
call :clean_all
call :create_directories_all
call :build_all_platforms
goto end

:: Subroutines
:clean_specific
if exist "%~dp0publish\%~1" rmdir /s /q "%~dp0publish\%~1"
if exist "%~dp0releases" rmdir /s /q "%~dp0releases"
mkdir "%~dp0publish\%~1"
mkdir "%~dp0releases"
goto :eof

:clean_all
echo Cleaning previous debug builds...
if exist "%~dp0publish" rmdir /s /q "%~dp0publish"
if exist "%~dp0releases" rmdir /s /q "%~dp0releases"
goto :eof

:create_directories
mkdir "%~dp0publish\%~1"
mkdir "%~dp0releases"
goto :eof

:create_directories_all
echo Creating directories...
mkdir "%~dp0publish\osx-x64"
mkdir "%~dp0publish\osx-arm64"
mkdir "%~dp0publish\win-x64"
mkdir "%~dp0releases"
goto :eof

:build_windows
echo.
echo Publishing Debug version for Windows x64...
dotnet publish %projectPath% -c Debug -r win-x64 -p:PublishReadyToRun=false -p:TieredCompilation=false -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true --self-contained -o "%~dp0publish\win-x64"
echo Copying levels folder to win-x64...
xcopy /E /I "%~dp0levelscripts" "%~dp0publish\win-x64\levels"

:: Create Windows README
(
echo Windows x64 Debug Version
echo ------------------
echo Simply run %gameName%.exe to start the game.
echo.
echo If you get any security warnings, right-click the executable and select "Properties",
echo then check the "Unblock" box and click OK.
) > "%~dp0publish\win-x64\README.txt"

echo Creating zip file...
powershell -Command "Compress-Archive -Path '%~dp0publish\win-x64\*' -DestinationPath '%~dp0releases\%gameName%-Windows-x64-Debug-Version-%version%.zip' -Force"
goto :eof

:build_mac_x64
echo.
echo Publishing Debug version for macOS x64...
dotnet publish %projectPath% -c Debug -r osx-x64 -p:PublishReadyToRun=false -p:TieredCompilation=false -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true --self-contained -o "%~dp0publish\osx-x64"
echo Copying levels folder to osx-x64...
xcopy /E /I "%~dp0levelscripts" "%~dp0publish\osx-x64\levels"

:: Create macOS x64 setup script
(
echo #!/bin/bash
echo.
echo # Check if running with sudo
echo if [ "$EUID" -ne 0 ]; then
echo   echo "Please run this script with sudo: sudo ./setup.sh"
echo   exit 1
echo fi
echo.
echo echo "Setting up %gameName% for macOS..."
echo.
echo # Remove quarantine attributes if they exist
echo echo "Removing quarantine attributes..."
echo if [ -f "%gameName%" ]; then
echo   xattr -r -d com.apple.quarantine "%gameName%" 2^>/dev/null ^|^| true
echo else
echo   echo "Warning: %gameName% executable not found!"
echo fi
echo.
echo if [ -f "libSDL2.dylib" ]; then
echo   xattr -r -d com.apple.quarantine "libSDL2.dylib" 2^>/dev/null ^|^| true
echo fi
echo.
echo if [ -f "libopenal.1.dylib" ]; then
echo   xattr -r -d com.apple.quarantine "libopenal.1.dylib" 2^>/dev/null ^|^| true
echo fi
echo.
echo # Set executable permissions
echo echo "Setting executable permissions..."
echo chmod +x "%gameName%" 2^>/dev/null ^|^| echo "Warning: Could not set executable permissions on %gameName%"
echo.
echo echo "Setup completed! You can now run ./%gameName%"
) > "%~dp0publish\osx-x64\setup.sh"

:: Fixed line ending conversion
powershell -Command "(Get-Content '%~dp0publish\osx-x64\setup.sh' -Raw) | ForEach-Object { $_ -replace \"`r`n\", \"`n\" } | Set-Content '%~dp0publish\osx-x64\setup.sh' -NoNewline -Force -Encoding UTF8"

:: Create macOS x64 README
(
echo Mac Intel Debug Version
echo ---------------
echo 1. Open Terminal in this folder
echo 2. Run the following command:
echo    sudo ./setup.sh
echo.
echo If you get a "Permission denied" error when running setup.sh, first run:
echo    chmod +x setup.sh
echo.
echo 3. After setup completes, run the game with:
echo    ./%gameName%
) > "%~dp0publish\osx-x64\README.txt"

echo Creating zip file...
powershell -Command "Compress-Archive -Path '%~dp0publish\osx-x64\*' -DestinationPath '%~dp0releases\%gameName%-MacOS-Intel-Debug-Version-%version%.zip' -Force"
goto :eof

:build_mac_arm64
echo.
echo Publishing Debug version for macOS ARM64...
dotnet publish %projectPath% -c Debug -r osx-arm64 -p:PublishReadyToRun=false -p:TieredCompilation=false -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true --self-contained -o "%~dp0publish\osx-arm64"
echo Copying levels folder to osx-arm64...
xcopy /E /I "%~dp0levelscripts" "%~dp0publish\osx-arm64\levels"

:: Create macOS ARM64 setup script
(
echo #!/bin/bash
echo.
echo # Check if running with sudo
echo if [ "$EUID" -ne 0 ]; then
echo   echo "Please run this script with sudo: sudo ./setup.sh"
echo   exit 1
echo fi
echo.
echo echo "Setting up %gameName% for macOS..."
echo.
echo # Remove quarantine attributes if they exist
echo echo "Removing quarantine attributes..."
echo if [ -f "%gameName%" ]; then
echo   xattr -r -d com.apple.quarantine "%gameName%" 2^>/dev/null ^|^| true
echo else
echo   echo "Warning: %gameName% executable not found!"
echo fi
echo.
echo if [ -f "libSDL2.dylib" ]; then
echo   xattr -r -d com.apple.quarantine "libSDL2.dylib" 2^>/dev/null ^|^| true
echo fi
echo.
echo if [ -f "libopenal.1.dylib" ]; then
echo   xattr -r -d com.apple.quarantine "libopenal.1.dylib" 2^>/dev/null ^|^| true
echo fi
echo.
echo # Set executable permissions
echo echo "Setting executable permissions..."
echo chmod +x "%gameName%" 2^>/dev/null ^|^| echo "Warning: Could not set executable permissions on %gameName%"
echo.
echo echo "Setup completed! You can now run ./%gameName%"
) > "%~dp0publish\osx-arm64\setup.sh"

:: Fixed line ending conversion
powershell -Command "(Get-Content '%~dp0publish\osx-arm64\setup.sh' -Raw) | ForEach-Object { $_ -replace \"`r`n\", \"`n\" } | Set-Content '%~dp0publish\osx-arm64\setup.sh' -NoNewline -Force -Encoding UTF8"

:: Create macOS ARM64 README
(
echo Mac Apple Silicon Debug Version
echo ----------------------
echo 1. Open Terminal in this folder
echo 2. Run the following command:
echo    sudo ./setup.sh
echo.
echo If you get a "Permission denied" error when running setup.sh, first run:
echo    chmod +x setup.sh
echo.
echo 3. After setup completes, run the game with:
echo    ./%gameName%
) > "%~dp0publish\osx-arm64\README.txt"

echo Creating zip file...
powershell -Command "Compress-Archive -Path '%~dp0publish\osx-arm64\*' -DestinationPath '%~dp0releases\%gameName%-MacOS-AppleSilicon-Debug-Version-%version%.zip' -Force"
goto :eof

:build_all_platforms
call :build_windows
call :build_mac_x64
call :build_mac_arm64
goto :eof

:end
:: Retrieve version from .csproj file (moved here to ensure it's available for all paths)
for /f "tokens=*" %%i in ('powershell -Command "(Select-Xml -Path '%projectPath%' -XPath '//Version').Node.InnerText"') do set "version=%%i"

echo.
echo Build process completed!
echo.
echo Created files in the 'releases' folder:
if exist "%~dp0releases\%gameName%-Windows-x64-Debug-Version-%version%.zip" echo - %gameName%-Windows-x64-Debug-Version-%version%.zip
if exist "%~dp0releases\%gameName%-MacOS-Intel-Debug-Version-%version%.zip" echo - %gameName%-MacOS-Intel-Debug-Version-%version%.zip
if exist "%~dp0releases\%gameName%-MacOS-AppleSilicon-Debug-Version-%version%.zip" echo - %gameName%-MacOS-AppleSilicon-Debug-Version-%version%.zip
echo.
echo Opening releases folder...
explorer "%~dp0releases"
pause
goto menu