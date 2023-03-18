public class NestedTransactionManager : IDbContextTransactionManager
{
    readonly ISqlServerConnection _sqlServerConnection;
    public NestedTransactionManager(ISqlServerConnection sqlServerConnection)
    {
        // Dependency inject ISqlServerConnection, ISqlServerConnection is original IDbContextTransactionManager in EF Core 3.1 .
        _sqlServerConnection = sqlServerConnection;
    }
    int Layer = 0;

    public IDbContextTransaction CurrentTransaction => _sqlServerConnection.CurrentTransaction;

    public IDbContextTransaction BeginTransaction()
    {
        if (Layer++ == 0)
            _sqlServerConnection.BeginTransaction();
        return new NestedTransation(this, Layer);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Layer++ == 0)
            await _sqlServerConnection.BeginTransactionAsync(cancellationToken);
        return new NestedTransation(this, Layer);
    }

    public void CommitTransaction()
    {
        if (Layer-- <= 1)
            _sqlServerConnection.CommitTransaction();
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    => Layer-- <= 1
    ? _sqlServerConnection.CurrentTransaction.CommitAsync(cancellationToken)
    : Task.CompletedTask;

    public void ResetState() => _sqlServerConnection.ResetState();

    public Task ResetStateAsync(CancellationToken cancellationToken = default) => _sqlServerConnection.ResetStateAsync(cancellationToken);

    public void RollbackTransaction() => _sqlServerConnection.RollbackTransaction();

    class NestedTransation : IDbContextTransaction
    {
        readonly NestedTransactionManager _manager;
        readonly int _layer;
        public NestedTransation(NestedTransactionManager manager, int layer)
        {
            _manager = manager;
            _layer = layer;
        }

        bool Commited => _layer > _manager.Layer;

        public Guid TransactionId => Transaction.TransactionId;

        IDbContextTransaction Transaction => _manager.CurrentTransaction;

        public void Commit() => _manager.CommitTransaction();

        public Task CommitAsync(CancellationToken cancellationToken = default) => _manager.CommitTransactionAsync(cancellationToken);

        public void Dispose()
        {
            if (!Commited && Transaction != null)
                Transaction.Dispose();
        }
        public ValueTask DisposeAsync()
        => !Commited && Transaction != null
        ? Transaction.DisposeAsync()
        : default;

        public void Rollback() => Transaction.Rollback();

        public Task RollbackAsync(CancellationToken cancellationToken = default) => Transaction.RollbackAsync(cancellationToken);
    }
}
