SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe
if %PROCESSOR_ARCHITECTURE% == x86 (
	SET MSBUILD=C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
)

%MSBUILD% ..\WorkQueue.sln /p:Configuration=Release /verbosity:m > build.log

xcopy ..\AGO.WorkQueue\bin\Release\AGO.WorkQueue.dll nuget\lib\ /Y >> build.log

cd nuget

..\..\.nuget\nuget pack workqueue.nuspec -OutputDirectory ..\ >> ..\build.log
cd ..

pause