# QckMox

A quick but flexible API mocking tool for .NET Core APIs.

## Quick Start
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
    demo\
      Get.json
  YourApi.dll
```

QckMox creates the `/api/qckmox/` route within your API and any request made using this starting route will be responded to based on the folder structure in `.qckmox`.

In the example above, the response received when making the request `GET /api/qckmox/demo` will be the content of the file `.qckmox\demo\Get.json`. Any other request made using the `/api/qckmox` route will result in HTTP 404.

More examples in the demo folder.