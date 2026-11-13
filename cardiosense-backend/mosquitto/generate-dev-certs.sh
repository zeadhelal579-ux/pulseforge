#!/bin/sh
# =============================================================================
# File    : generate-dev-certs.sh
# Purpose : Creates a throw-away CA and a broker server certificate for local development.
# Logic   : Runs once inside the `certs` compose job (alpine/openssl image). It is idempotent: if the certificates
#           already exist it does nothing, so restarting the stack never invalidates a certificate a device already
#           trusts. The CA private key is deleted right after signing - nothing else needs it. Files are world-readable
#           inside the volume because the broker drops privileges to an unprivileged user; this is acceptable for
#           development only.
# Devices: copy ca.crt out of the mqtt-certs volume and install it as the trusted CA on the ESP32 / phone.
# =============================================================================
set -eu

CERTS=/certs

if [ -f "$CERTS/ca.crt" ] && [ -f "$CERTS/server.crt" ] && [ -f "$CERTS/server.key" ]; then
  echo "Dev certificates already exist - nothing to do."
  exit 0
fi

cd "$CERTS"

openssl genrsa -out ca.key 4096
openssl req -x509 -new -nodes -key ca.key -sha256 -days 825 -subj "/CN=CardioSense Dev CA" -out ca.crt

openssl genrsa -out server.key 2048
openssl req -new -key server.key -subj "/CN=mosquitto" -out server.csr
printf "subjectAltName=DNS:mosquitto,DNS:localhost,IP:127.0.0.1\nextendedKeyUsage=serverAuth\n" > server.ext
openssl x509 -req -in server.csr -CA ca.crt -CAkey ca.key -CAcreateserial -out server.crt -days 825 -sha256 -extfile server.ext

rm -f ca.key ca.srl server.csr server.ext
chmod 0644 ca.crt server.crt server.key
echo "Dev certificates created."
