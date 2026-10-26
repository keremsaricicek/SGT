# Plan 001: Yetki paneli

## Anayasa kontrolü
| İlke | Uyum |
|---|---|
| I SOLID/OOP | Kurallar `Role` ve `UserAccess` aggregate'lerinde; `PermissionCode` value object; handler yalnızca akış |
| II Modülerlik | `Sgt.Modules.Identity.*`; diğer modüller yalnızca `Identity.Contracts` (`IPermissionQuery`) kullanır |
| III Değer hareketi | Para yok. Değişiklik kayıtları append-only (KK6) |
| IV API-only/tenant | Her aggregate `TenantId` taşır; sorgular tenant kapsamlı |
| V Test-önce | Domain unit testleri önce; handler testleri sahte repository ile |
| VI Güvenlik | Gerçek veri yok; süper admin kontrolü sunucuda |
| VII İzlenebilirlik | `PermissionChange` kaydı: aktör, zaman, gerekçe kodu, correlation id |
| VIII Ekran yetkisi | Bu spec'in kendisi |

## Tasarım
- **Domain:** `PermissionCode` (ekran + eylem), `PermissionSet` (kurallar KK1–KK3, KK7), `Role`, `UserAccess`,
  `PermissionChange` (değişiklik kaydı), `ActorContext` (aktör, süper admin mi, gerekçe, correlation id).
- **Application:** `GrantRolePermission`, `RevokeRolePermission`, `GrantUserPermission`, `RevokeUserPermission`,
  `AssignRole`, `GetMyMenu`. Portlar: `IRoleRepository`, `IUserAccessRepository`, `IScreenCatalog`, `ILicensedProducts`.
- **Contracts:** `IPermissionQuery.HasPermission(tenant, user, code)` — diğer modüllerin sunucu tarafı kontrolü (KK10).
- **Infrastructure / Api:** veritabanı ADR'sinden sonra (görev T6–T7).

## Complexity Tracking
Yok.
