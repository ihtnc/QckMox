# QckMox Demo (Passthrough)

This is a configuration setup demo that allows the request to passthrough if there are no mocks found.

## Configuration
On `Startup.cs`:
```C#
public Startup(IConfiguration configuration)
{
    Configuration = configuration;
}

public IConfiguration Configuration { get; }

public void ConfigureServices(IServiceCollection services)
{
    services.AddQckMox(Configuration);
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    ...
    app.UseQckMox();

    app.UseRouting();
    ...
}
```

On `appsettings.json`:
```JSON
{
    "QckMox": {
      "EndPoint": "/api/",
      "ResponseSource": "Mocks",
      "Request": {
        "UnmatchedRequest": {
          "Passthrough": true
        }
      }
    }
}
```

On your API's root folder:
```
OutputDir\
  Mocks\
    weatherforecast\
      Get.json
    weathersummary\
      1\
        Get.json
      3\
        Get.json
      ...
  QckMox.Demo.Passthrough.dll
```

QckMox creates the `/api/` route within your API and any request made using this starting route will be responded to based on the folder structure in `Mocks`. Note that the default value for the `EndPoint` configuration is `/api/qckmox/`.

Also, any request that doesn't have a corresponding mock file will pass through QckMox which will processed by succeeding middlewares in the request pipeline as defined in `Startup.cs`. Note that the default value of the `Passthrough` configuration is `false`.

It is important to note that `app.UseQckMox()` must come before any other routing declarations to ensure that all unmatched request that passes through will be handled properly.

## Example
|Request|Response|
|:-|:-|
|`GET /api/weatherforecast`|Response will be the content of the file `Mocks\weatherforecast\Get.json`|
|`GET /api/weathersummary/[mocked number]`|Response will be the content of the file `Mocks\weathersummary\[N]\Get.json`|
|`GET /api/weathersummary/[other numbers]`|Response will be from the appropriate route in `WeatherSummaryController`|
|Any other endpoint starting with `/api/`|Response will be from the appropriate route in `WeatherSummaryController`|

## Additional Notes
Explanation on the `Mocks\weatherforecast\Get.json` is covered on the `02-standalone` demo. But in addition to this, QckMox will look for the property specified in the `fileContentProp` configuration to determine the value to use. Note that the default value of the `fileContentProp` configuration is `data`.

In this example, QckMox will look for the property `mock_data` in the response file, and will use the value of that property as the response.