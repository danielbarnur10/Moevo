using TaskManagementAPI;
using TaskManagementAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// builder.Configuration.AddJsonFile("./appsettings.json", optional: false, reloadOnChange: true);

// Add AWS service configurations
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Load AWS options from configuration
var awsOptions = builder.Configuration.GetAWSOptions();
Console.WriteLine($"Region from AWSOptions: {awsOptions.Region}"); // Debug line

builder.Services.AddDefaultAWSOptions(awsOptions);
// Add Cognito Identity services
builder.Services.AddCognitoIdentity();

// Add custom infrastructure services
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddInfrastructure(defaultConnection);

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
