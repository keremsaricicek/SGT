namespace Sgt.BuildingBlocks;

/// <summary>Durum değiştiren tek bir işlev.</summary>
public interface ICommandHandler<in TCommand>
{
    Task<Result> Handle(TCommand command, CancellationToken ct);
}

/// <summary>Durum değiştiren ve sonuç değeri döndüren tek bir işlev.</summary>
public interface ICommandHandler<in TCommand, TResult>
{
    Task<Result<TResult>> Handle(TCommand command, CancellationToken ct);
}

/// <summary>Yalnızca okuyan tek bir işlev.</summary>
public interface IQueryHandler<in TQuery, TResult>
{
    Task<TResult> Handle(TQuery query, CancellationToken ct);
}

/// <summary>Bir işlevdeki tüm değişiklikleri tek transaction'da kaydeder.</summary>
public interface IUnitOfWork
{
    Task Commit(CancellationToken ct);
}
