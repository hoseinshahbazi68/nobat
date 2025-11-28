@echo off
echo Building Nobat API...
cd src\Nobat.API
dotnet build
if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build completed successfully!
) else (
    echo.
    echo Build failed with errors!
    exit /b %ERRORLEVEL%
)
cd ..\..
