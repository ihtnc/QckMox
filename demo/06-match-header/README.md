# QckMox Demo (Match Header)

This is a configuration setup demo that allows mapping of requests and their headers to the corresponding response.

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
      "ResponseMap": {
        // the key pertains to the request, and the value is the corresponding response for that request
        "POST summary/1 qmx-user-id=1": "summary\\1.json",
        "POST summary/1 qmx-user-id=3": "summary\\3.json",
        ...
        "POST summary/1 qmx-user-id=9": "summary\\9.json"
      },
      "Request": {
        "MatchHeader": ["user-id"]
      }
    }
}
```

On your API's root folder:
```
OutputDir\
  .qckmox\
    summary\
      1\
        GET.json
        POST qmx-user-id=2.json
        ...
      1.json
      3.json
      ...
  QckMox.Demo.MatchHeader.dll
```

QckMox creates the `/api/qckmox/` route within your API and any request made using this starting route will be responded to based on the QckMox `ResponseMap` configuration and folder structure in `.qckmox`.

If the QckMox configuration specifies a `ResponseMap`, which is basically a mapping of response files to a particular request, then a matching request will be responded to using the content of the file mapped to that request. By default, or if the request isn't mapped, QckMox will operate as normal and will try to locate the mock response by following the folder structure.

QckMox converts the request to a string and uses that string to look for the appropriate response. Since there could be an arbitrary number of request queries or request headers, and since there could be an even larger number of responses, QckMox needs to know the specific queries or headers that it needs to look at to speed up the process.

The `MatchHeader` configuration tells QckMox which request header it needs to include when converting the request to a string. QckMox will basically ignore the rest that are not included in the configuration.

## Example
|Request|Request Headers|Response|
|:-|:-|:-|
|`GET /api/qckmox/summary/1`||Response will be the content of the file `.qckmox\summary\1\GET.json`|
|`POST /api/qckmox/summary/1`|`"user-id: 1"`|Response will be the content of the file that matches the request as specified in the map|
|`POST /api/qckmox/summary/1`|`"user-id: 2"`|Response will be the content of the file `.qckmox\summary\1\POST qmx-user-id=2.json`|
|`POST /api/qckmox/summary/1`|`"user-id: 1", "other: header"`|Response will be the content of the file that matches the request as specified in the map. The `other` header is ignored.|
|`POST /api/qckmox/summary/1`|`"other: header"`|Response will be `HTTP 404`|
|`POST /api/qckmox/summary/2`|`"user-id: 1"`|Response will be `HTTP 404`|
|Any other endpoint starting with `/api/qckmox/`||Response will be `HTTP 404`|

## Additional Notes
Explanation on the format of the request string is covered on the `05-match-query` demo. But in addition to this, QckMox will prefix queries and headers with the appropriate tag when converting the request to string. This is to ensure that QckMox can differentiate between queries and headers when finding a match for the request.

So a `GET` request for `/api/qckmox/resources/1` with a header of `correlation-id: abc123` will, by default, be converted as the string `GET resources/1 qmx-correlation-id=abc123`. Note that the default tag for headers is `qmx-`.

Additional explanation on the `ResponseMap` configuration is covered on the `04-response-map` demo.

Explanation on the structure of the response files in this demo is covered on the `02-standalone` demo.