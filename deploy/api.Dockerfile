# SGT API imajı. Repo kökünden derlenir:
#   docker build -f deploy/api.Dockerfile -t sgt-api:dev .
# Hariç tutulan dosyalar: deploy/api.Dockerfile.dockerignore

# --- 1. aşama: derleme (.NET 10 SDK) ---
FROM mcr.microsoft.com/dotnet/sdk:10.0.403-noble AS build
WORKDIR /repo

# Derleme kuralları (uyarı = hata, kod stili, yasaklı API listesi) repo kökündeki bu dosyalardadır.
COPY Directory.Build.props .editorconfig ./
COPY build/BannedSymbols.txt build/
COPY src/ src/

RUN dotnet publish src/Bootstrapper/Sgt.Api/Sgt.Api.csproj \
        --configuration Release \
        --output /app \
        -p:UseAppHost=false

# Geliştirme anahtarları imaja girmez; tüm ayarlar ortam değişkeni / secret manager ile verilir.
RUN rm -f /app/appsettings.Development.json

# --- 2. aşama: çalıştırma (ASP.NET Core 10 runtime) ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0.14-noble AS runtime
WORKDIR /app
COPY --from=build /app ./

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080

# Root olmayan kullanıcı (imajda hazır gelen "app", UID 1654).
USER $APP_UID

ENTRYPOINT ["dotnet", "Sgt.Api.dll"]
