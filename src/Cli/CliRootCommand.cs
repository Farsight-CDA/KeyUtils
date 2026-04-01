using KeyUtils.Cli.Derive;
using System.CommandLine;

namespace KeyUtils.Cli;

public class CliRootCommand : RootCommand
{
    public CliRootCommand(DeriveChainCommand deriveChainCommand, DerivePathCommand derivePathCommand) : base()
    {
        Add(deriveChainCommand);
        Add(derivePathCommand);
    }
}
