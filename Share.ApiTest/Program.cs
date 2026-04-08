using Shared.Database.MongoDb;

var builder = WebApplication.CreateBuilder(args);
string neo4jDbUri = builder.Configuration["NEO4JDB_URI"];
string neo4jUsername = builder.Configuration["NEO4JDB_USERNAME"];
string neo4jPassword = builder.Configuration["NEO4JDB_PASSWORD"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
// Add services to the container.
builder.Services.UseDatabaseNeo4j(neo4jDbUri, neo4jUsername, neo4jPassword);
builder.Services.AddControllers();
builder.Services.AddRedis(option =>
{
    option.ConnectionString = "localhost:6379";
    option.InstanceName = "tradeapp:";
});
var app = builder.Build();
app.UseCors("AllowAll");
// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
