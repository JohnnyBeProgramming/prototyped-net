@echo off
echo ------------------------------------------------------------------------------
echo  Create Database
echo ------------------------------------------------------------------------------

:setup
set db_exe=proto.exe
set db_cmd=init
set db_name=ProtoDB
set db_args=sql %db_cmd% -db %db_name% -wa

:detect
echo  - Locating: %db_exe%
if exist "%db_exe%" goto init
if exist "../%db_exe%" cd ..
if exist "../../%db_exe%" cd .. & cd ..
if exist "../../../%db_exe%" cd .. & cd .. & cd ..
if not exist "%db_exe%" goto warning

:init
echo  - Database: %db_name%
echo  - CMD Name: %db_cmd%
echo  - CMD Args: %db_args%

echo ------------------------------------------------------------------------------

:start
echo  - Runnning Database Utility...
%db_exe% %db_args% || goto error

:warning
echo -------------------------------------------------------------------------------
echo  - Warning: Could not find the required resources.
echo -------------------------------------------------------------------------------
pause 
goto end

:error
echo -------------------------------------------------------------------------------
echo  - Error: The batch operation did not complete successfully
echo -------------------------------------------------------------------------------
pause 
goto end

:done
echo  - Done
echo ------------------------------------------------------------------------------
:end