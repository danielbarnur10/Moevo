using TaskManagementAPI;
using TaskManagementAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add AWS service configurations
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());

// Add Cognito Identity services
builder.Services.AddCognitoIdentity();

// Add custom infrastructure services
#pragma warning disable CS8604 // Possible null reference argument.
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection"));
#pragma warning restore CS8604 // Possible null reference argument.

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
