# QckMox Demo (Standalone)

This is the simplest configuration setup demo (presented as a standalone API instead of an internal API call).

Basically, you only need to "add" and "use" QckMox in Startup.cs, add a simple configuration in `appsettings.json`, and add the responses as json files in the default QckMox folder.

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
    app.UseQckMox();
}
```

On `appsettings.json`:
```JSON
{
    "QckMox": {
      "EndPoint": "/api/"
    }
}
```

On your API's root folder:
```
OutputDir\
  .qckmox\
    weatherforecast\
      Get.json
    weathersummary\
      1\
        Get.json
  QckMox.Demo.Standalone.dll
```

QckMox creates the `/api/` route within your API and any request made using this starting route will be responded to based on the folder structure in `.qckmox`. Note that the default value for the `EndPoint` configuration is `/api/qckmox/`.

## Example
|Request|Response|
|:-|:-|
|`GET /api/weatherforecast`|Response will be the content of the file `.qckmox\weatherforecast\Get.json`|
|`GET /api/weathersummary/1`|Response will be the content of the file `.qckmox\weathersummary\1\Get.json`|
|Any other endpoint starting with `/api/`|Response will be `HTTP 404`|

## Additional Notes
Response files should always be in a `JSON object` format. Since the response of `GET /api/weatherforecast` is a `JSON array`, a configuration section needs to be added within the response file itself.

This enables QckMox to process the file properly and return the appropriate response.

On `.qckmox/weatherforecast/Get.json`
```JSON
{
    "data": [...],
    "_qckmox": {
        "contentInProp": true
    }
}
```

QckMox looks for the JSON property `_qckmox` on each response file and retrieves any configuration items contained therein, combines it with other defined global configurations, and uses those configuration items to process that particular response file.

In this example, the configuration property `contentInProp` tells QckMox to look for the response value in a particular property.

By default, QckMox will look for the `data` property in the response file as long as `contentInProp` is set to `true`.

If there is no `_qckmox` property on the file, or if `contentInProp` is set to `false` then QckMox will treat the entire file as the response value itself. Note that QckMox strips the `_qckmox` property before sending the value as a response.