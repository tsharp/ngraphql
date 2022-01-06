var builder = WebApplication.CreateBuilder(args);
builder.Services
  .AddGraphQL()
  .AllowGetRequests(true);

var app = builder.Build();

if (builder.Environment.IsDevelopment()) 
{
  app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapGraphQL());
app.UseGraphQLGraphiQL("/");

app.Run();
