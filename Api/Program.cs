using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDataContext>();

builder.Services.AddCors(
    options => options.AddPolicy("Total Acess",
        configs => configs
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod())
);

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
app.MapGet("/api/users/list", async ([FromServices] AppDataContext ctx) =>
{
    var users = await ctx.Users.ToListAsync();

    if (users.Count <= 0)
    {
        return Results.NotFound("Não há nenhum usuário cadastrado");
    }

    return Results.Ok(users);
});

// Update User
app.MapPut("/api/users/update/{id}", async (int id, [FromBody] User updatedUser, [FromServices] AppDataContext ctx) =>
{
    var user = await ctx.Users.FindAsync(id);
    if (user is null) return Results.NotFound("Usuário não encontrado.");

    user.Name = updatedUser.Name ?? user.Name;
    user.Email = updatedUser.Email ?? user.Email;
    user.Password = updatedUser.Password ?? user.Password;

    await ctx.SaveChangesAsync();

    return Results.Ok("Usuário atualizado.");
});

// Delete User
app.MapDelete("/api/users/delete/{id}", ([FromServices] AppDataContext ctx, int id) =>
{

    User? user = ctx.Users.Find(id);

    if (user == null)
    {
        return Results.NotFound();
    }

    ctx.Users.Remove(user);
    ctx.SaveChanges();
    return Results.Ok("Usuário excluído");
});

// Create tasks
app.MapPost("/api/tasks/create", async ([FromBody] api.Models.Task task, [FromServices] AppDataContext ctx) =>
{
    User? user = await ctx.Users.FindAsync(task.User?.Id);
    Project? project = await ctx.Projects.FindAsync(task.Project?.Id);
    Tag? tag = await ctx.Tags.FindAsync(task.Tag?.Id);

    if (user == null) return Results.NotFound("Usuário não encontrado.");
    if (project == null) return Results.NotFound("Projeto não encontrado.");
    if (tag == null) return Results.NotFound("Tag não encontrada.");

    task.User = user;
    task.Project = project;
    task.Tag = tag;
    task.Status = "Não iniciada";

    ctx.Tasks.Add(task);
    await ctx.SaveChangesAsync();

    return Results.Created("/api/tasks/list", task);
});

// List Tasks
app.MapGet("/api/tasks/list", async ([FromServices] AppDataContext ctx) =>
{
    var tasks = await ctx.Tasks
        .Include(t => t.User)     // Incluir o User associado
        .Include(t => t.Tag)      // Incluir o Tag associado
        .Include(t => t.Project)  // Incluir o Project associado
        .ToListAsync();

    if (tasks.Count <= 0)
    {
        return Results.NotFound("Não há nenhuma tarefa criada");
    }

    return Results.Ok(tasks);
});

// Update Task
app.MapPut("/api/tasks/update/{id}", async (int id, [FromBody] api.Models.Task updatedTask, [FromServices] AppDataContext ctx) =>
{
    var task = await ctx.Tasks
        .Include(t => t.User)
        .Include(t => t.Project)
        .Include(t => t.Tag)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (task == null) return Results.NotFound("Tarefa não encontrada.");

    task.Title = updatedTask.Title ?? task.Title;
    task.Description = updatedTask.Description ?? task.Description;
    task.DueDate = updatedTask.DueDate ?? task.DueDate;
    task.Status = updatedTask.Status ?? task.Status;

    if (updatedTask.User != null && updatedTask.User.Id > 0)
    {
        var user = await ctx.Users.FindAsync(updatedTask.User.Id);
        if (user == null) return Results.BadRequest("Usuário não encontrado.");
        task.User = user;
    }

    if (updatedTask.Project != null && updatedTask.Project.Id > 0)
    {
        var project = await ctx.Projects.FindAsync(updatedTask.Project.Id);
        if (project == null) return Results.BadRequest("Projeto não encontrado.");
        task.Project = project;
    }

    if (updatedTask.Tag != null && updatedTask.Tag.Id > 0)
    {
        var tag = await ctx.Tags.FindAsync(updatedTask.Tag.Id);
        if (tag == null) return Results.BadRequest("Tag não encontrada.");
        task.Tag = tag;
    }

    await ctx.SaveChangesAsync();

    return Results.Ok(task);
});


