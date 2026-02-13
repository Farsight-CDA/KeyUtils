using Keysmith.Net.BIP;
using Keysmith.Net.EC;
using Keysmith.Net.SLIP;
using System.CommandLine;

namespace KeyUtils.Cli.Derive;

public class DeriveCommand : Command
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
    private readonly Option<FileInfo> _outputOption = new("--output", "-o")
    {
        Description = "Path to the file where the derived key will be saved",
        Required = true
    };

    public DeriveCommand() : base("derive", "derive private keys from mnemonics")
    {
        Add(_mnemonicFileOption);
        Add(_pathOption);
        Add(_outputOption);

        SetAction(Handle);
    }

    private async Task<int> Handle(ParseResult parseResult)
    {
        var mnemonicFile = parseResult.GetRequiredValue(_mnemonicFileOption);
        string path = parseResult.GetRequiredValue(_pathOption);
        var outputFile = parseResult.GetRequiredValue(_outputOption);

        if(!mnemonicFile.Exists)
        {
            Console.WriteLine($"Error: Mnemonic file not found at {mnemonicFile.FullName}");
            return 1;
        }

        string mnemonic = (await File.ReadAllTextAsync(mnemonicFile.FullName)).Trim();
        byte[] seed = BIP39.MnemonicToSeed(mnemonic);
        uint[] pathIndexes = BIP44.Parse(path);

        var (privateKey, _) = Slip10.DerivePath(Secp256k1.Instance, seed, pathIndexes);

        string privateKeyHex = Convert.ToHexStringLower(privateKey);
        await File.WriteAllTextAsync(outputFile.FullName, privateKeyHex);

        Console.WriteLine($"Successfully derived key and saved to {outputFile.FullName}");
        return 0;
    }
}

