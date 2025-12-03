@echo off
REM TidyPackRat Launcher - Portable Edition
REM "Sorting your files, to clean up your mess."

echo.
echo ================================================================================
echo   TidyPackRat Configuration Tool
echo   Portable Edition v1.1.0
echo ================================================================================
echo.

REM Check if TidyPackRat.exe exists
if not exist "%~dp0GUI\TidyPackRat.exe" (
    echo ERROR: TidyPackRat.exe not found!
    echo.
    echo Please ensure the folder structure is intact:
    echo   GUI\TidyPackRat.exe
    echo   GUI\Newtonsoft.Json.dll
    echo   Worker\TidyPackRat-Worker.ps1
    echo.
    pause
    exit /b 1
)

echo Launching TidyPackRat Configuration Tool...
echo.

REM Launch the GUI application
start "" "%~dp0GUI\TidyPackRat.exe"

REM Optional: Uncomment the line below to keep this window open
REM pause

exit /b 0
