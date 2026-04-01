#!/usr/bin/env node

import { chmod, cp, mkdir, readFile, rm, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(scriptDirectory, '..');
const version = await readVersion(path.join(repoRoot, 'version.props'));
const assemblyName = process.env.KEYUTILS_ASSEMBLY_NAME ?? 'KeyUtils';
const npmScope = process.env.NPM_SCOPE ?? '@farsight-cda';
const publishRoot = path.join(repoRoot, 'artifacts', 'publish');
const npmRoot = path.join(repoRoot, 'artifacts', 'npm');
const repositoryUrl = 'https://github.com/Farsight-CDA/KeyUtils';

const platforms = [
  {
    rid: 'linux-x64',
    packageName: `${npmScope}/keyutils-linux-x64`,
    os: ['linux'],
    cpu: ['x64'],
    binaryName: assemblyName,
  },
  {
    rid: 'win-x64',
    packageName: `${npmScope}/keyutils-win-x64`,
    os: ['win32'],
    cpu: ['x64'],
    binaryName: `${assemblyName}.exe`,
  },
];

await rm(npmRoot, { recursive: true, force: true });
await mkdir(npmRoot, { recursive: true });

for (const platform of platforms) {
  const sourceDirectory = path.join(publishRoot, platform.rid);
  const packageDirectory = path.join(npmRoot, platform.rid);

  await cp(sourceDirectory, packageDirectory, { recursive: true, force: true });
  await writeJson(path.join(packageDirectory, 'package.json'), {
    name: platform.packageName,
    version,
    description: `Native ${assemblyName} binary for ${platform.rid}.`,
    license: 'MIT',
    repository: {
      type: 'git',
      url: `${repositoryUrl}.git`,
    },
    homepage: repositoryUrl,
    bugs: {
      url: `${repositoryUrl}/issues`,
    },
    os: platform.os,
    cpu: platform.cpu,
    preferUnplugged: true,
    bin: {
      keyutils: `./${platform.binaryName}`,
    },
    files: ['*'],
    publishConfig: {
      access: 'public',
    },
  });
  await writeFile(path.join(packageDirectory, 'README.md'), buildPlatformReadme(platform, assemblyName), 'utf8');

  if (!platform.rid.startsWith('win-')) {
    await chmod(path.join(packageDirectory, platform.binaryName), 0o755);
  }
}

const metaDirectory = path.join(npmRoot, 'meta');
await mkdir(path.join(metaDirectory, 'bin'), { recursive: true });

await writeJson(path.join(metaDirectory, 'package.json'), {
  name: `${npmScope}/keyutils`,
  version,
  description: 'Cross-platform KeyUtils CLI package.',
  license: 'MIT',
  repository: {
    type: 'git',
    url: `${repositoryUrl}.git`,
  },
  homepage: repositoryUrl,
  bugs: {
    url: `${repositoryUrl}/issues`,
  },
  bin: {
    keyutils: './bin/keyutils.js',
  },
  files: ['bin'],
  optionalDependencies: Object.fromEntries(platforms.map((platform) => [platform.packageName, version])),
  publishConfig: {
    access: 'public',
  },
});
await writeFile(path.join(metaDirectory, 'README.md'), buildMetaReadme(npmScope), 'utf8');
await writeFile(path.join(metaDirectory, 'bin', 'keyutils.js'), buildLauncher(platforms), 'utf8');
await chmod(path.join(metaDirectory, 'bin', 'keyutils.js'), 0o755);

async function readVersion(versionFilePath) {
  const versionFile = await readFile(versionFilePath, 'utf8');
  const match = versionFile.match(/<Version>([^<]+)<\/Version>/);

  if (!match) {
    throw new Error(`Could not find <Version> in ${versionFilePath}`);
  }

  return match[1].trim();
}

async function writeJson(filePath, value) {
  await writeFile(filePath, `${JSON.stringify(value, null, 2)}\n`, 'utf8');
}

function buildMetaReadme(scope) {
  return `# ${scope}/keyutils

Install KeyUtils with:

\`npm install -g ${scope}/keyutils\`

The package pulls in the matching platform-specific binary package for the current machine.
`;
}

function buildPlatformReadme(platform, binaryName) {
  return `# ${platform.packageName}

Platform-specific ${binaryName} package for ${platform.rid}.
`;
}

function buildLauncher(platforms) {
  const platformMap = Object.fromEntries(
    platforms.map((platform) => [`${platform.os[0]}-${platform.cpu[0]}`, {
      packageName: platform.packageName,
      binaryName: platform.binaryName,
    }]),
  );

  return `#!/usr/bin/env node

const path = require('node:path');
const { spawnSync } = require('node:child_process');

const platformPackages = ${JSON.stringify(platformMap, null, 2)};
const targetKey = \`${'${process.platform}'}-${'${process.arch}'}\`;
const target = platformPackages[targetKey];

if (!target) {
  console.error(\`KeyUtils does not support ${'${process.platform}'}/${'${process.arch}'}.\`);
  process.exit(1);
}

let packageJsonPath;

try {
  packageJsonPath = require.resolve(\`${'${target.packageName}'}/package.json\`);
} catch {
  console.error(\`The platform package ${'${target.packageName}'} is not installed. Reinstall the package on this platform.\`);
  process.exit(1);
}

const binaryPath = path.join(path.dirname(packageJsonPath), target.binaryName);
const result = spawnSync(binaryPath, process.argv.slice(2), {
  stdio: 'inherit',
  env: process.env,
});

if (result.error) {
  console.error(result.error.message);
  process.exit(1);
}

process.exit(result.status ?? 1);
`;
}
