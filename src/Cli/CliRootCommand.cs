using KeyUtils.Cli.Derive;
using System.CommandLine;

namespace KeyUtils.Cli;

public class CliRootCommand : RootCommand
{
    public CliRootCommand(DeriveCommand deriveCommand, DerivePathCommand derivePathCommand) : base()
    {
        Add(deriveCommand);
        Add(derivePathCommand);
    }
}
