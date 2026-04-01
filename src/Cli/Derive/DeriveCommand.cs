using Keysmith.Net.BIP;
using Keysmith.Net.EC;
using Keysmith.Net.ED;
using System.CommandLine;

namespace KeyUtils.Cli.Derive;

public sealed class DeriveCommand : Command
{
    public DeriveCommand() : base("derive", "derive a key for a supported chain")
    {
        Add(new DeriveChainLeafCommand(
            "solana",
            "derive a Solana key",
            BIP44.Solana,
            ED25519.Instance,
            [OutputFormat.Hex, OutputFormat.Base58, OutputFormat.Json],
            OutputFormat.Base58));

        Add(new DeriveChainLeafCommand(
            "evm",
            "derive an EVM key",
            accountIndex => BIP44.Ethereum((uint) accountIndex),
            Secp256k1.Instance,
            [OutputFormat.Hex],
            OutputFormat.Hex));

        Add(new DeriveChainLeafCommand(
            "cosmos",
            "derive a Cosmos key",
            BIP44.Cosmos,
            Secp256k1.Instance,
            [OutputFormat.Hex],
            OutputFormat.Hex));
    }

    private sealed class DeriveChainLeafCommand : Command
    {
        private readonly Func<int, string> _pathFactory;
        private readonly ECCurve _curve;
        private readonly OutputFormat[] _supportedFormats;

        private readonly Option<FileInfo> _mnemonicFileOption = new("--mnemonic-file", "-i")
        {
            Description = "Path to the file containing the mnemonic",
            Required = true
        };

        private readonly Option<int> _accountIndexOption = new("--account-index")
        {
            Description = "The account index to derive",
            DefaultValueFactory = _ => 0
        };

        private readonly Option<string> _formatOption;

        private readonly Option<FileInfo?> _outputOption = new("--output", "-o")
        {
            Description = "Path to the file where the derived key will be saved"
        };

        public DeriveChainLeafCommand(
            string name,
            string description,
            Func<int, string> pathFactory,
            ECCurve curve,
            OutputFormat[] supportedFormats,
            OutputFormat defaultFormat) : base(name, description)
        {
            _pathFactory = pathFactory;
            _curve = curve;
            _supportedFormats = supportedFormats;
            _formatOption = new Option<string>("--format", "-f")
            {
                Description = $"Output format ({OutputFormatExtensions.Describe(supportedFormats)}). Defaults to {defaultFormat.ToCliValue()}",
                DefaultValueFactory = _ => defaultFormat.ToCliValue()
            };
            _formatOption.AcceptOnlyFromAmong([.. supportedFormats.Select(format => format.ToCliValue())]);

            Add(_mnemonicFileOption);
            Add(_accountIndexOption);
            Add(_formatOption);
            Add(_outputOption);

            SetAction(Handle);
        }

        private Task<int> Handle(ParseResult parseResult)
        {
            var mnemonicFile = parseResult.GetValue(_mnemonicFileOption)!;
            int accountIndex = parseResult.GetValue(_accountIndexOption);
            string formatText = parseResult.GetValue(_formatOption)!;
            var outputFile = parseResult.GetValue(_outputOption);

            if(!OutputFormatExtensions.TryParse(formatText, out var format) || !_supportedFormats.Contains(format))
            {
                Console.WriteLine($"Error: Unsupported format '{formatText}'. Supported formats: {OutputFormatExtensions.Describe(_supportedFormats)}");
                return Task.FromResult(1);
            }

            return DeriveKeySupport.DeriveAsync(mnemonicFile, _pathFactory(accountIndex), _curve, format, outputFile);
        }
    }
}
