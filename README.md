# StickyNet

This program helps to build up a database full of IPs that don't use the internet in ways that we should accept.
  - Scanning the entire internet
  - Spamming Requests
  - Bruteforcing
  - ...
  
What this program will do is opening Tcp / Udp Servers on ports you specified, imitating a set protocol.
Everyone who connects to it will be logged to a file, a [ScanBanServer](https://github.com/JojiiOfficial/ScanBanServer), or both.

## Benefits
### Security
If you use StickyNet to open listeners that imitate protocols like SSH & FTP (You can have loads of them) 
potential attackers are not able to distinguish from the real and the imitated one.
There is no way telling which one is actually used to control your server.
Moreover you can install this with the [TripWireReporter](https://github.com/JojiiOfficial/Tripwire-reporter) 
that automatically pulls IPs that have been reported to a [ScanBanServer](https://github.com/JojiiOfficial/ScanBanServer) 
and blocks them from interacting with your server using iptables.
You can specify which criteria the IPs have to fulfill to go into your banlist (Confirmed Scanner, Tor Exit Node, Command Spammer...)

### Help Others
When you install this application on your server and send the collected data to a [ScanBanServer](https://github.com/JojiiOfficial/ScanBanServer) 
you will help others to protect themselves. Your data will help significantly in identifying potential malicious IP adresses 
which are automatically blocked on any server that uses the [TripWireReporter](https://github.com/JojiiOfficial/Tripwire-reporter).

## Installation
### Linux
#### Method 1: Auto installation script: 
Note: This will automatically install a daemon so it will autorun on startup
```
This method is not yet supported! I'm working on it.
```

#### Method 2: Clone and Build
Note: You will need to setup a daemon yourself if you want to have it in autorun

You need to [setup dotnet](https://dotnet.microsoft.com/download/linux-package-manager/debian9/sdk-current) before proceeding! 
```
git clone https://github.com/Playwo/StickyNet
dotnet publish -c Release -o Binaries --self-contained --runtime linux-x64 StickyNet/
chmod +x Binaries/StickyNet
```

### Windows
#### Method 1: Clone and Build
Guide will be added at a later point in time...
