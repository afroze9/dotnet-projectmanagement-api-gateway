using ProjectManagement.ApiGateway.Extensions;

namespace ProjectManagement.ApiGateway;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddApplicationConfiguration(builder.Environment);
        builder.Services.RegisterDependencies(builder.Configuration);

        WebApplication app = builder.Build();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseRouting();
        app.UseCustomOcelot().Wait();
        app.Run();
    }
}