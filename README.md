# MessagingApp
A C# project that contains a server, as well as a client application for two clients to chat via tcp

The intent was to learn and apply low-level concepts such as byte arrays, System.Threading, writing data to a Network Stream, and handling two simultaneous connections, all while making an interactive command line chat application
## How to use

(If both clients and the server are on the same network as the server, skip step 2)

1. Change the IP Address of the Messaging App to whatever the IP Address is of the Server device
2. Port forward port 50002 via your routers settings to the device that will act as the server
3. Run the server application on the device you want to act as the server
4. Run the two client applications and they should automatically connect to the server
5. Chat away!
