using TaskManagementAPI.Infrastructure;
using TaskManagementAPI;

var builder = WebApplication.CreateBuilder(args);

// Add AWS service configurations
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());

// Add Cognito Identity services
builder.Services.AddCognitoIdentity();

// Add custom infrastructure services
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection"));

// Add controllers
builder.Services.AddControllers();

// Configure authentication
builder.Services.AddCognitoAuthentication(builder.Configuration);

var app = builder.Build();

// Configure middleware
app.UseHttpsRedirection();

app.UseAuthentication(); // Must be added before UseAuthorization()
app.UseAuthorization();

app.MapControllers();
app.Run();