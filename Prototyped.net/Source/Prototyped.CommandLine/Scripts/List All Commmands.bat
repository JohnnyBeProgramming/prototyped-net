@echo off
echo ------------------------------------------------------------------------------
echo  List Command Line Options
echo ------------------------------------------------------------------------------

:setup
set db_exe=proto.exe

:detect
echo  - Locating: %db_exe%
if exist "%db_exe%" goto init
if exist "../%db_exe%" cd ..
if exist "../../%db_exe%" cd .. & cd ..
if exist "../../../%db_exe%" cd .. & cd .. & cd ..
if not exist "%db_exe%" goto warning

proto /? || error

echo ------------------------------------------------------------------------------

proto sql /? || error

echo -------------------------------------------------------------------------------

proto shell /? || error

echo -------------------------------------------------------------------------------

proto winproc /? || error


:warning
echo -------------------------------------------------------------------------------
echo  - Warning: Could not find the required resources.
echo -------------------------------------------------------------------------------
pause 
goto end

:error
echo -------------------------------------------------------------------------------
echo  - Error: The batch operation did not complete successfully
echo --------------------------------------------------------------------------------
pause 
goto end

:done
echo  - Done
echo -------------------------------------------------------------------------------

:end