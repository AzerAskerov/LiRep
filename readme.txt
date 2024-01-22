Instructions how to prepare Libra DEV environment

****************************************************************************************

    1. Prepare binaries.
		- Open `.\src\Libra.sln` (use VS2015 or VS2017). 
		- Build it in DEBUG mode (later for production use RELEASE mode). 
	2. Prepare client side resources. 
		- Install npm (detailed instructions can be foud here: https://docs.npmjs.com/getting-started/installing-node).
		- Open cmd in admin mode and go to `.\src\Libra.Frontend`.
		- Run `npm install` for retrieving packages from public feed.
		- Run `npm run build:dev` to build it (for production use `npm run build:prod`). Compiled resources will be automatically dropped to `.\src\Libra.web\Content`.
		- NOTE: for developing client side I recommend to use Visual Studio Code (https://code.visualstudio.com/). Just install & run the application, choose open folder `.\src\Libra.Frontend` and start coding.
	3. Prepare database.
		- Create an empty database.
		- Run `.\src\Libra.Services\Database\Scripts\LibraDb_1.0.sql` on the database.
		- Script creates tables&views and adds test users with different roles.
	4. Set up environment. 
		- Create a web site in IIS and map it to `.\src\Libra.Web` path. Use integrated pipeline mode.
		- Check app settings and connection strings in `web.config`. Change connection string to the database you have created earlier.

****************************************************************************************

After above do the following:

1.On webserver open CMD with admin and run :

C:\Program Files (x86)\IIS Express>iisexpress /config:"C:\inetpub\wwwroot\Libra\Libra.Web\applicationhost.config" /site:"Libra.Web"
This will launch IIS Express 

2. You may now run the web app by opening iis express from the tray and running the libra web app

