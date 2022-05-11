# Lomdom_server
 
## How to run
To run the server you need to make a `config.json` file in the projects `Assets` directory  or if its built `BUILD_NAME_Data` directory then put the following in the `config.json` file 
```
{
	"Port": 7777,
	"MaxClientCount": 30,
	"Api_url": "http://127.0.0.1:8000/api/server",
	"ServerID": The servers id in the website's db,
	"ServerToken": The servers token in the website db
}
```

To get server token on local website go [here](http://127.0.0.1:8000/admin/server/server/) And click the 'ADD SERVER' button copy the server token and paste it into the `config.json` file (it will be hashed upon save so you will not be able to find it after saving) then fill in the `Ip` `Port` and `Max play count` fields and click save. After you save click the item with the ip and port in the format `IP:PORT` and look in the url it should look like `http://127.0.0.1:8000/admin/server/server/ServerID/change/` put the `ServerID` into the `config.json` file then run the server.

If you lose the server token just delete the server on the webiste and make a new one as the server token is hashed.