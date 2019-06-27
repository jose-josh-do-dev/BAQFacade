# BAQFacade
A Web API Facade to allow External Entities access to specific BAQ's based on Security

# Things To Note
appsettings.json file contains all the settings to connect to your Epicor REST API Instance. Update these to match yours before using/testing.
* Host: your.tld.url where your Epicor REST API Resides
* Company: Epicor company ID
* Instance: Pilot, Test etc
* ApiVersion: 1
* User: Epicor user ID that you'd like to run the BAQs as
* Password: password for above user
* AuthBAQ: BAQ which checks that the current request is authorized to execute a BAQ. This BAQ MUST accept 2 parameters 
	* TokenID passed in the request as a bearer token
	* BAQID: Passed in in the URL
# How To Run
Change the above settings and then execute the project.
Project should launch at 
[https://URL:PORT/api/Epicor](https://URL>:<PORT>/api/Epicor)
To run a BAQ [GET] simply append it to the URL
[https://URL:PORT/api/Epicor/MyBAQID](https://URL>:<PORT>/api/Epicor/MyBAQID)
Ensure you are passing in an Authorization Header with your Bearer Token
If all is well the application will check your AuthBAQ which should return (1 or more) records if the Token and the BAQ are found to be "WhiteListed" in your system.
If the records are returned the request is granted and passed onto the Epicor REST API.

For Patch the same as above applies

This supports all the standard oData filters, triggers and parameters as you would use with regular Epicor REST
![Valid Demo](https://i.imgur.com/g76dmlW.png)

![Bad Token](https://i.imgur.com/VD2HcMA.png)
