@echo off
cls
set log=Events.log
set npm=C:\Program Files\nodejs\npm.cmd
set WAIT_TIME=2000
set BROWSER=iexplore.exe
echo ===============================================================================
echo  Setup and Deployment 
echo ===============================================================================
:init
echo /* -- [ Event log created ] --------------------------------------------------- */ > "%log%"
echo /* -- Source: %~dpnx0  			 */ >> "%log%"
echo /* -- Caller: %0 					 */ >> "%log%"
echo /* -- + Args: %* 					 */ >> "%log%"
echo /* -- Status: [ %errorlevel% /  ] ---------------------------------------- */ >> "%log%"
echo  - Running pre-checks...
goto check_npm


:check_npm
echo /* -- [ NPM Check ] ------------------------------ */ >> "%log%"
if exist "%npm%" goto check_npm_version
:check_npm_download
echo /* -- [ NPM Download ] ------------------------------ */ >> "%log%"
echo  - Node Package Manager Required! Please download and install... 
start %BROWSER% "http://nodejs.org/" >> nul
rem if %errorlevel% goto error
:check_npm_wait
if exist "%npm%" goto check_npm_version
timeout 1 > nul
goto check_npm_wait
:end_check_npm_wait
:check_npm_version
echo  - NPM Version: >> "%log%"
call "%npm%" -v >> "%log%"
echo /* -- Returns [ %errorlevel% ] ----------------------------------------------- */ >> "%log%"
:check_npm_done


:npm_install
echo /* -- [ NPM Install and Update ] ---------------------------------------------- */ >> "%log%"
call npm run add:globals >> "%log%"
set npm_args= -express --loglevel error
if not exist "node_modules" (
	echo  - Installing the node modules...
	echo -------------------------------------------------------------------------------
	call "%npm%" install %npm_args%  >> "%log%"
	echo  - Installed.
) else (
	echo  - Updating the node modules...
	echo -------------------------------------------------------------------------------
	call "%npm%" update %npm_args% >> "%log%"
	echo  - Updated.
)
echo -------------------------------------------------------------------------------
call "%npm%" list --depth=0
echo -------------------------------------------------------------------------------
echo /* -- Returns [ %errorlevel% ] ----------------------------------------------- */ >> "%log%"
:npm_install_done

set msbuild_src=C:\Program Files (x86)\MSBuild\Microsoft\VisualStudio\v12.0\WebApplications
set msbuild_dst=C:\Program Files (x86)\MSBuild\Microsoft\VisualStudio\v11.0\WebApplications
if not exist "%msbuild_dst%" (
	md "%msbuild_dst%"
	copy /Y "%msbuild_src%\*.*" "%msbuild_dst%\" > nul || goto error
)
:npm_grunt
echo /* -- [ Grunt Tasks ] ------------------------------ */ >> "%log%"
echo  - Running Tasks...
echo -------------------------------------------------------------------------------
call grunt || goto error
echo /* -- Returns [ %errorlevel% ] ----------------------------------------------- */ >> "%log%"
:npm_grunt_done


:done
rem if %errorlevel% goto error
echo -------------------------------------------------------------------------------
echo  - The script has been run successfully
echo -------------------------------------------------------------------------------
goto wait
:error
echo -------------------------------------------------------------------------------
echo  - Error : The script encountered one or more errors (level %errorlevel%)...
echo -------------------------------------------------------------------------------
:wait
pause
:close