rem Prepare directory structure
del /Q apinet.zip
rd distr /S /Q > build.log
mkdir distr >> build.log
mkdir distr\api >> build.log
mkdir distr\reporting >> build.log
mkdir distr\watcher >> build.log
mkdir distr\client >> build.log
mkdir distr\notification >> build.log

rem Make build and publish
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe
if %PROCESSOR_ARCHITECTURE% == x86 (
	SET MSBUILD=C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
)

rem build all and publish api
rd /S /Q tmp >> build.log
mkdir tmp >> build.log
SET CURDIR=%CD%
pushd ..
call "C:\Program Files (x86)\VC\vcvarsall.bat" x86 >> build.log
%MSBUILD% AGO.sln /p:Configuration=Debug /verbosity:m >> %CURDIR%\build.log
%MSBUILD% AGO.WebApiApp\AGO.WebApiApp.csproj /t:_WPPCopyWebApplication /p:BuildingProject=false;WebProjectOutputDir=%CURDIR%\distr\api /verbosity:m >> %CURDIR%\build.log
popd
rd /S /Q tmp  >> build.log
rem copy module bins to api
robocopy ..\AGO.Tasks\bin\Debug\ distr\api\bin\ AGO.Tasks.* >> build.log >> build.log
robocopy ..\AGO.Tasks\bin\Debug\ru\ distr\api\bin\ru\ AGO.Tasks.resources.* >> build.log

rem copy reporting service
robocopy ..\AGO.Reporting.Service\bin\Debug\ distr\reporting\ /MIR /XD logs Mappings >> build.log
rem copy module bins to reporting
robocopy ..\AGO.Tasks\bin\Debug\ distr\reporting\ AGO.Tasks.* >> build.log >> build.log
robocopy ..\AGO.Tasks\bin\Debug\ru\ distr\reporting\ru\ AGO.Tasks.resources.* >> build.log

rem copy watcher host
robocopy ..\AGO.WatchersHost\bin\Debug\ distr\watcher\ /MIR /XD logs Mappings >> build.log

rem copy notification host
robocopy ..\AGO.NotificationHost\ distr\notification\ /MIR /NFL /NDL /XD node_modules >> build.log


rem Build client
SET APINET_CLIENT=..\..\apinet-client

pushd %APINET_CLIENT%\src\core
call npm install >> %CURDIR%\build.log
call bower install >> %CURDIR%\build.log
call grunt build >> %CURDIR%\build.log
popd

pushd %APINET_CLIENT%\src\tasks
call npm install >> %CURDIR%\build.log
call bower install >> %CURDIR%\build.log
call grunt build >> %CURDIR%\build.log
popd

rem copy client
robocopy %APINET_CLIENT%\release\ distr\client /MIR >> build.log

rem Zip all
SET ZIPPER="c:\Program Files\7-Zip\7z.exe"
%ZIPPER% a apinet.zip distr\ >> build.log

pause