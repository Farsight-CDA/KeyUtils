using KeyUtils.Cli;
using System.CommandLine;

namespace KeyUtils;

public sealed class App(CliRootCommand rootCommand)
{
    public const string NAME = "KeyDeriverCLI";

    private readonly CliRootCommand _rootCommand = rootCommand;

    public Task<int> RunAsync(string[] arguments)
    {
        var parseResult = _rootCommand.Parse(arguments);
        return Task.FromResult(parseResult.Invoke());
    }
}
