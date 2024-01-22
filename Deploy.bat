if not exist "Deploy\" mkdir Deploy

xcopy Libra.Web\favicon.ico Deploy\Libra.Web\ /Y
xcopy Libra.Web\web.config Deploy\Libra.Web\ /Y
xcopy Libra.Web\Global.asax Deploy\Libra.Web\ /Y
xcopy .vs\config\applicationhost.config Deploy\Libra.Web\applicationhost.config.orig /Y /Q

xcopy Libra.Web\bin\* Deploy\Libra.Web\bin\ /S /Y
xcopy Libra.Web\Content\* Deploy\Libra.Web\Content\ /S /Y
xcopy Libra.Web\Resources\* Deploy\Libra.Web\Resources\ /S /Y
xcopy Libra.Web\Views\* Deploy\Libra.Web\Views\ /S /Y

rem xcopy Libra.Frontend\Images\* Deploy\Libra.Frontend\Images\ /S
rem xcopy Libra.Frontend\Scripts\* Deploy\Libra.Frontend\Scripts\ /S

del Libra.Setup.zip
powershell.exe -nologo -noprofile -command "&{ Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('Deploy\','Libra.setup.zip',[IO.Compression.CompressionLevel]::Optimal, 0); }"