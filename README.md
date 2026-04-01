# KeyUtils

A minimal CLI tool for deriving private keys from BIP39 mnemonics.

## Features

- **Small & Verifiable** – Single-file AOT binary, no runtime dependencies, easy to audit
- **Standards Compliant** – BIP39 (mnemonics) + BIP44 (derivation paths) + SLIP10
- **Multi-Chain** – Native support for EVM, Solana, Cosmos
- **Flexible** – Use predefined chains or specify custom derivation paths and curves

## Installation

Download the latest release archive for your platform from [Releases](../../releases) and extract it.

Each release bundle includes the `KeyUtils` executable and the native libraries it needs, including `secp256k1`.

Or install from npm:

```bash
npm install -g @farsight-cda/keyutils
```

The npm meta package installs the matching platform package for the current OS, including the bundled native libraries.

Or build from source:

```bash
dotnet publish -c Release -r linux-x64 --self-contained true -o artifacts/publish/linux-x64
```

## Usage

### Derive for a supported chain

```bash
keyutils derive solana \
  --mnemonic-file mnemonic.txt \
  --format base58 \
  --output key.txt
```

Supported chain commands: `solana`, `evm`, `cosmos`

`--account-index` is optional and defaults to `0`.

`derive solana` defaults to `--format base58`.

`derive evm` and `derive cosmos` only support `--format hex`.

### Derive with custom path

```bash
keyutils derive-path ed25519 \
  --mnemonic-file mnemonic.txt \
  --path "m/44'/501'/0'/0'/0'" \
  --format hex \
  --output key.txt
```

Supported path subcommands: `ed25519`, `secp256k1`

`derive-path ed25519` supports `hex`, `base58`, and `json`.

`derive-path secp256k1` only supports `hex`.

Format meanings:

- `hex`: raw 32-byte private key as lowercase hex
- `base58`: Ed25519 64-byte keypair encoded for Solana/Rust `Keypair::from_base58_string(...)`
- `json`: Ed25519 64-byte keypair as a JSON byte array

## How It Works

```
BIP39 Mnemonic → Seed → BIP44 Path + Curve → Private Key
```

| Chain   | Default Path         | Curve      |
|---------|---------------------|------------|
| EVM     | m/44'/60'/0'/0/N    | Secp256k1  |
| Solana  | m/44'/501'/N'/0'/0' | Ed25519    |
| Cosmos  | m/44'/118'/0'/0/N   | Secp256k1  |

## Security

- No network connectivity
- No key storage or caching
- Private keys written to user-specified output only
- Entirely offline operation

## Release Automation

The release workflow reads the package version from `version.props`, creates the matching GitHub release tag, and publishes these npm packages:

- `@farsight-cda/keyutils`
- `@farsight-cda/keyutils-linux-x64`
- `@farsight-cda/keyutils-win-x64`

Publishing to npm requires an `NPM_TOKEN` repository secret with permission to publish those packages.

## License

MIT
