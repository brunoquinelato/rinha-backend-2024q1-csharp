using Endpoints.Balance;
using Endpoints.Transaction;
using Helpers;
using Npgsql;
using Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<NpgsqlConnection>((sp) => new NpgsqlConnection(builder.Configuration.GetConnectionString("DbConnection")));
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<TransactionRepository>();
builder.Services.AddScoped<BalanceHandler>();
builder.Services.AddScoped<TransactionHandler>();
builder.Services.AddMemoryCache();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.TypeInfoResolverChain.Insert(0, ApiJsonContext.Default));

var app = builder.Build();
app.MapGet("/", () => "Api Rinha Backend");
app.RegisterBalanceEndpoints();
app.RegisterTransactionEndpoints();

app.Run();