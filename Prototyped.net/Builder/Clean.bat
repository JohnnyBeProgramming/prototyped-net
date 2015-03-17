@echo off
cls
set log=Events.log
set npm=C:\Program Files\nodejs\npm.cmd
set WAIT_TIME=2000
set BROWSER=iexplore.exe
echo ===============================================================================
echo  Clean Development Environment 
echo ===============================================================================
:init
echo /* -- [ Event log created ] --------------------------------------------------- */ > "%log%"
echo /* -- Source: %~dpnx0  			 */ >> "%log%"
echo /* -- Caller: %0 					 */ >> "%log%"
echo /* -- + Args: %* 					 */ >> "%log%"
echo /* -- Status: [ %errorlevel% /  ] ---------------------------------------- */ >> "%log%"


:cmd_delete
echo /* -- [ Clean Files ] ------------------------------ */ >> "%log%"
echo  - Warning: Cleaninig your current workspace...
echo -------------------------------------------------------------------------------
echo  1 Express clean-up.   ( Just delete the Build folder only )
echo  2 Clean dist folder.  ( Default - Include Destination folders )
echo  3 Full clean-up.      ( Including node modules )
echo  4 Quit.               ( Exit to command line )
echo -------------------------------------------------------------------------------
choice /C:1234 /N /T:5 /D 2 /M "-- Select option (auto starts in 5s):"
if ERRORLEVEL 4 goto cmd_delete_skip
if ERRORLEVEL 3 goto cmd_delete_modules
if ERRORLEVEL 2 goto cmd_delete_app
if ERRORLEVEL 1 goto cmd_delete_bin
echo  - Option not found: %ERRORLEVEL%
goto cmd_delete_finish

rem ++++++++ call grunt clean || goto error

:cmd_delete_modules
echo  - Deleting: Module Dependencies...
if exist "node_modules\" rmdir "node_modules" /S /Q > nul
:cmd_delete_app
echo  - Deleting: Application Folders... 
if exist "Published\" rmdir "Published" /S /Q > nul
:cmd_delete_bin
echo  - Deleting: Build Folder...
if exist "Build\" rmdir "Build" /S /Q > nul
:cmd_delete_logs
del *.log /Q
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S bin') DO RMDIR /S /Q "%%G"
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"
:cmd_delete_finish
goto cmd_delete_done 
:cmd_delete_skip
echo  - Skipped.
echo /* -- Returns [ %errorlevel% ] ----------------------------------------------- */ >> "%log%"
:cmd_delete_done


:done
del *.log /Q /F > nul
rem if %errorlevel% goto error
echo -------------------------------------------------------------------------------
echo  - The script has been run successfully
echo -------------------------------------------------------------------------------
goto close
:error
echo -------------------------------------------------------------------------------
echo  - Error : The script encountered one or more errors (level %errorlevel%)...
echo -------------------------------------------------------------------------------
:wait
pause
:close