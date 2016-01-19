To add the missing files, follow the next steps:

1] Download OpenSSL for Windows (https://slproweb.com/products/Win32OpenSSL.html)

2] From a command prompt:

    > set OPENSSL_CONF=c:\OpenSSL-Win32\bin\openssl.cfg
    > openssl genrsa -aes128 -out acceptant-private-key.pem -passout pass:P@ssw0rd1 2048
    > openssl req -x509 -sha256 -new -key acceptant-private-key.pem -passin pass:P@ssw0rd1 -days 1825 -out acceptant-public-certificate.cer
    > openssl pkcs12 -export -in acceptant-public-certificate.cer -inkey acceptant-private-key.pem -out acceptant-private-certificate.p12

    At the last step, enter P@ssw0rd1 as password, both as pass phrase and Export password.

3] Upload acceptant-public-certificate.cer using the portal of your acquirer.

4] Download the public certificate from your acquirer and rename it to "acquirer-public-certificate.cer".

5] In Web.config, change your merchant ID and acquirer URL to match for your acquirer.
