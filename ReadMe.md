## DD-WRT API 

[![Build Status](https://dev.azure.com/bradut/DD-WRT_API/_apis/build/status/DD-WRT_API-.NET%20Desktop-CI?branchName=master)](https://dev.azure.com/bradut/DD-WRT_API/_build/latest?definitionId=10&branchName=master)

**About:**  
 - Minimalistic API to communicate with a router powered by [DD-WRT](https://dd-wrt.com/) without using its UI control pannel.
 - I'm using it for my home automation and intentionally made it compatible with Windows XP (.NET 4.0).    
 - Repo contains: 
      - the API 
      - a console app that uses the API.   


Here is the console app in action:

![DD-WRT-API Demo](DD-WRT_API/Docs/DD-WRT-Demo.jpg)


**API's Features:**
 - Get the list of WiFi devices
 - Get the list of Active devices
 - Get the list of All devices
 - Reboot the router

**Setup:**  
Before using please modify the *app.config* file:
 - *Username* and *Password* 
 - *RouterUrl*  (router's IP needs to be prefixed with http:// such as http://192.168.0.1)
 - *DisplayDuration* in format Days.HH:mm:ss = the duration the app will run
 - *WaitTimeSeconds* = [when reading data] the time between two subsequent data readings from router.

**Usage**  
The command line app accepts *zero*, *one* or *four* parameters:
  -  **0 (zero) parameters**:   
     If configured correctly, the app will display all users in a useful format (see below *commandName* **all**) and will loop
 until the end of *DisplayDuration* with pauses of *WaitTimeSeconds* after each loop.

-  **1 parameter**: *commandName*  
This is a convenient way to run the app by just passing the command, assuming that the rest of parameters in config file are correct
  
-  **4 parameters**: *commandName,  userName, password, routerUrl*  
   The app will execute the command and will use the *userName, password* and *routerUrl* passed as arguments, so you can avoid exposing them in the config file.

**List of command names:**        
   * **wifi**   - display WiFi users
   * **active** - display active users (including WiFi)
   * **lan**    - display all users
   * **all**    - combines the commands above to display all users and to indicate which ones are Active and/or WiFi
   * **reboot** - reboots the router.



### Implementation details [C#/ .NET 4.0]
 - Can run on older machines powered by Windows XP, so it targets on purpose .NET 4.0  
 - Works only with routers flashed with [DD-WRT Firmware](https://wiki.dd-wrt.com/wiki/index.php/Installation)  
 - Tested with DD-WRT version 3.0 but it may work with other versions.  
 - Communicates with the router via its HTML Control Interface   
   Note: DD-WRT does not offer a WebAPI REST service, but anyone may extend this API and create one.    
 
[I also wrote a reboot app that controls the router via Telnet and it seems more efficient]   



### History
This project is a result of my need to measure the efficiency of a High-Gain antenna I purchased to improve the WiFi signal of an outdoors IP WebCam.  
The new antenna didn't seem to make a difference, so I wanted to see some numbers to prove me right or wrong.

The DD-WRT Control Panel can display useful info about WiFi nodes, but it's difficult to figure-out the device names:   
![DD-WRT UI - Wireless Nodes ](DD-WRT_API/Docs/WiFi_Nodes.jpg)

This UI didn't fit my needs: I wanted, for each antenna I tested, 
to measure and *record* these values 
for longer periods of time (two hours) 
and with a certain frequency (every 20 seconds).

Since I couldn't find such a tool, I wrote this app. I hope it will be useful to others.

**Not directly related to this project:**
My measurements indicated that the <ins>11 dBi High-Gain Antenna</ins> I tested improved the WiFi signal with only 2 dBi.   
Unfortunately, switching antennas during measurements damaged the connector and rendered my camera useless :(

### What is a DD-WRT router anyways?
DD-WRT is Linux-based firmware for wireless routers and access points. Originally designed for the Linksys WRT54G series, it now runs on a wide variety of models. ... The remainder of the name was taken from the Linksys WRT54G model router, a home router popular in 2002–2004.   
Source: https://en.wikipedia.org/wiki/DD-WRT


### Links
 [Miles Burton - DD-WRT Web-services](https://www.milesburton.com/index.php?title=DD-WRT_Web-services&oldid=1500)   
 [My selection of DD-WRT commands](DD-WRT_API/Docs/DD-WRT_Commands.txt)   
 [ChangeLog](DD-WRT_API/Docs/ChangeLog.txt)



Similar projects:  
[DD-WRT calling web service from Arduino ESP8266 (18 Oct.2018)](https://superuser.com/questions/988465/dd-wrt-calling-web-service-from-arduino-esp8266)





   
