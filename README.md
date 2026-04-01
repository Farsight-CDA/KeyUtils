# KeyUtils

A minimal CLI tool for deriving private keys from BIP39 mnemonics.

## Features

- **Small & Verifiable** – Single-file AOT binary, no runtime dependencies, easy to audit
- **Standards Compliant** – BIP39 (mnemonics) + BIP44 (derivation paths) + SLIP10
- **Multi-Chain** – Native support for EVM, Solana, Cosmos
- **Flexible** – Use predefined chains or specify custom derivation paths and curves

## Installation

Download the latest release for your platform from [Releases](../../releases).

Or build from source:

```bash
dotnet publish -c Release
```

## Usage

### Derive for a supported chain

```bash
keyutils derive-chain \
  --mnemonic-file mnemonic.txt \
  --type Evm \
  --output key.txt
```

Supported chain types: `Evm`, `Solana`, `Cosmos`

`--account-index` is optional and defaults to `0`.

### Derive with custom path

```bash
keyutils derive-path \
  --mnemonic-file mnemonic.txt \
  --path "m/44'/60'/0'/0/0" \
  --curve Secp256k1 \
  --output key.txt
```

Supported curves: `Secp256k1`, `Ed25519`

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

## License

MIT
