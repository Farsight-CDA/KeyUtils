using Keysmith.Net.BIP;
using Keysmith.Net.EC;
using Keysmith.Net.SLIP;
using System.CommandLine;

namespace KeyUtils.Cli.Derive;

public class DeriveCommand : Command
{
    private readonly Option<FileInfo> _mnemonicFileOption = new("--mnemonic-file", "mf")
    {
        Description = "Path to the file containing the mnemonic"
    };
    private readonly Option<string> _pathOption = new("--path")
    {
        Description = "The BIP44 derivation path"
    };
    private readonly Option<FileInfo> _outputOption = new("--output", "o")
    {
        Description = "Path to the file where the derived key will be saved"
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
        var mnemonicFile = parseResult.GetValue(_mnemonicFileOption);
        string? path = parseResult.GetValue(_pathOption);
        var outputFile = parseResult.GetValue(_outputOption);

        if(mnemonicFile is null)
        {
            Console.WriteLine("Error: --mnemonic-file is required");
            return 1;
        }

        if(path is null)
        {
            Console.WriteLine("Error: --path is required");
            return 1;
        }

        if(outputFile is null)
        {
            Console.WriteLine("Error: --output is required");
            return 1;
        }

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

