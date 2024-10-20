using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDataContext>();
var app = builder.Build();

// Home Route - Test if the application is running
app.MapGet("/", () => "API de Lista de Tarefas");

// Create Users
app.MapPost("/api/users/register", ([FromBody] User user, [FromServices] AppDataContext ctx) => 
{
    ctx.Users.Add(user);
    ctx.SaveChanges();
    return Results.Created("", user);
});

// List users
app.MapGet("/api/users/list", ([FromServices] AppDataContext ctx) => 
{   
    List<User> users = ctx.Users.ToList();

    if (users.Count <= 0) {
        return Results.NotFound("Não há nenhum usuário cadastrado");
    }

    return Results.Ok(users);
});

// Delete User
app.MapDelete("/api/users/delete/{id}", ([FromServices] AppDataContext ctx, int id) => {

    User? user = ctx.Users.Find(id);

    if (user == null) {
        return Results.NotFound();
    }

    ctx.Users.Remove(user);
    ctx.SaveChanges();
    return Results.Ok("Usuário excluído");
});

// Create tasks
app.MapPost("/api/tasks/create", ([FromBody] api.Models.Task task, [FromServices] AppDataContext ctx) => 
{
    if (task.UserId <= 0) return Results.BadRequest("Insira um Id de usuário válido");

    User? user = ctx.Users.Find(task.UserId);

    if (user == null) return Results.NotFound("Usuário não encontrado, verifique o Id");

    ctx.Tasks.Add(task);
    ctx.SaveChanges();

    return Results.Created("/api/tasks/list", task);
});

// List Tasks
app.MapGet("/api/tasks/list", ([FromServices] AppDataContext ctx) => 
{   
    List<api.Models.Task> tasks = ctx.Tasks.ToList();

    if (tasks.Count <= 0) {
        return Results.NotFound("Não há nenhuma tarefa criada");
    }

    return Results.Ok(tasks);
});

// Delete Task
app.MapDelete("/api/tasks/delete/{id}", ([FromServices] AppDataContext ctx, int id) => {

    api.Models.Task? task = ctx.Tasks.Find(id);

    if (task == null) {
        return Results.NotFound();
    }

    ctx.Tasks.Remove(task);
    ctx.SaveChanges();
    return Results.Ok("Tarefa excluída");
});

app.Run();
