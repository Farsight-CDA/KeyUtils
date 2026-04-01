using Keysmith.Net.BIP;
using Keysmith.Net.EC;
using Keysmith.Net.ED;
using Keysmith.Net.SLIP;
using System.Numerics;
using System.Text;

namespace KeyUtils.Cli.Derive;

internal static class DeriveKeySupport
{
    private const string BASE58_ALPHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

    public static async Task<int> DeriveAsync(FileInfo mnemonicFile, string path, ECCurve curve, OutputFormat format, FileInfo? outputFile)
    {
        if(!mnemonicFile.Exists)
        {
            Console.WriteLine($"Error: Mnemonic file not found at {mnemonicFile.FullName}");
            return 1;
        }

        string mnemonic = (await File.ReadAllTextAsync(mnemonicFile.FullName)).Trim();
        byte[] seed = BIP39.MnemonicToSeed(mnemonic);

        try
        {
            uint[] pathIndexes = BIP44.Parse(path);
            var (privateKey, _) = Slip10.DerivePath(curve, seed, pathIndexes);
            string output = FormatKey(privateKey, curve, format);

            if(outputFile is null)
            {
                Console.WriteLine(output);
                return 0;
            }

            await File.WriteAllTextAsync(outputFile.FullName, output);
            Console.WriteLine($"Successfully derived key and saved to {outputFile.FullName}");
            return 0;
        }
        catch(ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static string FormatKey(ReadOnlySpan<byte> privateKey, ECCurve curve, OutputFormat format)
        => format switch
        {
            OutputFormat.Hex => Convert.ToHexStringLower(privateKey),
            OutputFormat.Base58 => EncodeBase58(CreateEd25519KeypairBytes(privateKey, curve)),
            OutputFormat.Json => EncodeJsonArray(CreateEd25519KeypairBytes(privateKey, curve)),
            _ => throw new NotImplementedException()
        };

    private static byte[] CreateEd25519KeypairBytes(ReadOnlySpan<byte> privateKey, ECCurve curve)
    {
        if(curve is not EdwardCurve edwardCurve || curve != ED25519.Instance)
        {
            throw new ArgumentException("The selected format is only supported for Ed25519 derivations.");
        }

        byte[] keypairBytes = new byte[privateKey.Length + edwardCurve.PublicKeyLength];
        privateKey.CopyTo(keypairBytes);
        edwardCurve.MakePublicKey(privateKey, keypairBytes.AsSpan(privateKey.Length));
        return keypairBytes;
    }

    private static string EncodeJsonArray(ReadOnlySpan<byte> bytes)
    {
        var builder = new StringBuilder(bytes.Length * 4 + 2);
        builder.Append('[');

        for(int i = 0; i < bytes.Length; i++)
        {
            if(i > 0)
            {
                builder.Append(',');
            }

            builder.Append(bytes[i]);
        }

        builder.Append(']');
        return builder.ToString();
    }

    private static string EncodeBase58(ReadOnlySpan<byte> bytes)
    {
        if(bytes.IsEmpty)
        {
            return String.Empty;
        }

        int leadingZeroCount = 0;
        while(leadingZeroCount < bytes.Length && bytes[leadingZeroCount] == 0)
        {
            leadingZeroCount++;
        }

        var value = new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
        var builder = new StringBuilder(bytes.Length * 2);

        while(value > BigInteger.Zero)
        {
            value = BigInteger.DivRem(value, 58, out var remainder);
            builder.Append(BASE58_ALPHABET[(int) remainder]);
        }

        for(int i = 0; i < leadingZeroCount; i++)
        {
            builder.Append(BASE58_ALPHABET[0]);
        }

        char[] encoded = new char[builder.Length];
        for(int i = 0; i < builder.Length; i++)
        {
            encoded[i] = builder[builder.Length - 1 - i];
        }

        return new string(encoded);
    }
}
