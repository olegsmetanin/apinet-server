rem Prepare directory structure
del /Q apinet.zip
rd distr /S /Q > build.log
mkdir distr >> build.log
mkdir distr\api >> build.log
mkdir distr\reporting >> build.log
mkdir distr\client >> build.log
mkdir distr\notification >> build.log

rem Make build and publish
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe
if %PROCESSOR_ARCHITECTURE% == x86 (
	SET MSBUILD=C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
)
rd /S /Q tmp >> build.log
mkdir tmp >> build.log
SET CURDIR=%CD%
pushd ..

call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\vcvarsall.bat" x86

%MSBUILD% AGO.sln /p:Configuration=Debug /verbosity:m >> %CURDIR%\build.log

%MSBUILD% AGO.WebApiApp\AGO.WebApiApp.csproj /t:_WPPCopyWebApplication /p:BuildingProject=false;WebProjectOutputDir=%CURDIR%\distr\api /verbosity:m >> %CURDIR%\build.log

%MSBUILD% AGO.Reporting.Service\AGO.Reporting.Service.csproj /t:_WPPCopyWebApplication /p:BuildingProject=false;WebProjectOutputDir=%CURDIR%\distr\reporting /verbosity:m >> %CURDIR%\build.log

popd
rd /S /Q tmp  >> build.log

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

robocopy %APINET_CLIENT%\release\ distr\client /MIR >> build.log
robocopy %APINET_CLIENT%\ distr\client index.html >> build.log

rem Copy unbuildable stuff
robocopy distr\api\bin\ distr\reporting\bin\ AGO.Tasks.* >> build.log
robocopy distr\api\bin\ru\ distr\reporting\bin\ru\ AGO.Tasks.resources.* >> build.log
robocopy ..\AGO.NotificationHost\ distr\notification\ /MIR /NFL /NDL /XD node_modules >> build.log

rem Zip all
SET ZIPPER="c:\Program Files (x86)\7-Zip\7z.exe"
%ZIPPER% a apinet.zip distr\ >> build.log

pause