# QckMox Demo (Match Parameters)

This is a configuration setup demo that allows mapping of requests, their queries, and their headers to the corresponding response.

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
        "POST summary id=1&qmx-user-id=1": "summary\\1_1.json",
        "POST summary id=1&qmx-user-id=3": "summary\\1_3.json",
        "POST summary qmx-user-id=2&id=2": "summary\\2_2.json",
        "POST summary qmx-user-id=4&id=2": "summary\\2_4.json",
        "POST summary id=3": "summary\\3_x.json"
      },
      "Request": {
        "MatchQuery": ["id"],
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
      1_1.json
      1_3.json
      ...
      POST qmx-user-id=4&id=1.json
      POST qmx-user-id=5.json
  QckMox.Demo.MatchParameters.dll
```

QckMox creates the `/api/qckmox/` route within your API and any request made using this starting route will be responded to based on the QckMox `ResponseMap` configuration and folder structure in `.qckmox`.

If the QckMox configuration specifies a `ResponseMap`, which is basically a mapping of response files to a particular request, then a matching request will be responded to using the content of the file mapped to that request. By default, or if the request isn't mapped, QckMox will operate as normal and will try to locate the mock response by following the folder structure.

QckMox converts the request to a string and uses that string to look for the appropriate response. Since there could be an arbitrary number of request queries or request headers, and since there could be an even larger number of responses, QckMox needs to know the specific queries or headers that it needs to look at to speed up the process.

The `MatchQuery` and `MatchHeader` configurations tell QckMox which request query or header it needs to include when converting the request to a string. QckMox will basically ignore the rest that are not included in the configuration.

## Example
|Request|Request Headers|Response|
|:-|:-|:-|
|`GET /api/qckmox/summary?id=1`||Response will be the content of the file `.qckmox\summary\GET id=1.json`|
|`POST /api/qckmox/summary?id=1`|`"user-id: 1"`|Response will be the content of the file that matches the request as specified in the map|
|`POST /api/qckmox/summary?id=1`|`"user-id: 2"`|Response will be the content of the file `.qckmox\summary\POST qmx-user-id=2&id=1.json`|
|`POST /api/qckmox/summary?id=2&other=query`|`"user-id: 1"`|Response will be the content of the file `.qckmox\summary\POST id=2&qmx-user-id=1.json`. The `other` query is ignored.|
|`POST /api/qckmox/summary?id=2`|`"user-id: 2", "other: header"`|Response will be the content of the file that matches the request as specified in the map. The `other` header is ignored.|
|`POST /api/qckmox/summary?id=3`||Response will be the content of the file that matches the request as specified in the map|
|`POST /api/qckmox/summary?other=query`|`"user-id: 5"`|Response will be the content of the file `.qckmox\summary\POST qmx-user-id=5.json`.|
|`POST /api/qckmox/summary?id=1`|`"user-id: 9"`|Response will be `HTTP 404`|
|`POST /api/qckmox/summary?id=9`|`"user-id: 1"`|Response will be `HTTP 404`|
|`POST /api/qckmox/summary?id=9`|`"other: header"`|Response will be `HTTP 404`|
|`POST /api/qckmox/summary?other=query`|`"user-id=9"`|Response will be `HTTP 404`|
|`POST /api/qckmox/summary?id=9`|`"user-id: 9"`|Response will be `HTTP 404`|
|Any other endpoint starting with `/api/qckmox/`||Response will be `HTTP 404`|

## Additional Notes
Explanation on the format of the request string is covered on the `05-match-query` and `06-match-header` demos. But in addition, even though QckMox will look at both `MatchQuery` and `MatchHeader` configurations when converting the request to the internal request string format, it won't insist on the presence of those parameters.

So a QckMox endpoint with `MatchQuery: ['id']` and `MatchHeader: ['correlation-id']` configuration, and a `GET` request for `/api/qckmox/resources?id=1`, will be converted as the string `GET resources id=1` without errors even though the `correlation-id` header is missing from the request.

Consider the `MatchQuery` and `MatchHeader` configuration as a way of telling QckMox that if the specified queries or headers exist in the request, then add those in the request string conversion.

Note that the order of the request query or header on the converted request string does not matter when matching the request. QckMox will be able to match the request string to the actual request provided every parameter matches. However, it is recommended to sort the parameters (including their prefix tags) alphabetically, for performance.

Additional explanation on the `ResponseMap` configuration is covered on the `04-response-map` demo.

Explanation on the structure of the response files in this demo is covered on the `02-standalone` demo.