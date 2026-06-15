@echo off
echo === DNote Build Script ===

echo.
echo [1/3] Restoring packages...
dotnet restore DNote\DNote.csproj
if %errorlevel% neq 0 exit /b %errorlevel%

echo.
echo [2/3] Building Release...
dotnet build DNote\DNote.csproj -c Release --no-restore
if %errorlevel% neq 0 exit /b %errorlevel%

echo.
echo [3/3] Publishing self-contained...
dotnet publish DNote\DNote.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\portable
if %errorlevel% neq 0 exit /b %errorlevel%

echo.
echo === Build Complete ===
echo Output: publish\portable\DNote.exe
echo.
pause
