using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Users.RegisterUser;

/// <summary>Personel kaydı (Spec 003 KK1–KK2). <paramref name="UserId"/> Keycloak'taki kimliktir (sub).</summary>
public sealed record RegisterUserCommand(TenantId TenantId, UserId UserId, string DisplayName, string EmployeeCode, AuditContext Audit);

public sealed class RegisterUserHandler(
    IUserAccessRepository users,
    IIdentityUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RegisterUserCommand>
{
    public async Task<Result> Handle(RegisterUserCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var user = UserAccess.Register(command.UserId, command.TenantId, command.DisplayName, command.EmployeeCode, command.Audit, clock.UtcNow);
        if (user.IsFailure)
        {
            return user;
        }

        if (await users.Get(command.TenantId, command.UserId, ct) is not null)
        {
            return IdentityErrors.UserAlreadyExists;
        }

        if (await users.EmployeeCodeExists(command.TenantId, user.Value.EmployeeCode, ct))
        {
            return IdentityErrors.EmployeeCodeTaken;
        }

        users.Add(user.Value);
        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
