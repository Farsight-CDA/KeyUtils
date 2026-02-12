using KeyUtils;
using KeyUtils.Cli;
using KeyUtils.Cli.Derive;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

int exitCode;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<App>();
        services.AddTransient<CliRootCommand>();
        services.AddTransient<DeriveCommand>();
    })
    .Build();

try
{
    var app = host.Services.GetRequiredService<App>();
    exitCode = await app.RunAsync(args.Length == 0 ? ["-?"] : args);
}
catch(Exception ex)
{
    Console.WriteLine($"Error during execution: {ex}");
    exitCode = -1;
}

return exitCode;
