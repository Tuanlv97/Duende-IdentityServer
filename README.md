# Duende-IdentityServer

docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Passw0rd!" -p 1433:1433 --name sql -d mcr.microsoft.com/mssql/server:201-latest
# Add-migration
1: Add-Migration InitialPersistedGrantMigration -c PersistedGrantDbContext -o Persistence/Migraions/IdentityServer/PersistedGrantDb 
2: Add-Migration InitialConfigurationMigration -c ConfigurationDbContext -o Persistence/Migraions/IdentityServer/ConfigurationDb
3: dotnet ef database update -c PersistedGrantDbContext
4: dotnet ef database update -c ConfigurationDbContext