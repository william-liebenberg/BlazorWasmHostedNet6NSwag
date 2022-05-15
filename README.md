# Blazor Wasm Hosted + .NET 6 API + NSwag
Blazor WebAssembly Hosted App with .NET 6 API and NSwag for OpenAPI Spec and C# Client Generation.

NOTE: Due to a current bug/issue with NSwag, we have to use the older style `Program.cs` and `Startup.cs`. For some reason the Minimal API style is generating an empty OpenAPI specification. Here is the GitHub Issue: https://github.com/RicoSuter/NSwag/issues/3727

To create our app, run the following `dotnet new` command:

```ps1
dotnet new blazorwasm -n MyApp -ho -f net6.0
```

## Add NSwag

In order for NSwag to generate an Open API specification for our application and also a strongly typed C# HTTP Client, we need to add a couple of important packages.

In the Server project:

```ps1
cd ./src/Server
dotnet add package NSwag.AspNetCore
dotnet add package NSwag.MSBuild
```

## Configuring NSwag

Add your `nswag.json` configuration

You can install the NSwag CLI via NPM:

```ps1
npm i nswag -g
```

Then to generate a new NSwag config simply run:

```ps1
nswag new /runtime:Net60
```

This will produce a `nswag.json` file that you can customise for how the OpenAPI Spec and Clients are generated.

Alternatively you can use [NSwagStudio](https://github.com/RicoSuter/NSwag/wiki/NSwagStudio) to customise your configuration (Windows Only).

Here are some noteworthy configuration options I use for NSwag:

```json
{
	"documentGenerator": {
		"aspNetCoreToOpenApi": {
			"project": "MyApp.Server.csproj",
			"output": "wwwroot/swagger/v1/swagger.json",
			"outputType": "OpenApi3"
			...
		}
	},
	"codeGenerators": {
		"openApiToCSharpClient": {
			"generateClientClasses": true,
			"generateClientInterfaces": true,
			"clientBaseInterface": null,
			"injectHttpClient": true,
			"disposeHttpClient": true,
			"useHttpClientCreationMethod": false,
			"httpClientType": "System.Net.Http.HttpClient",
			"useBaseUrl": false,
			"generateBaseUrlProperty": true,
			"generateSyncMethods": false,
			"additionalNamespaceUsages": [
				"MyApp.Shared"
			],
			"namespace": "MyApp.Client",
			"output": "../Client/Services/MyAppApiClients.g.cs"
			...
		}
	}
}
```

## Generating our OpenAPI specification and C# Client

We can run NSwag after a successful build to generate our OpenAPI `specification.json` file, and to generate a strongly typed C# client.

Add the following to `Server.csproj`
 
```xml
<PropertyGroup>
	<RunPostBuildEvent>OnBuildSuccess</RunPostBuildEvent>
</PropertyGroup>

<Target Name="NSwag" AfterTargets="PostBuildEvent" Condition="$(Configuration)=='Debug'">
	<Message Importance="High" Text="$(NSwagExe_Net60) run nswag.json /variables:Configuration=$(Configuration)" />

	<Exec WorkingDirectory="$(ProjectDir)" EnvironmentVariables="ASPNETCORE_ENVIRONMENT=Development" Command="$(NSwagExe_Net60) run nswag.json /variables:Configuration=$(Configuration)" />

	<Delete Files="$(ProjectDir)\obj\$(MSBuildProjectFile).NSwag.targets" /> <!-- This thingy trigger project rebuild -->
</Target>
```

> Note that we are using `NSwagExe_Net60` for .NET 6

Now after each build, we will have:

1. `specification.json` file in `wwwroot/api/v1` 
2. `MyAppApiClient.g.cs` in `src/Client/Services`


## Using our Generated C# Client

In our Blazor Wasm app, we need to wire in our generated client.

In `Program.cs`:

```cs
builder.Services.AddScoped<IWeatherForecastClient, WeatherForecastClient>();
```

Then we can inject our client to a page or component by using the `@inject` directive:

```cs
// Inject our client interface
@inject IWeatherForecastClient Client

@code {
	public async Task Refresh()
	{
		// now we can call any of our client methods - without having to concern ourselves with HTTP methods, errors, etc.
		forecasts = await Client.GetAsync();
	}
}
```

Done!

Happy coding :)