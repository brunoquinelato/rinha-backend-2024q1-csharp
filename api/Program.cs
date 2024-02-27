using Endpoints.Balance;
using Endpoints.Transaction;
using Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<TransactionRepository>();
builder.Services.AddScoped<BalanceHandler>();
builder.Services.AddScoped<TransactionHandler>();
builder.Services.AddMemoryCache();

var app = builder.Build();
app.MapGet("/", () => "Api Rinha Backend");
app.RegisterBalanceEndpoints();
app.RegisterTransactionEndpoints();

app.Run();