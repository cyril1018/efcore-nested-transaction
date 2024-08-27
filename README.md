# EFCore Nested Transaction
EFCore Nested Transaction enables nested transactions in Entity Framework Core. With this class, you can call "BEGIN TRANSACTION" and "COMMIT TRANSACTION" commands in a nested manner within EFCore. The actual "BEGIN TRANSACTION" and "COMMIT TRANSACTION" commands are only sent to the database server in the outermost calls. This implementation currently supports Microsoft SQL Server only.
## How to Use
To use EFCore Nested Transaction, you need to replace the default IDbContextTransactionManager in your Startup.cs file.
```csharp
services.AddDbContext <AppDbContext> (options =>
{
    options.ReplaceService <IDbContextTransactionManager, NestedTransactionManager>();
});
```