// Delete Task
app.MapDelete("/api/tasks/delete/{id}", async (int id, [FromServices] AppDataContext ctx) =>
{
    var task = await ctx.Tasks.FindAsync(id);

    if (task == null) return Results.NotFound("Tarefa não encontrada.");

    ctx.Tasks.Remove(task);
    await ctx.SaveChangesAsync();

    return Results.Ok("Tarefa excluída");
});

// Create Tag
app.MapPost("/api/tags/create", ([FromBody] Tag tag, [FromServices] AppDataContext ctx) =>
{

    if (string.IsNullOrEmpty(tag.Name))
        return Results.BadRequest("O nome da tag não pode ser vazio.");


    ctx.Tags.Add(tag);
    ctx.SaveChanges();

    return Results.Created($"/api/tags/{tag.Id}", tag);
});

// Listar todas as Tags
app.MapGet("/api/tags/list", async (AppDataContext ctx) =>
{
    var tags = await ctx.Tags.ToListAsync();

    if (tags.Count <= 0)
    {
        return Results.NotFound("Não há nenhuma tag criada");
    }
    return Results.Ok(tags);
});


// Update Tag
app.MapPut("/api/tags/update/{id}", async (int id, [FromBody] Tag updatedTag, [FromServices] AppDataContext ctx) =>
{

    var tag = await ctx.Tags.FindAsync(id);
    if (tag is null) return Results.NotFound("Tag não encontrada.");


    tag.Name = updatedTag.Name ?? tag.Name;
    tag.Color = updatedTag.Color ?? tag.Color;
    tag.Priority = updatedTag.Priority ?? tag.Priority;

    await ctx.SaveChangesAsync();

    return Results.Ok("tag atualizada."); // Retorna a tag atualizada
});


// Delete Tag
app.MapDelete("/api/tags/delete/{id}", async (int id, [FromServices] AppDataContext ctx) =>
{

    var tag = await ctx.Tags.FindAsync(id);
    if (tag is null) return Results.NotFound("Tag não encontrada.");

    ctx.Tags.Remove(tag);
    await ctx.SaveChangesAsync();

    return Results.Ok("Tag excluída");
});


// Creat Project 
app.MapPost("/api/projects/create", async (Project project, AppDataContext db) =>
{
    if (string.IsNullOrEmpty(project.Name))
    {
        return Results.BadRequest("O nome do projeto é obrigatório.");
    }

    if (project.StartDate >= project.FinalDate)
    {
        return Results.BadRequest("A data de início deve ser anterior à data final.");
    }

    db.Projects.Add(project);
    await db.SaveChangesAsync();

    return Results.Created($"/api/projects/{project.Id}", project);
});

// Listar Projetos
app.MapGet("/api/projects/list", async (AppDataContext db) =>
{
    var projects = await db.Projects.ToListAsync();

    if (projects.Count <= 0)
    {
        return Results.NotFound("Não há nenhum projeto criado");
    }

    return Results.Ok(projects);
});

// Update Project
app.MapPut("/api/projects/update/{id}", async (int id, Project updatedProject, AppDataContext db) =>
{
    var project = await db.Projects.FindAsync(id);
    if (project is null)
    {
        return Results.NotFound("Projeto não encontrado.");
    }

    project.Name = updatedProject.Name ?? project.Name;
    project.Description = updatedProject.Description ?? project.Description;
    project.StartDate = updatedProject.StartDate;
    project.FinalDate = updatedProject.FinalDate;

    await db.SaveChangesAsync();

    return Results.Ok(project);
});

// Delete Project
app.MapDelete("/api/projects/delete/{id}", async (int id, AppDataContext db) =>
{
    var project = await db.Projects.FindAsync(id);
    if (project is null)
    {
        return Results.NotFound("Projeto não encontrado.");
    }

    db.Projects.Remove(project);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.UseCors("Total Acess");
app.Run();
