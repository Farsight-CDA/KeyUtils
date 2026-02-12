using KeyUtils.Cli;

namespace KeyUtils;

public sealed class App(CliRootCommand rootCommand)
{
    public const string NAME = "KeyUtils";

    private readonly CliRootCommand _rootCommand = rootCommand;

    public Task<int> RunAsync(string[] arguments)
    {
        var parseResult = _rootCommand.Parse(arguments);
        return Task.FromResult(parseResult.Invoke());
    }
}
