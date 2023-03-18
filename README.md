# efcore-nested-transaction
You can call "begin | commit tran" nestedly with EFCore, "begin | commit tran" commands only pass to database servers in outmost calls.
Implement Microsoft SQL Server only.
