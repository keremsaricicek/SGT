#!/bin/sh
# Yalnızca PostgreSQL veri klasörü İLK KEZ oluşturulurken çalışır.
# Keycloak için ayrı kullanıcı ve veritabanı açar; SGT verisi ile Keycloak verisi ayrı tutulur.
# Şifre ortam değişkeninden gelir (deploy/.env); bu dosyada şifre yoktur.
set -eu

: "${KEYCLOAK_DB_PASSWORD:?KEYCLOAK_DB_PASSWORD tanımlı değil (deploy/.env)}"

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" \
     -v kc_pw="$KEYCLOAK_DB_PASSWORD" <<'SQL'
CREATE ROLE keycloak LOGIN PASSWORD :'kc_pw';
CREATE DATABASE keycloak OWNER keycloak;
SQL
