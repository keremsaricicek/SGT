using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Sgt.BuildingBlocks.Web;

/// <summary>Kayıt oluşturan uçların 201 gövdesi: <c>{"id": "..."}</c>.</summary>
public sealed record CreatedId(Guid Id);

/// <summary>
/// Uçların OpenAPI yanıt şemalarını bildirir (ADR-0006). Davranışı değiştirmez; yalnızca sözleşmeye yazılır.
/// </summary>
public static class ApiResponseMetadata
{
    /// <summary>Ekran yetkisi isteyen okuma ucu: yetki yoksa 403.</summary>
    public static RouteHandlerBuilder ProducesQueryProblems(this RouteHandlerBuilder builder) =>
        builder.ProducesProblem(StatusCodes.Status403Forbidden);

    /// <summary>Gerekçe kodlu değişiklik ucu: doğrulama 400, yetki 403, kayıt yok 404, çakışma 409.</summary>
    public static RouteHandlerBuilder ProducesCommandProblems(this RouteHandlerBuilder builder) =>
        builder
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    /// <summary>Gövdesiz başarılı değişiklik (204) ve olası hatalar.</summary>
    public static RouteHandlerBuilder ProducesNoContentCommand(this RouteHandlerBuilder builder) =>
        builder.Produces(StatusCodes.Status204NoContent).ProducesCommandProblems();

    /// <summary>Kayıt oluşturan değişiklik (201, <see cref="CreatedId"/>) ve olası hatalar.</summary>
    public static RouteHandlerBuilder ProducesCreatedCommand(this RouteHandlerBuilder builder) =>
        builder.Produces<CreatedId>(StatusCodes.Status201Created).ProducesCommandProblems();
}
