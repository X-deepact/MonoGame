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
echo Cleaning previous release builds...
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
echo Publishing Release version for Windows x64...
dotnet publish %projectPath% -c Release -r win-x64 -p:PublishReadyToRun=true -p:TieredCompilation=false -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true --self-contained -o "%~dp0publish\win-x64"

:: Create Windows README
(
echo Windows x64 Release Version
echo ------------------
echo Simply run %gameName%.exe to start the game.
echo.
echo If you get any security warnings, right-click the executable and select "Properties",
echo then check the "Unblock" box and click OK.
) > "%~dp0publish\win-x64\README.txt"

echo Creating zip file...
powershell -Command "Compress-Archive -Path '%~dp0publish\win-x64\*' -DestinationPath '%~dp0releases\%gameName%-Windows-x64-Release-%version%.zip' -Force"
goto :eof

:build_mac_x64
echo.
echo Publishing Release version for macOS x64...
dotnet publish %projectPath% -c Release -r osx-x64 -p:PublishReadyToRun=true -p:TieredCompilation=false -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true --self-contained -o "%~dp0publish\osx-x64"

:: Create macOS x64 README
(
echo Mac Intel Release Version
echo ---------------
echo 1. Open Terminal in this folder
echo 2. Run the game with:
echo    ./%gameName%
) > "%~dp0publish\osx-x64\README.txt"

echo Creating zip file...
powershell -Command "Compress-Archive -Path '%~dp0publish\osx-x64\*' -DestinationPath '%~dp0releases\%gameName%-MacOS-Intel-Release-%version%.zip' -Force"
goto :eof

:build_mac_arm64
echo.
echo Publishing Release version for macOS ARM64...
dotnet publish %projectPath% -c Release -r osx-arm64 -p:PublishReadyToRun=true -p:TieredCompilation=false -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true --self-contained -o "%~dp0publish\osx-arm64"

:: Create macOS ARM64 README
(
echo Mac Apple Silicon Release Version
echo ----------------------
echo 1. Open Terminal in this folder
echo 2. Run the game with:
echo    ./%gameName%
) > "%~dp0publish\osx-arm64\README.txt"

echo Creating zip file...
powershell -Command "Compress-Archive -Path '%~dp0publish\osx-arm64\*' -DestinationPath '%~dp0releases\%gameName%-MacOS-AppleSilicon-Release-%version%.zip' -Force"
goto :eof

:build_all_platforms
call :build_windows
call :build_mac_x64
call :build_mac_arm64
goto :eof

:end
:: Retrieve version from .csproj file
for /f "tokens=*" %%i in ('powershell -Command "(Select-Xml -Path '%projectPath%' -XPath '//Version').Node.InnerText"') do set "version=%%i"

echo.
echo Build process completed!
echo.
echo Created files in the 'releases' folder:
if exist "%~dp0releases\%gameName%-Windows-x64-Release-%version%.zip" echo - %gameName%-Windows-x64-Release-%version%.zip
if exist "%~dp0releases\%gameName%-MacOS-Intel-Release-%version%.zip" echo - %gameName%-MacOS-Intel-Release-%version%.zip
if exist "%~dp0releases\%gameName%-MacOS-AppleSilicon-Release-%version%.zip" echo - %gameName%-MacOS-AppleSilicon-Release-%version%.zip
echo.
echo Opening releases folder...
explorer "%~dp0releases"
pause
goto menu
