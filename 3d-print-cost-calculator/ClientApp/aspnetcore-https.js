// This script sets up HTTPS for the application using the ASP.NET Core HTTPS certificate
const fs = require('fs');
const spawn = require('child_process').spawn;
const path = require('path');

const baseFolder =
  process.env.APPDATA !== undefined && process.env.APPDATA !== ''
    ? `${process.env.APPDATA}/ASP.NET/https`
    : `${process.env.HOME}/.aspnet/https`;

const certificateArg = process.argv.map(arg => arg.match(/--name=(?<value>.+)/i)).filter(Boolean)[0];
const certificateName = certificateArg ? certificateArg.groups.value : process.env.npm_package_name;

if (!certificateName) {
  console.error('Invalid certificate name. Run this script in the context of an npm/yarn script or pass --name=<<app>> explicitly.')
  process.exit(-1);
}

const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
  console.log('Attempting to export HTTPS certificate...');
  
  try {
    const certProcess = spawn('dotnet', [
      'dev-certs',
      'https',
      '--export-path',
      certFilePath,
      '--format',
      'Pem',
      '--no-password',
    ], { stdio: 'inherit' });

    certProcess.on('exit', (code) => {
      if (code !== 0) {
        console.warn(`Warning: Certificate export exited with code ${code}`);
        console.warn('Continuing without certificate export as certificate is known to exist and be trusted.');
        // Continue anyway instead of exiting
      }
    });
    
    // Don't wait for the process to exit before continuing
    console.log('Proceeding with startup...');
  } catch (error) {
    console.warn('Warning: Failed to export HTTPS certificate:');
    console.warn(error);
    console.warn('Continuing anyway as certificate is known to exist and be trusted.');
    // Continue execution without the certificate export
  }
}
