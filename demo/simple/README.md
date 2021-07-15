# QckMox Demo (Simple)

This is the simplest configuration setup demo.

Basically, you only need to "add" and "use" QckMox in Startup.cs, then add the responses as json files in the default QckMox folder.

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

On your API's root folder:
```
OutputDir\
  .qckmox\
    summary\
      1\
        Get.json
        Post.json
      Get.json
  QckMox.Demo.Simple.dll
```

QckMox creates the `/api/qckmox/` route within your API and any request made using this starting route will be responded to based on the folder structure in `.qckmox`.

## Example
|Request|Response|
|:-|:-|
|`GET /api/qckmox/summary`|Response will be the content of the file `.qckmox\summary\Get.json`|
|`GET /api/qckmox/summary/1`|Response will be the content of the file `.qckmox\summary\1\Get.json`|
|`POST /api/qckmox/summary/1`|Response will be the content of the file `.qckmox\summary\1\Post.json`|
|Any other endpoint starting with `/api/qckmox/`|Response will be `HTTP 404`|