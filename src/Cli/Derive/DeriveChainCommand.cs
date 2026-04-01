using System.CommandLine;

namespace KeyUtils.Cli.Derive;

public class DeriveChainCommand : Command
{
    private readonly Option<FileInfo> _mnemonicFileOption = new("--mnemonic-file", "-i")
    {
        Description = "Path to the file containing the mnemonic",
        Required = true
    };

    private readonly Option<ChainType> _typeOption = new("--type")
    {
        Description = "The chain type (Evm, Solana, Cosmos)",
        Required = true
    };

    private readonly Option<int> _accountIndexOption = new("--account-index")
    {
        Description = "The account index to derive",
        Required = true
    };

    private readonly Option<FileInfo> _outputOption = new("--output", "-o")
    {
        Description = "Path to the file where the derived key will be saved",
        Required = true
    };

    public DeriveChainCommand() : base("derive-chain", "derive a key for a supported chain type")
    {
        Add(_mnemonicFileOption);
        Add(_typeOption);
        Add(_accountIndexOption);
        Add(_outputOption);

        SetAction(Handle);
    }

    private Task<int> Handle(ParseResult parseResult)
    {
        var mnemonicFile = parseResult.GetValue(_mnemonicFileOption)!;
        var type = parseResult.GetValue(_typeOption);
        int accountIndex = parseResult.GetValue(_accountIndexOption);
        var outputFile = parseResult.GetValue(_outputOption)!;

        var (path, curve) = DeriveKeySupport.ResolveChain(type, accountIndex);
        return DeriveKeySupport.DeriveAsync(mnemonicFile, path, curve, outputFile);
    }
}
