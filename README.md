# Duende-IdentityServer

docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Passw0rd!" -p 1436:1433 --name sql -d mcr.microsoft.com/mssql/server:2019-latest
# Add-migration
1: Add-Migration InitialPersistedGrantMigration -c PersistedGrantDbContext -o Persistence/Migraions/IdentityServer/PersistedGrantDb 
2: Add-Migration InitialConfigurationMigration -c ConfigurationDbContext -o Persistence/Migraions/IdentityServer/ConfigurationDb
3: dotnet ef database update -c PersistedGrantDbContext
4: dotnet ef database update -c ConfigurationDbContext

# rund Docker-compose
1: docker-compose -f docker-compose.yml up -d --build