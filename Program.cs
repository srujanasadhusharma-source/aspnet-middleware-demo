using System.Net.Mail;
using MiddlewareDemo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Custom middleware: logs how long each request takes
app.UseMiddleware<RequestTimingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

// In-memory user store
var users = new List<User>
{
    new User { Id = 1, Name = "Asha Rao", Email = "asha@example.com", Age = 28 },
    new User { Id = 2, Name = "Ravi Kumar", Email = "ravi@example.com", Age = 34 }
};
var nextId = 3;
var sync = new object();

// READ all users
app.MapGet("/api/users", () =>
{
    lock (sync) { return Results.Ok(users.ToList()); }
})
.WithName("GetUsers");

// READ one user
app.MapGet("/api/users/{id:int}", (int id) =>
{
    lock (sync)
    {
        var user = users.FirstOrDefault(u => u.Id == id);
        return user is null ? Results.NotFound(new { message = $"User {id} not found." }) : Results.Ok(user);
    }
})
.WithName("GetUserById");

// CREATE a user (with validation)
app.MapPost("/api/users", (UserRequest req) =>
{
    var errors = Validate(req);
    if (errors.Count > 0) return Results.ValidationProblem(errors);

    lock (sync)
    {
        var user = new User { Id = nextId++, Name = req.Name!.Trim(), Email = req.Email!.Trim(), Age = req.Age!.Value };
        users.Add(user);
        return Results.Created($"/api/users/{user.Id}", user);
    }
})
.WithName("CreateUser");

// UPDATE a user (with validation)
app.MapPut("/api/users/{id:int}", (int id, UserRequest req) =>
{
    var errors = Validate(req);
    if (errors.Count > 0) return Results.ValidationProblem(errors);

    lock (sync)
    {
        var user = users.FirstOrDefault(u => u.Id == id);
        if (user is null) return Results.NotFound(new { message = $"User {id} not found." });

        user.Name = req.Name!.Trim();
        user.Email = req.Email!.Trim();
        user.Age = req.Age!.Value;
        return Results.Ok(user);
    }
})
.WithName("UpdateUser");

// DELETE a user
app.MapDelete("/api/users/{id:int}", (int id) =>
{
    lock (sync)
    {
        var user = users.FirstOrDefault(u => u.Id == id);
        if (user is null) return Results.NotFound(new { message = $"User {id} not found." });

        users.Remove(user);
        return Results.NoContent();
    }
})
.WithName("DeleteUser");

app.Run();

// Validation: only valid user data is processed
static Dictionary<string, string[]> Validate(UserRequest req)
{
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(req.Name))
        errors["name"] = new[] { "Name is required." };
    else if (req.Name.Trim().Length < 2 || req.Name.Trim().Length > 50)
        errors["name"] = new[] { "Name must be between 2 and 50 characters." };

    if (string.IsNullOrWhiteSpace(req.Email) || !MailAddress.TryCreate(req.Email, out _))
        errors["email"] = new[] { "A valid email address is required." };

    if (req.Age is null || req.Age < 0 || req.Age > 120)
        errors["age"] = new[] { "Age must be a number between 0 and 120." };

    return errors;
}
