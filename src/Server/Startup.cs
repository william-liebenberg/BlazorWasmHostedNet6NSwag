using System.Text.Json.Serialization;

namespace MyApp.Server;

public class Startup
{
	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	public IConfiguration Configuration { get; }

	public void ConfigureServices(IServiceCollection services)
	{
		services.AddControllersWithViews()
			.AddJsonOptions(config =>
				config.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

		services.AddRazorPages();

		services.AddOpenApiDocument();
	}
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		// Configure the HTTP request pipeline.
		if (env.IsDevelopment())
		{
			app.UseWebAssemblyDebugging();
		}
		else
		{
			app.UseExceptionHandler("/Error");
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
		}

		app.UseHttpsRedirection();

		app.UseBlazorFrameworkFiles();
		app.UseStaticFiles();

		app.UseOpenApi();
		app.UseSwaggerUi3();

		app.UseRouting();

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapRazorPages();
			endpoints.MapControllers();
			endpoints.MapFallbackToFile("index.html");
		});
	}
}