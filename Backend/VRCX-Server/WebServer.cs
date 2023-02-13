namespace VRCX_Server;

public class WebServer
{
    public WebServer(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public static void Start(string[] args)
    {
        Task.Run(() => { CreateHostBuilder(args).Build().Run(); });
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<WebServer>()
                    .UseUrls("http://0.0.0.0:5000");
            });
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSpaStaticFiles(options => { options.RootPath = "dist"; });
        services.AddCors(options =>
        {
            options.AddPolicy("VueCorsPolicy", builder =>
            {
                builder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithOrigins("https://localhost:5173");
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors("VueCorsPolicy");
        app.UseWebSockets();
        app.UseRouting();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        app.UseSpaStaticFiles();
        app.UseSpa(builder =>
        {
            if (env.IsDevelopment()) builder.UseProxyToSpaDevelopmentServer("http://localhost:5173/");
        });
    }
}