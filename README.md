# SignalRServer
## Overview

The Gaming Multiplayer Backend is an ASP.NET Core server built to handle real-time communication and room management for a multiplayer gaming application. The server leverages SignalR for real-time messaging and WebRTC signaling, enabling peer-to-peer (P2P) communication between clients.

## Features

-  **Real-Time Communication:** Uses SignalR to send notifications, chat messages, and WebRTC signaling (offers, answers, and ICE candidates) between clients.

-  **Room Management:** Supports creating private rooms and assigning clients to random non-private rooms with configurable room sizes.

-  **Client Management:** Automatically creates and updates client records upon connection.

-  **P2P Signaling:** Facilitates the WebRTC negotiation process by exchanging offers, answers, and ICE candidates.

-  **Logging:** Integrated with Serilog for both console and file logging.

-  **Database:** Uses Entity Framework Core with PostgreSQL.

## Requirements

-  .NET 8.0

-  PostgreSQL

-  (Optional) ngrok or LocalTunnel for external testing

## Getting Started

### 1\. Clone the Repository

```bash
git clone https://github.com/yourusername/Gaming-multiplayer-backend.git
cd Gaming-multiplayer-backend
```

### 2\. Configure the Database and STUN/TURN

Update the connection string in your `appsettings.json` file. For example:

```bash
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=GMServer;Username=postgres;Password=YourPassword"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "SignalR": {
    "StunServer": "stun.l.google.com:19302",
    "TurnServer": "turn:my-turn-server.com"
  }
}
```

### 3\. Run the Application

You can run the application using Visual Studio or the .NET CLI:

#### Installing .NET 8.0 1. 

