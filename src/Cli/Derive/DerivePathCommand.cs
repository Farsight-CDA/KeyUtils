using System.CommandLine;

namespace KeyUtils.Cli.Derive;

public class DerivePathCommand : Command
{
    private readonly Option<FileInfo> _mnemonicFileOption = new("--mnemonic-file", "-i")
    {
        Description = "Path to the file containing the mnemonic",
        Required = true
    };

    private readonly Option<string> _pathOption = new("--path")
    {
        Description = "The BIP44 derivation path",
        Required = true
    };

    private readonly Option<CurveType> _curveOption = new("--curve")
    {
        Description = "The elliptic curve to use (Secp256k1, Ed25519)",
        Required = true
    };

    private readonly Option<FileInfo> _outputOption = new("--output", "-o")
    {
        Description = "Path to the file where the derived key will be saved",
        Required = true
    };

    public DerivePathCommand() : base("derive-path", "derive a key for an explicit path and curve")
    {
        Add(_mnemonicFileOption);
        Add(_pathOption);
        Add(_curveOption);
        Add(_outputOption);

        SetAction(Handle);
    }

    private Task<int> Handle(ParseResult parseResult)
    {
        var mnemonicFile = parseResult.GetValue(_mnemonicFileOption)!;
        string path = parseResult.GetValue(_pathOption)!;
        var curve = DeriveKeySupport.GetCurve(parseResult.GetValue(_curveOption));
        var outputFile = parseResult.GetValue(_outputOption)!;

        return DeriveKeySupport.DeriveAsync(mnemonicFile, path, curve, outputFile);
    }
}
