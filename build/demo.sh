#!/usr/bin/env bash
# SGT demo ortamı (Spec 012): sahte ama gerçekçi veriyle dolu casino, tek komut.
#
#   build/demo.sh up            PostgreSQL + Keycloak + API'yi açar, demo kullanıcılarını, migration'ı ve demo verisini yükler
#   build/demo.sh down          kapatır (veri kalır)
#   build/demo.sh reset --yes   TÜM VERİYİ SİLER, yeniden kurar ve yükler
#
# deploy/.env yoksa YALNIZCA DEMO için rastgele şifre ve anahtarlarla üretilir (SGT_DEMO_ENV=1 işaretiyle; repoya girmez).
# Bu işareti taşımayan bir .env (pilot/geliştirme) ile up/reset çalışmaz.
# Gereken: Docker (compose v2), openssl, curl. Ayrıntı: docs/kurulum.md "Demo ortamı".
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
DEPLOY="$ROOT/deploy"
ENV_FILE="$DEPLOY/.env"
DEMO_USERS="$DEPLOY/keycloak/demo-users.json"

compose() {
  docker compose -f "$DEPLOY/docker-compose.yml" --env-file "$ENV_FILE" --profile api "$@"
}

# Dosyadaki bir değişkeni değiştirir. Değer awk'a ENVIRON ile geçer (komut satırında görünmez; "/", "+", "=" içerebilir).
set_var() {
  local file="$1" tmp
  tmp="$(mktemp "$file.XXXXXX")"
  K="$2" V="$3" awk 'index($0, ENVIRON["K"] "=") == 1 { print ENVIRON["K"] "=" ENVIRON["V"]; next } { print }' "$file" > "$tmp"
  chmod 600 "$tmp"
  mv -f "$tmp" "$file"
}

get_var() {
  grep -E "^$1=" "$ENV_FILE" | tail -n 1 | cut -d= -f2-
}

# Keycloak şifre kuralı: en az 12 karakter, büyük + küçük harf, rakam.
demo_password() {
  printf 'Demo-%s-9x' "$(openssl rand -hex 8)"
}

# .env geçici dosyada kurulur ve tek adımda yerine taşınır; yarıda kalırsa geçici dosya silinir.
create_env() {
  echo "deploy/.env yok; demo değerleriyle üretiliyor (yalnızca demo, gerçek kurulumda kullanmayın)."
  local tmp
  tmp="$(mktemp "$DEPLOY/.env.XXXXXX")"
  # shellcheck disable=SC2064 # yol şimdi sabitlenir
  trap "rm -f '$tmp'" EXIT
  chmod 600 "$tmp"
  cp "$DEPLOY/.env.example" "$tmp"
  set_var "$tmp" POSTGRES_PASSWORD "$(openssl rand -hex 16)"
  set_var "$tmp" KEYCLOAK_DB_PASSWORD "$(openssl rand -hex 16)"
  set_var "$tmp" KC_BOOTSTRAP_ADMIN_PASSWORD "$(demo_password)"
  set_var "$tmp" SGT_DEV_SUPERADMIN_PASSWORD "$(demo_password)"
  set_var "$tmp" SGT_DEV_KASIYER1_PASSWORD "$(demo_password)"
  set_var "$tmp" SGT_DEMO_PASSWORD "$(demo_password)"
  set_var "$tmp" SGT_PERSONAL_DATA_KEY "$(openssl rand -base64 32)"
  set_var "$tmp" SGT_BLIND_INDEX_KEY "$(openssl rand -base64 32)"
  # Demo: OTP yalnızca kurmuş kullanıcıya sorulur; komut satırından token alınabilir.
  set_var "$tmp" SGT_OTP_REQUIREMENT CONDITIONAL
  set_var "$tmp" SGT_DEV_DIRECT_GRANT true
  set_var "$tmp" ASPNETCORE_ENVIRONMENT Development
  printf '\n# build/demo.sh tarafından üretildi; yalnızca demo.\nSGT_DEMO_ENV=1\n' >> "$tmp"
  mv -f "$tmp" "$ENV_FILE"
  trap - EXIT
}

require_demo_env() {
  if [[ "$(get_var SGT_DEMO_ENV)" != "1" ]]; then
    echo "deploy/.env demo için üretilmemiş (SGT_DEMO_ENV=1 yok). Pilot/geliştirme ortamında demo çalıştırılmaz." >&2
    echo "Demo için mevcut .env'i başka yere taşıyıp tekrar deneyin; demo.sh yenisini üretir." >&2
    exit 1
  fi
}

