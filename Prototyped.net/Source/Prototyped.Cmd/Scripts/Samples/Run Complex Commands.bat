@echo off
echo ------------------------------------------------------------------------------
echo  Example: Complex Commands
echo ------------------------------------------------------------------------------

:setup
set proto_exe=proto.exe

:detect
echo  - Locating: %proto_exe%
set "depth=0"
set "depth_max=4"
if exist "%proto_exe%" goto init
:detect_parent
if "%depth%"=="%depth_max%" goto warning
set "proto_exe=../%proto_exe%"	
if not exist "%proto_exe%" ( 
	set /a depth+=1 & goto detect_parent
) else (
	goto init
)
if not exist "%proto_exe%" goto warning
goto init


:init
echo  - Invoking: %proto_exe%
call "%proto_exe%" shell tester info /Q -conn ProtoDB -context:ProtoContext || goto error
goto done


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