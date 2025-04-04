# fiks-io-send-client-dotnet

[![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ks-no/fiks-io-send-client-dotnet/blob/master/LICENSE)
[![Nuget](https://img.shields.io/nuget/vpre/KS.fiks.io.send.client.svg)](https://www.nuget.org/packages/KS.Fiks.IO.Send.Client)
[![GitHub issues](https://img.shields.io/github/issues-raw/ks-no/fiks-io-send-client-dotnet.svg)](//github.com/ks-no/fiks-io-send-client-dotnet/issues)

## About this library
This is a .NET library compatible with _[.NET Standard 2.1](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)_  and _[.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0)_ for sending messages to the Fiks platform. 
The library provides functionality to send messages to the Fiks platform using the Fiks IO Send API. The library supports sending messages with and without ASiC-E encryption.

## Getting Started

To get started with KS.Fiks.IO.Send.Client, add the NuGet package to your .NET project:

```sh
dotnet add package KS.Fiks.IO.Send.Client --version <version_number>
```

## Usage Example

Sending a Message
To send a message, you need to create an instance of MeldingSpesifikasjonApiModel and provide the necessary stream or payload. Here's an example:

```csharp
using KS.Fiks.IO.Send.Client;
using KS.Fiks.IO.Send.Client.Models;
using System;
using System.IO;
using System.Threading.Tasks;

public class SendMessageExample
{
    public async Task Send()
    {
        var sender = new FiksIOSender(/* configuration and maskinportenClient */);
        var metaData = new MeldingSpesifikasjonApiModel(
            _fiksIoAccountId, // Guid
            _toAccountId, // Guid
            _messageType, // string
            ttl: (long)TimeSpan.FromDays(2).TotalMilliseconds, // long
            headere: new()); // Dictionary<string, string>

        // Without ASiC-E
        using var dataStream = new MemoryStream(/* your data */);
        var sentMelding = await sender.Send(metaData, dataStream);
        
        // With ASiC-E
        var payload = new Payload("testfile.txt")
        var sentMelding = await sender.SendWithEncryptedData(metaData, payload);
        
        Console.WriteLine($"Message sent with ID: {sendtMelding.MeldingId}");
    }
}
```

## Configuration of FiksIOSender
The FiksIOSender class requires a configuration object to be passed in the constructor. The configuration object must implement the IFiksIOConfiguration interface. Here's an example of how to create a configuration object:

### Step 1: Create an instance of FiksIOSenderConfigurationBuilder
```csharp
var configurationBuilder = new FiksIOSenderConfigurationBuilder();
```

### Step 2: Configure Asice Signing
You can configure Asice Signing using either a public and private key pair or an X509Certificate2 instance.

#### Using Public and Private Key Pair
```csharp
configurationBuilder.WithAsiceSigningConfiguration(publicKeyPath, privateKeyPath);
```
Replace publicKeyPath and privateKeyPath with the paths to your public and private keys respectively.

#### Using X509Certificate2
```csharp
configurationBuilder.WithAsiceSigningConfiguration(certificate);
```
Replace certificate with an instance of X509Certificate2.

### Step 3: Configure Fiks Integrasjon
```csharp
configurationBuilder.WithFiksIntegrasjonConfiguration(fiksIntegrasjonId, fiksIntegrasjonPassword);
```
### Step 4: Configure API
```csharp
configurationBuilder.WithApiConfiguration(path, scheme, host, hostPort);
```
Step 5: Build the Configuration
```csharp
var configuration = configurationBuilder.Build();
```

### Example
```csharp
var configuration = new FiksIOSenderConfigurationBuilder()
            .WithAsiceSigningConfiguration(appSettings.AsiceSigningPublicKey, appSettings.AsiceSigningPrivateKey)
            .WithFiksIntegrasjonConfiguration(appSettings.FiksIoIntegrationId, appSettings.FiksIoIntegrationPassword)
            .WithApiConfiguration(null, appSettings.ApiScheme, appSettings.ApiHost, appSettings.ApiPort)
            .Build();

var sender = new FiksIOSender(configuration, _maskinportenClient);
```