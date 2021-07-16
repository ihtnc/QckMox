# QckMox Demo (Response Map)

This is a configuration setup demo that allows mapping of requests to specific responses.

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
      "EndPoint": "/api/",
      "ResponseMap": {
        // the key pertains to the request, and the value is the corresponding response for that request
        "GET weatherforecast": "weatherforecast\\Forecast.json",
        "GET weathersummary/1": "weathersummary\\1.json",
        ...
        "GET weathersummary/8": "weathersummary\\Default.json",
      }
    }
}
```

On your API's root folder:
```
OutputDir\
  .qckmox\
    weatherforecast\
      Forecast.json
    weathersummary\
      9\
        Get.json
      10\
        Get.json
      1.json
      3.json
      ...
      Default.json
  QckMox.Demo.ResponseMap.dll
```

QckMox creates the `/api/` route within your API and any request made using this starting route will be responded to based on the QckMox `ResponseMap` configuration and folder structure in `.qckmox`. Note that the default value for the `EndPoint` configuration is `/api/qckmox/`.

If the QckMox configuration specifies a `ResponseMap`, which is basically a mapping of response files to a particular request, then a matching request will be responded to using the content of the file mapped to that request. By default, or if the request isn't mapped, QckMox will operate as normal and will try to locate the mock response by following the folder structure.

## Example
|Request|Response|
|:-|:-|
|`GET /api/weatherforecast`|Response will be the content of the file `.qckmox\weatherforecast\forecast.json`|
|`GET /api/weathersummary/[mapped number]`|Response will be the content of the file that matches the request as specified in the map|
|`GET /api/weathersummary/9`|Response will be the content of the file `.qckmox\weathersummary\9\Get.json`|
|`GET /api/weathersummary/10`|Response will be the content of the file `.qckmox\weathersummary\10\Get.json`|
|`GET /api/weathersummary/[other numbers]`|Response will be `HTTP 404`|
|Any other endpoint starting with `/api/`|Response will be `HTTP 404`|

## Additional Notes
The key on the `ResponseMap` configuration is relative to the value of the `EndPoint` configuration. In this demo, since the `EndPoint` configuration is `/api/`, the mapping for the weatherforecast request only needs to specify `GET weatherforecast` as the key instead of `GET /api/weatherforecast`.

Similarly, the value on the `ResponseMap` configuration is relative to the value of the `ResponseSource` configuration. In this demo, since the default `ResponseSource` value is `.qckmox`, the mapping for the weatherforecast response only needs to specify `weatherforecast\\Forecast.json` as the value instead of `.qckmox\\weatherforecast\\Forecast.json`.

Explanation on the structure of the `.qckmox\weatherforecast\Forecast.json` file is covered on the `02-standalone` demo.