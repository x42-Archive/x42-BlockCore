## Stratis DNS Crawler 
The x42 DNS Crawler provides a list of x42 full nodes that have recently been active via a custom DNS server.

### Prerequisites

To install and run the DNS Server, you need
* [.NET Core 2.2](https://www.microsoft.com/net/download/core)
* [Git](https://git-scm.com/)

## Build instructions

### Get the repository and its dependencies

```
git clone https://github.com/x42protocol/X42-BlockCore.git
cd X42-BlockCore
git submodule update --init --recursive
```

### Build and run the code
With this node, you can run the DNS Server in isolation or as a x42 node with DNS functionality:

1. To run a <b>x42</b> node <b>only</b> on <b>MainNet</b>, do
```
cd x42.x42DnsD
dotnet run -dnslistenport=5399 -dnshostname=x42seed.host -dnsnameserver=node01.x42seed.host -dnsmailbox=admin@x42.tech
```  

2. To run a <b>x42</b> node and <b>full node</b> on <b>MainNet</b>, do
```
cd x42.x42DnsD
dotnet run -dnsfullnode -dnslistenport=5399 -dnshostname=x42seed.host -dnsnameserver=node01.x42seed.host -dnsmailbox=admin@x42.tech
```  

3. To run a <b>x42</b> node <b>only</b> on <b>TestNet</b>, do
```
cd x42.x42DnsD
dotnet run -testnet -dnslistenport=5399 -dnshostname=x42seed.host -dnsnameserver=node01.x42seed.host -dnsmailbox=admin@x42.tech
```  

4. To run a <b>x42</b> node and <b>full node</b> on <b>TestNet</b>, do
```
cd x42.x42DnsD
dotnet run -testnet -dnsfullnode -dnslistenport=5399 -dnshostname=x42seed.host -dnsnameserver=node01.x42seed.host -dnsmailbox=admin@x42.tech
```  

### Command-line arguments

| Argument      | Description                                                                          |
| ------------- | ------------------------------------------------------------------------------------ |
| dnslistenport | The port the Stratis DNS Server will listen on                                       |
| dnshostname   | The host name for Stratis DNS Server                                                 |
| dnsnameserver | The nameserver host name used as the authoritative domain for the Stratis DNS Server |
| dnsmailbox    | The e-mail address used as the administrative point of contact for the domain        |

### NS Record

Given the following settings for the Stratis DNS Server:

| Argument      | Value                             |
| ------------- | --------------------------------- |
| dnslistenport | 53                                |
| dnshostname   | x42seed.host					    |
| dnsnameserver | node01.x42seed.host				|

You should have NS and A record in your ISP DNS records for your DNS host domain:

| Type     | Hostname                          | Data                              |
| -------- | --------------------------------- | --------------------------------- |
| NS       | x42seed.host					   | x42seed.host					   |
| A        | node01.x42seed.host			   | 192.168.1.2                       |

To verify the Stratis DNS Server is running with these settings run:

```
dig +qr -p 53 node01.x42seed.host
```  
or
```
nslookup node01.x42seed.host
```
