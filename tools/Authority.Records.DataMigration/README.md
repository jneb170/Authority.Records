# Authority.Records.DataMigration

One-off utility that copies all business data from the production **SQL Server**
database into fresh **SQLite** database files (`app.db` + `auth.db`), so the app can
be switched to the SQLite provider without re-entering existing records.

It reuses the app's own EF Core model (`AppDbContext` / `AuthDbContext`) so all
provider quirks are handled exactly as the running app handles them:

- `RecordNumber` values are preserved (SQLite `HasDefaultValueSql` only fills blanks).
- `rowversion` → BLOB concurrency token; `datetime2` → TEXT.
- `Id`s are app-assigned Guids, copied as-is, so all foreign keys stay valid.

Read models are copied as-is (the running app's read-model rebuild is idempotent and
simply upserts). The transient `OutboxMessage` / `DeadLetterMessage` tables are skipped.
SQLite foreign keys are disabled during the copy so table order is irrelevant, and the
write-ahead log is checkpointed at the end so the output files are self-contained.

> Not part of `Authority.Records.sln` — CI does not build it. Run it manually.

## Usage

```
dotnet run --project tools/Authority.Records.DataMigration -- \
  --source "<SQL Server connection string>" \
  --out    "<output directory>"
```

The source connection string can also be supplied via the `SOURCE_SQLSERVER_CONNECTION`
environment variable instead of `--source`. The tool only **reads** the source; it
never modifies it. Output defaults to `./out`.

## Producing and deploying the production files

1. **Generate the files from production:**
   ```
   dotnet run --project tools/Authority.Records.DataMigration -- \
     --source "<AZURE_SQL_CONNECTION_STRING>" \
     --out    "C:/temp/prod-sqlite"
   ```
   Use the production Azure SQL connection string (the same value stored in the
   GitHub `AZURE_SQL_CONNECTION_STRING` secret / Azure App Service config). Confirm
   the printed per-table row counts look right.

2. **Upload before flipping the provider.** Upload `app.db` and `auth.db` to
   `D:\home\site\data\` on the Azure App Service via Kudu
   (`https://<app-name>.scm.azurewebsites.net` → Debug console → `site/data`).
   Do this **while the app is still on SQL Server**, so the directory is populated
   before the app ever boots on SQLite. Otherwise the app's boot-time
   `Database.MigrateAsync()` creates empty SQLite files first.

3. **Flip the provider** (see `.claude/session-state/active.md` for the full list):
   set `DefaultDatabaseProvider=Sqlite` plus the two `Sqlite*Connection` strings in
   Azure App Service config, set the GitHub `DEFAULT_DATABASE_PROVIDER=Sqlite`
   variable, then push `master`.

4. **On first SQLite boot**, `MigrateAsync` finds the uploaded files with a matching
   migration history, so it is a no-op against real data (rather than creating empty
   databases). The read-model rebuild repopulates projections; `DemoSeeder` adds the
   demo account idempotently.

The output files are self-contained (WAL checkpointed). If you ever copy them while a
process still holds them open, also copy any `-wal` / `-shm` sidecars alongside the
main file.