-  **Download .NET 8.0 SDK:** 

   Visit the \[official .NET download page\] (<https://dotnet.microsoft.com/download/dotnet/8.0>) and download the appropriate .NET 8.0 SDK installer for your operating system. 

-  **Install the SDK:** 

   Run the downloaded installer and follow the on-screen instructions to install the .NET 8.0 SDK.

-  **Verify the Installation:** 

   Open a terminal or command prompt and run: 

   `dotnet --version`

### Running the Project

You can run the application using either Visual Studio or the .NET CLI.

#### Using Visual Studio

1. **Open the Solution:**\
   Open Visual Studio and load the solution file (`*.sln`) located in the project root.

2. **Build and Run:**\
   Press `F5` or click on the **Start Debugging** button to build and run the application.\
   The server will start and be available at:\
   [`https://localhost:7283/`](https://localhost:7283/)

#### Using the .NET CLI

1. **Open a Terminal:**\
   Navigate to the project directory (where the `.csproj` file is located).

2. **Run the Application:**\
   Execute the following command:

```bash
dotnet run
```

Once the command completes, the server will be running and accessible at:\
[`https://localhost:7283/`](https://localhost:7283/)

## SignalR Hub

The SignalR hub is hosted at the `/notifications` endpoint.

### Example Client Code

Below is an example of how a client can connect to the SignalR hub:

```bash
var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7283/notifications")
    .ConfigureLogging(logging => logging.AddConsole())
    .Build();

await connection.StartAsync();
```

## API Endpoints

### Join Room

-  **Method:** `JoinRoom(string roomCode, int maxRoomSize)`

-  **Description:** Allows a client to join a room using a specific room code. The client is added to the room group, and all room participants are notified.

### Join Random Room

-  **Method:** `JoinRandomRoom(int maxRoomSize)`

-  **Description:** Assigns a client to a random active non‑private room with available space.

### Create Private Room

-  **Method:** `CreatePrivateRoom(int maxRoomSize)`

-  **Description:** Creates a new private room for the client.

### Send Message to Room

-  **Method:** `SendMessageToRoom(string roomCode, string message)`

-  **Description:** Broadcasts a chat message to all clients in the specified room.

### Send NAT Traversal Info

-  **Method:** `SendNATTraversalInfo(string roomCode, string publicIp, int publicPort)`

-  **Description:** Updates the client's public IP and port for WebRTC traversal and notifies other clients in the room.

### Report P2P Failure

-  **Method:** `ReportP2PFailure(string roomCode)`

-  **Description:** Notifies the client that a P2P connection failed and returns TURN server information.

### WebRTC Signaling

-  **Methods:** `SendOffer`, `SendAnswer`, `SendIceCandidate`

-  **Description:** Facilitates the WebRTC negotiation process by exchanging offers, answers, and ICE candidates between clients.

## Connection Algorithm and Steps

1. **Initialize the SignalR Connection**

   -  The client creates a new `HubConnection` using the SignalR client library.

   -  Upon starting the connection, the server assigns a unique connection ID.

   -  **Example:**
```bash
var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7283/notifications")
    .ConfigureLogging(logging => logging.AddConsole())
    .Build();
await connection.StartAsync();
```



2. **Join a Room**

   -  **Join by Room Code:**

      -  The client calls the `JoinRoom` method with a specific room code.

      -  The server checks if the room exists and has available space, then adds the client to that room's SignalR group.

   -  **Join Random Room:**

      -  Alternatively, the client calls `JoinRandomRoom` to be assigned to an active non‑private room.

   -  **Example:**

```bash
// Join by room code
await connection.InvokeAsync("JoinRoom", "ROOM123", 10);

// Or join a random room
await connection.InvokeAsync("JoinRandomRoom", 10);
```


3. **Exchange NAT Traversal Information**

   -  After joining a room, the client determines its public IP and port (e.g., via a temporary RTCPeerConnection).

   -  The client then sends this information using `SendNATTraversalInfo`.

   -  The server forwards this NAT info to other clients in the room to facilitate P2P connections.

   -  **Example:**
```bash
await connection.InvokeAsync("SendNATTraversalInfo", "ROOM123", "203.0.113.1", 5000);
```

4. **Establish P2P Connection Using WebRTC Signaling**

   -  **Offer/Answer Negotiation:**

      -  The offerer creates an offer using `RTCPeerConnection.CreateOffer()`, sets it as its local description, and sends it via `SendOffer`.

      -  The answerer receives the offer, sets it as its remote description, creates an answer with `RTCPeerConnection.CreateAnswer()`, sets it as its local description, and sends it back via `SendAnswer`.

   -  **ICE Candidate Exchange:**

      -  As ICE candidates are generated, they are sent between clients using `SendIceCandidate`.

      -  Each client adds received ICE candidates to its `RTCPeerConnection` object.

   -  **Example (Offerer):**
```bash
// Create an offer, set as local description, and send it via SignalR
var offer = await peerConnection.CreateOffer();
await peerConnection.SetLocalDescription(offer);
await connection.InvokeAsync("SendOffer", "ROOM123", targetConnectionId, JsonConvert.SerializeObject(offer));
```
   -  **Example (Answerer):**

```bash
// Set received offer as remote description, create an answer, set as local description, and send it via SignalR
await peerConnection.SetRemoteDescription(receivedOffer);
var answer = await peerConnection.CreateAnswer();
await peerConnection.SetLocalDescription(answer);
await connection.InvokeAsync("SendAnswer", "ROOM123", targetConnectionId, JsonConvert.SerializeObject(answer));
```

5. **Establish Data Channel and Exchange Messages**

   -  Once the P2P connection is established, a data channel is created (by the offerer or received by the answerer).

   -  Clients use the data channel to exchange messages directly.

   -  **Example:**
```bash
dataChannel.Send("Hello, peer-to-peer world!");
```

6. **Client-Side Message Processing**

   -  The client must listen for incoming messages and events from the server. Typical events include:

      -  **ReceiveMessage:** For system and chat messages.

      -  **UpdateRoomParticipants:** For updated lists of room participants.

      -  **ReceiveOffer, ReceiveAnswer, ReceiveIceCandidate:** For WebRTC signaling.

   -  **Example (Listening for messages):**

```bash
connection.on("ReceiveMessage", (sender, message) => {
  console.log(`Message from ${sender}: ${message}`);
  // Process or display the message as needed.
});

connection.on("UpdateRoomParticipants", (participants) => {
  console.log("Updated participant list:", participants);
  // Update the UI with the new participant list.
});
```
## Additional Tools: 
### Test SignalR client 

by Angular
[SignalR Client](https://github.com/DenisBezdolny/SignalRClient.git)

### ngrok

ngrok is a tool that creates secure tunnels from the public internet to your local machine. It is very useful for exposing your local server to external testers, mobile devices, or webhook providers without deploying your application to a public server.

### Steps to Install and Use ngrok

1. **Download ngrok:**

   -  Visit the [ngrok download page](https://ngrok.com/download) and download the binary appropriate for your operating system (Windows, macOS, or Linux).

   -  Extract the downloaded archive to a directory of your choice.

2. **Add ngrok to Your PATH (Optional):**

   -  To run ngrok from any command prompt, add the directory containing the `ngrok` executable to your system’s PATH.

   -  On Windows, you can do this by going to *Control Panel* --> *System and Security* --> *System* --> *Advanced system settings* --> *Environment Variables* --> Edit the `Path` variable and add the folder path.

3. **Authenticate with ngrok (Recommended):**

   -  Sign up for a free account at [ngrok.com](http://ngrok.com) if you haven’t already.

   -  After logging in, go to your [dashboard](https://dashboard.ngrok.com/) to find your authtoken.

   -  Open a terminal/command prompt and run:

      ```
      ngrok config add-authtoken YOUR_AUTHTOKEN
      ```

      Replace `YOUR_AUTHTOKEN` with the token from your dashboard. This step enables additional features and custom subdomains (if needed).

4. **Start an ngrok Tunnel:**

   -  To expose your local server (e.g., running on port 7283) to the internet, run:

      ```
      ngrok http 7283
      ```

   -  The command will create an HTTP tunnel on port 7283. In the terminal, you will see output similar to:

      ```
      ngrok by @inconshreveable                                             (Ctrl+C to quit)
      
      Session Status                online
      Account                       Your Name (Plan: Free)
      Version                       3.x.x
      Region                        United States (us)
      Web Interface                 http://127.0.0.1:4040
      Forwarding                    http://<random_subdomain>.ngrok.io -> http://localhost:7283
      Forwarding                    https://<random_subdomain>.ngrok.io -> http://localhost:7283
      
      ```

   -  Use the HTTPS URL (e.g., `https://<random_subdomain>.`[`ngrok.io`](http://ngrok.io)) provided by ngrok to access your local server from external devices.

5. **Test the Tunnel:**

   -  Open a web browser on a different device (or ask someone else) and navigate to the HTTPS URL from ngrok.

   -  You should be able to access your local application through this public URL.

### Troubleshooting

-  **Firewall/Antivirus Settings:**\
   Make sure your firewall or antivirus software isn’t blocking ngrok or the specific port (7283) that your server is using.

-  **VPN Issues:**\
   If you're using a VPN, try disconnecting it or check its settings, as it might interfere with ngrok connections.

-  **Custom Subdomains (Paid Feature):**\
   If you require a custom subdomain, this feature is available on paid ngrok plans.

With these steps, you can successfully expose your local server to the internet using ngrok for testing and development purposes.

## Logging

The server uses Serilog for logging. Logs are written to both the console and a rolling file (`Logs/log.txt`). Logging is configured via the `SerilogExtensions` class.
