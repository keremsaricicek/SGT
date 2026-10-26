#!/bin/bash
# Yeni modül iskeleti: Contracts, Domain, Application, Infrastructure, Api + birim test projesi.
# Kullanım: build/new-module.sh <ModülAdı>   (ör. Configuration)
# Sonra: tests/Architecture/ArchitectureTests.cs içindeki ModuleCatalog'a satır ekleyin.
set -euo pipefail
NAME="${1:?Modül adı gerekli}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
B="src/Modules/$NAME"
BB="src/BuildingBlocks/Sgt.BuildingBlocks"

for L in Contracts Domain Application Infrastructure Api; do
  P="Sgt.Modules.$NAME.$L"
  dotnet new classlib -n "$P" -o "$B/$P" >/dev/null
  rm -f "$B/$P/Class1.cs"
  printf 'namespace %s;\n\n/// <summary>Mimari testlerin bu derlemeyi bulması için işaret tipi.</summary>\npublic static class AssemblyMarker\n{\n}\n' "$P" > "$B/$P/AssemblyMarker.cs"
done

ref() { dotnet add "$B/Sgt.Modules.$NAME.$1" reference "${@:2}" >/dev/null; }
ref Contracts "$BB"
ref Domain "$BB"
ref Application "$BB" "$B/Sgt.Modules.$NAME.Domain" "$B/Sgt.Modules.$NAME.Contracts"
ref Infrastructure "$B/Sgt.Modules.$NAME.Application" src/BuildingBlocks/Sgt.BuildingBlocks.Infrastructure
ref Api "$B/Sgt.Modules.$NAME.Application" "$B/Sgt.Modules.$NAME.Contracts" src/BuildingBlocks/Sgt.BuildingBlocks.Web

sed -i 's#<PropertyGroup>#<ItemGroup>\n    <FrameworkReference Include="Microsoft.AspNetCore.App" />\n  </ItemGroup>\n\n  <PropertyGroup>#' "$B/Sgt.Modules.$NAME.Api/Sgt.Modules.$NAME.Api.csproj"
dotnet add "$B/Sgt.Modules.$NAME.Infrastructure" package EFCore.NamingConventions --version 10.0.1 >/dev/null
dotnet add "$B/Sgt.Modules.$NAME.Infrastructure" package Microsoft.EntityFrameworkCore.Design --version 10.0.4 >/dev/null

T="tests/Modules/$NAME/Sgt.Modules.$NAME.Tests"
dotnet new xunit -n "Sgt.Modules.$NAME.Tests" -o "$T" >/dev/null
rm -f "$T/UnitTest1.cs"
sed -i '/coverlet.collector/d' "$T/Sgt.Modules.$NAME.Tests.csproj"
dotnet add "$T" reference "$B/Sgt.Modules.$NAME.Application" >/dev/null

for L in Contracts Domain Application Infrastructure Api; do
  dotnet add tests/Architecture/Sgt.Tests.Architecture.csproj reference "$B/Sgt.Modules.$NAME.$L/Sgt.Modules.$NAME.$L.csproj" >/dev/null
done
dotnet sln Sgt.slnx add "$B"/*/*.csproj "$T" >/dev/null
echo "Modül $NAME oluşturuldu. ModuleCatalog'a eklemeyi unutmayın."
