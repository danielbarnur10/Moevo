using TaskManagementAPI;
using TaskManagementAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("./appsettings.json", optional: false, reloadOnChange: true);

// Add AWS service configurations
var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);

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
