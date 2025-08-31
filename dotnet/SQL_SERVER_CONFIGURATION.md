# SQL Server Connection String Examples

This application has been migrated to use Microsoft SQL Server. Below are sample connection strings for different scenarios:

## LocalDB (Development on Windows)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DV8_SS_SD_WAN;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

## SQL Server Express (Local instance)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=DV8_SS_SD_WAN;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

## SQL Server with Windows Authentication
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server-name;Database=DV8_SS_SD_WAN;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

## SQL Server with SQL Authentication
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server-name;Database=DV8_SS_SD_WAN;User Id=your-username;Password=your-password;MultipleActiveResultSets=true"
  }
}
```

## Azure SQL Database
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Database=DV8_SS_SD_WAN;User Id=your-username;Password=your-password;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;MultipleActiveResultSets=true"
  }
}
```

## Docker SQL Server
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=DV8_SS_SD_WAN;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true"
  }
}
```

## Testing with In-Memory Database
For testing, the application automatically uses Entity Framework's in-memory database provider, so no SQL Server is required for running tests.

## Migration Notes
- The application was successfully migrated from SQLite to SQL Server
- All Entity Framework models and relationships are preserved
- The database schema will be automatically created using Entity Framework migrations
- Tests use in-memory database provider for isolation and speed