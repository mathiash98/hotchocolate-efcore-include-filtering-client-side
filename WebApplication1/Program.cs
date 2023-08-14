using Microsoft.EntityFrameworkCore;
using WebApplication1;
using AppContext = WebApplication1.AppContext;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppContext>(options => options.UseSqlServer(
    builder.Configuration.GetSection("AzureSqlDb").GetValue<string>("ConnectionString"),
    ServiceProviderOptions => ServiceProviderOptions.EnableRetryOnFailure()));

builder.Services.AddMemoryCache(); // InMemory Cache service used by GraphQL to automatically persist queries, can be used for all kinds of thingsl

// Injection of GraphQL related services.
builder.Services.AddGraphQLServer()
    .RegisterDbContext<AppContext>()
    .AddQueryType<GraphQLQuery>()
    .AddProjections()
    .AddFiltering(x => x
        .AddDefaults()
    )
    .AddSorting()
    .UseAutomaticPersistedQueryPipeline()
    .AddInMemoryQueryStorage();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGraphQL();

MigrateAndSeedData(app);

app.Run();



static void MigrateAndSeedData(IApplicationBuilder app)
{
    try
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<AppContext>();
        context.Database
            .SetCommandTimeout(
                1200); // Increase CommandTimeout for migrations as some migrations might take a long time
        context.MigrateAndSeed();
    }
    catch (Exception)
    {
        Console.WriteLine("Failed to migrate and seed database!");
        throw;
    }
}