#!/usr/bin/env bash
# SGT demo ortamı (Spec 012): sahte ama gerçekçi veriyle dolu casino, tek komut.
#
#   build/demo.sh up      PostgreSQL + Keycloak + API'yi açar, migration ve demo verisini yükler
#   build/demo.sh down    kapatır (veri kalır)
#   build/demo.sh reset   tüm veriyi siler, yeniden kurar ve yükler
#
# deploy/.env yoksa YALNIZCA DEMO için rastgele şifre ve anahtarlarla üretilir (repoya girmez).
# Gereken: Docker (compose v2), openssl, curl. Ayrıntı: docs/kurulum.md "Demo ortamı".
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
DEPLOY="$ROOT/deploy"
ENV_FILE="$DEPLOY/.env"

compose() {
  docker compose -f "$DEPLOY/docker-compose.yml" --env-file "$ENV_FILE" --profile api "$@"
}

# .env içindeki bir değişkeni değiştirir (değer "/", "+", "=" içerebilir; sed yerine awk).
set_var() {
  local key="$1" value="$2" tmp
  tmp="$(mktemp)"
  awk -v k="$key" -v v="$value" 'index($0, k "=") == 1 { print k "=" v; next } { print }' "$ENV_FILE" > "$tmp"
  mv "$tmp" "$ENV_FILE"
}

get_var() {
  grep -E "^$1=" "$ENV_FILE" | tail -n 1 | cut -d= -f2-
}

# Keycloak şifre kuralı: en az 12 karakter, büyük + küçük harf, rakam.
demo_password() {
  printf 'Demo-%s-9x' "$(openssl rand -hex 8)"
}

create_env() {
  echo "deploy/.env yok; demo değerleriyle üretiliyor (yalnızca demo, gerçek kurulumda kullanmayın)."
  cp "$DEPLOY/.env.example" "$ENV_FILE"
  chmod 600 "$ENV_FILE"
  set_var POSTGRES_PASSWORD "$(openssl rand -hex 16)"
  set_var KEYCLOAK_DB_PASSWORD "$(openssl rand -hex 16)"
  set_var KC_BOOTSTRAP_ADMIN_PASSWORD "$(demo_password)"
  set_var SGT_DEV_SUPERADMIN_PASSWORD "$(demo_password)"
  set_var SGT_DEV_KASIYER1_PASSWORD "$(demo_password)"
  set_var SGT_DEMO_PASSWORD "$(demo_password)"
  set_var SGT_PERSONAL_DATA_KEY "$(openssl rand -base64 32)"
  set_var SGT_BLIND_INDEX_KEY "$(openssl rand -base64 32)"
  # Demo: OTP yalnızca kurmuş kullanıcıya sorulur; komut satırından token alınabilir.
  set_var SGT_OTP_REQUIREMENT CONDITIONAL
  set_var SGT_DEV_DIRECT_GRANT true
  set_var ASPNETCORE_ENVIRONMENT Development
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
  compose build api
  compose up -d --wait postgres keycloak
  # Migration + demo verisi; veri zaten varsa hiçbir şey yazmaz (Spec 012 KK5).
  compose run --rm api --seed-demo
  compose up -d api
  wait_for_api "http://localhost:$(get_var API_PORT)/health"
  print_summary
}

down() {
  compose down
}

reset() {
  compose down -v
  up
}

case "${1:-}" in
  up) up ;;
  down) down ;;
  reset) reset ;;
  *)
    echo "Kullanım: build/demo.sh up | down | reset" >&2
    exit 64
    ;;
esac
