using Keysmith.Net.BIP;
using Keysmith.Net.EC;
using Keysmith.Net.ED;
using Keysmith.Net.SLIP;

namespace KeyUtils.Cli.Derive;

internal static class DeriveKeySupport
{
    public static ECCurve GetCurve(CurveType curveType)
        => curveType switch
        {
            CurveType.Ed25519 => ED25519.Instance,
            CurveType.Secp256k1 => Secp256k1.Instance,
            _ => throw new NotImplementedException()
        };

    public static (string Path, ECCurve Curve) ResolveChain(ChainType chainType, int accountIndex) => chainType switch
    {
        ChainType.Cosmos => (BIP44.Cosmos(accountIndex), Secp256k1.Instance),
        ChainType.Evm => (BIP44.Ethereum((uint) accountIndex), Secp256k1.Instance),
        ChainType.Solana => (BIP44.Solana(accountIndex), ED25519.Instance),
        _ => throw new NotImplementedException()
    };

    public static async Task<int> DeriveAsync(FileInfo mnemonicFile, string path, ECCurve curve, FileInfo? outputFile)
    {
        if(!mnemonicFile.Exists)
        {
            Console.WriteLine($"Error: Mnemonic file not found at {mnemonicFile.FullName}");
            return 1;
        }

        string mnemonic = (await File.ReadAllTextAsync(mnemonicFile.FullName)).Trim();
        byte[] seed = BIP39.MnemonicToSeed(mnemonic);
        uint[] pathIndexes = BIP44.Parse(path);
        var (privateKey, _) = Slip10.DerivePath(curve, seed, pathIndexes);

        string privateKeyHex = Convert.ToHexStringLower(privateKey);

        if(outputFile is null)
        {
            Console.WriteLine(privateKeyHex);
            return 0;
        }

        await File.WriteAllTextAsync(outputFile.FullName, privateKeyHex);
        Console.WriteLine($"Successfully derived key and saved to {outputFile.FullName}");
        return 0;
    }
}