# Pilot realm'inde olmayan demo kullanıcıları (mudur, pitboss, slotmudur, host) Keycloak'a eklenir; varsa atlanır.
import_demo_users() {
  # Şablon (şifre + tenant) bellekte doldurulup doğrudan konteynere aktarılır; diskte şifreli kopya kalmaz.
  # Değerler awk'a ENVIRON ile geçer (demo şifresi ve GUID; gsub için özel karakter içermez).
  # Yönetici bilgisi konteynere ortam değişkeniyle geçer; kcadm yapılandırması konteyner içinde, trap ile silinir.
  # shellcheck disable=SC2016 # değişkenler bilerek konteyner içinde genişler
  P="$(get_var SGT_DEMO_PASSWORD)" T="$(get_var SGT_TENANT_ID)" \
    awk '{ gsub(/__SGT_DEMO_PASSWORD__/, ENVIRON["P"]); gsub(/__SGT_TENANT_ID__/, ENVIRON["T"]); print }' "$DEMO_USERS" |
    KC_USER="$(get_var KC_BOOTSTRAP_ADMIN_USERNAME)" KC_PASS="$(get_var KC_BOOTSTRAP_ADMIN_PASSWORD)" \
      compose exec -T -e KC_USER -e KC_PASS keycloak bash -c '
        set -euo pipefail
        cfg="$(mktemp)"
        trap "rm -f \"$cfg\"" EXIT
        /opt/keycloak/bin/kcadm.sh config credentials --config "$cfg" --server http://localhost:8080 --realm master \
          --user "$KC_USER" --password "$KC_PASS" > /dev/null
        /opt/keycloak/bin/kcadm.sh create partialImport --config "$cfg" -r sgt -f - > /dev/null'
}

wait_for_api() {
  local url="$1" _
  for _ in $(seq 1 60); do
    if curl -fsS "$url" > /dev/null 2>&1; then
      return 0
    fi
    sleep 2
  done
  echo "API $url adresinde yanıt vermedi. Loglar: docker compose -f deploy/docker-compose.yml logs api" >&2
  return 1
}

print_summary() {
  local api_port kc_port
  api_port="$(get_var API_PORT)"
  kc_port="$(get_var KEYCLOAK_PORT)"
  cat <<EOF

SGT demo ortamı hazır.
  API       : http://localhost:${api_port:-5080}  (sağlık: /health)
  Keycloak  : http://localhost:${kc_port:-8080}  (yönetim: /admin, kullanıcı: KC_BOOTSTRAP_ADMIN_USERNAME)
  Web paneli: cd clients/web && npm run dev  ->  http://localhost:3000

Demo kullanıcıları (şifreler deploy/.env içinde):
  superadmin  Süper admin      SGT_DEV_SUPERADMIN_PASSWORD
  kasiyer1    Kasiyer          SGT_DEV_KASIYER1_PASSWORD
  mudur       Casino müdürü    SGT_DEMO_PASSWORD
  pitboss     Pit boss         SGT_DEMO_PASSWORD
  slotmudur   Slot müdürü      SGT_DEMO_PASSWORD
  host        Host             SGT_DEMO_PASSWORD
EOF
}

up() {
  [[ -f "$ENV_FILE" ]] || create_env
  require_demo_env
  compose build api
  compose up -d --wait postgres keycloak
  import_demo_users
  # Migration + demo verisi; tamamlanmışsa hiçbir şey yazmaz (Spec 012 KK5). Yalnızca bu komut demo yüklemeyi açar.
  compose run --rm -e DemoSeed__Enabled=true api --seed-demo
  compose up -d api
  wait_for_api "http://localhost:$(get_var API_PORT)/health"
  print_summary
}

down() {
  compose down
}

reset() {
  if [[ "${1:-}" != "--yes" ]]; then
    echo "reset TÜM demo verisini siler. Onaylamak için: build/demo.sh reset --yes" >&2
    exit 1
  fi
  [[ -f "$ENV_FILE" ]] || create_env
  require_demo_env
  compose down -v
  up
}

case "${1:-}" in
  up) up ;;
  down) down ;;
  reset) reset "${2:-}" ;;
  *)
    echo "Kullanım: build/demo.sh up | down | reset --yes" >&2
    exit 64
    ;;
esac
