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

    private readonly Option<string?> _pathOption = new("--path")
    {
        Description = "The BIP44 derivation path",
        Required = false
    };

    private readonly Option<KeyType?> _typeOption = new("--type")
    {
        Description = "The key type (Cosmos, Ethereum, Solana)",
        Required = false
    };

    private readonly Option<int> _indexOption = new("--index")
    {
        Description = "The account index (defaults to 0)",
        Required = false
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
        Add(_typeOption);
        Add(_indexOption);
        Add(_outputOption);

        SetAction(Handle);
    }

    private async Task<int> Handle(ParseResult parseResult)
    {
        var mnemonicFile = parseResult.GetValue(_mnemonicFileOption)!;
        string? path = parseResult.GetValue(_pathOption);
        var type = parseResult.GetValue(_typeOption);
        int index = parseResult.GetValue(_indexOption);
        var outputFile = parseResult.GetValue(_outputOption)!;

        if(String.IsNullOrWhiteSpace(path) == (type is null))
        {
            Console.WriteLine("Error: You must specify either --path OR --type, but not both.");
            Console.WriteLine("Usage: derive -i <mnemonic> --type <type> [--index <index>] -o <output>");
            Console.WriteLine("       derive -i <mnemonic> --path <path> -o <output>");
            return 1;
        }

        if(!mnemonicFile.Exists)
        {
            Console.WriteLine($"Error: Mnemonic file not found at {mnemonicFile.FullName}");
            return 1;
        }

        if(type is not null)
        {
            path = type switch
            {
                KeyType.Cosmos => BIP44.Cosmos(index),
                KeyType.Ethereum => BIP44.Ethereum((uint) index),
                KeyType.Solana => BIP44.Solana(index),
                _ => throw new NotImplementedException()
            };
        }

        string mnemonic = (await File.ReadAllTextAsync(mnemonicFile.FullName)).Trim();
        byte[] seed = BIP39.MnemonicToSeed(mnemonic);
        uint[] pathIndexes = BIP44.Parse(path!);

        var (privateKey, _) = Slip10.DerivePath(Secp256k1.Instance, seed, pathIndexes);

        string privateKeyHex = Convert.ToHexStringLower(privateKey);
        await File.WriteAllTextAsync(outputFile.FullName, privateKeyHex);

        Console.WriteLine($"Successfully derived key and saved to {outputFile.FullName}");
        return 0;
    }
}

