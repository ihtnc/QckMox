# QckMox Demo (Match Query)

This is a configuration setup demo that allows mapping of requests and their queries to the corresponding response.

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
        "GET weathersummary id=1": "weathersummary\\1.json",
        ...
        "GET weathersummary id=9": "weathersummary\\9.json",
      },
      "Request": {
        "MatchQuery": ["id"]
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
      1.json
      3.json
      ...
      GET id=8.json
      GET id=10.json
  QckMox.Demo.MatchQuery.dll
```

QckMox creates the `/api/` route within your API and any request made using this starting route will be responded to based on the QckMox `ResponseMap` configuration and folder structure in `.qckmox`. Note that the default value for the `EndPoint` configuration is `/api/qckmox/`.

If the QckMox configuration specifies a `ResponseMap`, which is basically a mapping of response files to a particular request, then a matching request will be responded to using the content of the file mapped to that request. By default, or if the request isn't mapped, QckMox will operate as normal and will try to locate the mock response by following the folder structure.

QckMox converts the request to a string and uses that string to look for the appropriate response. Since there could be an arbitrary number of request queries or request headers, and since there could be an even larger number of responses, QckMox needs to know the specific queries or headers that it needs to look at to speed up the process.

The `MatchQuery` configuration tells QckMox which request query it needs to include when converting the request to a string. QckMox will basically ignore the rest that are not included in the configuration.

## Example
|Request|Response|
|:-|:-|
|`GET /api/weatherforecast`|Response will be the content of the file `.qckmox\weatherforecast\forecast.json`|
|`GET /api/weathersummary?id=[mapped number]`|Response will be the content of the file that matches the request as specified in the map|
|`GET /api/weathersummary?id=2`|Response will be the content of the file `.qckmox\weathersummary\GET id=2.json`|
|`GET /api/weathersummary?id=3&other=query`|Response will be the content of the file that matches the request as specified in the map. The `other` query is ignored.|
|`GET /api/weathersummary?id=[other numbers]`|Response will be `HTTP 404`|
|`GET /api/weathersummary?other=query`|Response will be `HTTP 404`|
|Any other endpoint starting with `/api/`|Response will be `HTTP 404`|

## Additional Notes
QckMox uses a particular format to convert a request to a string. This string is used to identify the request either in the `ResponseMap` configuration, or in the folder structure.

Basically, the request string is the string representation of the pertinent data associated to a particular request.

The request string format is as follows:

```
Format = [Method][space][Resource][space][Parameters]
```

|Component|Description|Notes|
|:-|:-|:-|
|Method|GET, POST, PUT, etc|Value is `case-insensitive`.|
|Resource|Request URI|`Optional` depending on context. Value is relative to the EndPoint configuration, without the query string. Value is `case-insensitive`.|
|Parameters|[`Queries`][`Headers`]|`Optional`. An ampersand (`&`) delimited list of name=value pairs.|
|Queries|[Tag][Name]=[Value]|An ampersand (`&`) delimited list of name=value pairs for each item in the request query string. By default, there are `no tags` for queries. Name is `case-insensitive` but the value is `case-sensitive`.|
|Headers|[Tag][Name]=[Value]|An ampersand (`&`) delimited list of name=value pairs for each item in the request header. By default, the tag for headers is `qmx-`. Name is `case-insensitive` but the value is `case-sensitive`.|

So a `GET` request for `/api/qckmox/resources?page=1&sort=asc` will, by default, be converted as the string `GET resources page=1&sort=asc`.

When used in the `ResponseMap` configuration, the request string usually includes the `Resource` component. But when it is used to locate the response within the `ResponseSource` folder structure, the `Resource` is not expected since the assumption is that the folder structure will resemble the request URI.

Since the request string can be used to locate response files in a folder, certain characters are not allowed (i.e.: `?`, `*`, `:`, `<`, `>`, etc). It is recommended to `URL encode` values specially in the request query string or request headers.

Additional explanation on the `ResponseMap` configuration is covered on the `04-response-map` demo.

Explanation on the structure of the `.qckmox\weatherforecast\Forecast.json` file is covered on the `02-standalone` demo.