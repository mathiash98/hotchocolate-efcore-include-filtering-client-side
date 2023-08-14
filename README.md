# hotchocolate-efcore-include-filtering-client-side

POC Filtering on nested array leads to client side evaluation

## Reproduce

1. Run SqlServer locally

```sh
# ARM processor like Mac M1
docker rm -f sqlserver; docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=password' -p 1433:1433 -d --name sqlserver mcr.microsoft.com/azure-sql-edge
```

2. Run the dotnet project
3. Wait for seed of db to finish (change parameters in `AppContext.cs`), currently 100 persons with 1000 personActions each will be added.
4. Open the graphQL endpoint in browser
5. Run the following query where we use filtering and ordering on person.actions which is a nested array of persons.

```graphql
query {
  persons {
    fullName
    actions(first: 1, order: { dateTimeUtc: DESC }) {
      nodes {
        action
        dateTimeUtc
      }
    }
  }
}
```

6. Observe in the logs that the following query is executed. Which shows that the pagination and ordering on person.actions is NOT performed on the database, but in client side. Observe also that the run time is around 2-3 seconds.

```sql
SELECT [p].[Id], [p].[BoardingState], [p].[DeletedUtcDateTime], [p].[FullName], [p].[LastUpdatedAt], [t].[Id], [t].[Action], [t].[DateTimeUtc], [t].[DeletedUtcDateTime], [t].[LastUpdatedAt], [t].[Location], [t].[PersonId]
      FROM [Persons] AS [p]
      LEFT JOIN (
          SELECT [p0].[Id], [p0].[Action], [p0].[DateTimeUtc], [p0].[DeletedUtcDateTime], [p0].[LastUpdatedAt], [p0].[Location], [p0].[PersonId]
          FROM [PersonActions] AS [p0]
          WHERE [p0].[DeletedUtcDateTime] IS NULL
      ) AS [t] ON [p].[Id] = [t].[PersonId]
      WHERE [p].[DeletedUtcDateTime] IS NULL
      ORDER BY [p].[Id]
```

## Equivalent EF Core valid query

See the GraphQL endpoint `PersonWithValidEfCoreQuery` which has a run time of 655ms, while the `person` endpoint has a run time of around 2s.

This endpoint uses valid EF Core filtering on include which was introduced in EF Core 5 https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager#filtered-include. And the resulting SQL Query is:

```sql
SELECT [p].[Id], [p].[BoardingState], [p].[DeletedUtcDateTime], [p].[FullName], [p].[LastUpdatedAt], [t0].[Id], [t0].[Action], [t0].[DateTimeUtc], [t0].[DeletedUtcDateTime], [t0].[LastUpdatedAt], [t0].[Location], [t0].[PersonId]
    FROM [Persons] AS [p]
    LEFT JOIN (
        SELECT [t].[Id], [t].[Action], [t].[DateTimeUtc], [t].[DeletedUtcDateTime], [t].[LastUpdatedAt], [t].[Location], [t].[PersonId]
        FROM (
            SELECT [p0].[Id], [p0].[Action], [p0].[DateTimeUtc], [p0].[DeletedUtcDateTime], [p0].[LastUpdatedAt], [p0].[Location], [p0].[PersonId], ROW_NUMBER() OVER(PARTITION BY [p0].[PersonId] ORDER BY [p0].[DateTimeUtc] DESC) AS [row]
            FROM [PersonActions] AS [p0]
            WHERE [p0].[DeletedUtcDateTime] IS NULL
        ) AS [t]
        WHERE [t].[row] <= 1
    ) AS [t0] ON [p].[Id] = [t0].[PersonId]
    WHERE [p].[DeletedUtcDateTime] IS NULL
    ORDER BY [p].[Id], [t0].[PersonId], [t0].[DateTimeUtc] DESC
```
