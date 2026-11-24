using Sgt.BuildingBlocks;

namespace Sgt.Modules.Identity.Application.Ports;

/// <summary>Identity modülünün unit of work'ü. Her modül kendi arayüzünü tanımlar; DI kayıtları çakışmaz.</summary>
public interface IIdentityUnitOfWork : IUnitOfWork
{
}
