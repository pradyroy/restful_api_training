using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Users.Application.Users.Dtos;
using Users.Application.Users.Services;

namespace Users.Api.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users");

        // 1. Create user  - POST /api/users
        group.MapPost("/", async (
            [FromBody] CreateUserRequest request,
            [FromServices] UserService service,
            CancellationToken ct) =>
        {
            try
            {
                var created = await service.CreateAsync(request, ct);
                return Results.Created($"/api/users/id/{created.Id}", created);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        });

        // 2. Get user by ID - GET /api/users/id/{id}
        group.MapGet("/id/{id:long}", async (
            long id,
            [FromServices] UserService service,
            CancellationToken ct) =>
        {
            var user = await service.GetByIdAsync(id, ct);
            return user is null ? Results.NotFound() : Results.Ok(user);
        });

        // 3. Get all users (paged) - GET /api/users?skip={x}&take={y}
        group.MapGet("/", async (
            [FromQuery] int? skip,
            [FromQuery] int? take,
            [FromServices] UserService service,
            CancellationToken ct) =>
        {
            var s = skip.GetValueOrDefault(0);
            var t = take.GetValueOrDefault(10); // default page size 10

            var result = await service.GetPagedAsync(s, t, ct);
            return Results.Ok(result);
        });

        // 4. Get all users (no paging) - GET /api/users/all
        group.MapGet("/all", async (
            [FromServices] UserService service,
            CancellationToken ct) =>
        {
            var users = await service.GetAllAsync(ct);
            return Results.Ok(users);
        });

        // 5. Filtered users (paged) - GET /api/users/filter?...&skip=&take=
        group.MapGet("/filter", async (
            [FromQuery] string? userName,
            [FromQuery] string? role,
            [FromQuery] string? emailId,
            [FromQuery] string? mobileNum,
            [FromQuery] int? skip,
            [FromQuery] int? take,
            [FromServices] UserService service,
            CancellationToken ct) =>
        {
            var filter = new UserFilterRequest
            {
                UserName = userName,
                Role = role,
                EmailId = emailId,
                MobileNum = mobileNum,
                Skip = skip,
                Take = take
            };

            var result = await service.GetFilteredPagedAsync(filter, ct);
            return Results.Ok(result);
        });

        // 6. Filtered users (no paging) - GET /api/users/filter/all?field=value...
        group.MapGet("/filter/all", async (
            [FromQuery] string? userName,
            [FromQuery] string? role,
            [FromQuery] string? emailId,
            [FromQuery] string? mobileNum,
            [FromServices] UserService service,
            CancellationToken ct) =>
        {
            var filter = new UserFilterRequest
            {
                UserName = userName,
                Role = role,
                EmailId = emailId,
                MobileNum = mobileNum
            };

            var users = await service.GetFilteredAsync(filter, paged: false, ct);
            return Results.Ok(users);
        });

        // 7. Update user - PUT /api/users/id/{id}
        group.MapPut("/id/{id:long}", async (
            long id,
            [FromBody] UpdateUserRequest request,
            [FromServices] UserService service,
            CancellationToken ct) =>
        {
            var updated = await service.UpdateAsync(id, request, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        // 8. Upload profile picture (generic) - POST /api/users/id/{id}/upload
        group.MapPost("/id/{id:long}/upload", async (
            long id,
            IFormFile file,
            [FromForm] string folder,
            [FromForm] string fieldname,
            [FromServices] UserService service,
            [FromServices] IWebHostEnvironment env,
            CancellationToken ct) =>
        {
            if (file is null || file.Length == 0)
                return Results.BadRequest(new { message = "File is required." });

            // Root folder for uploads: <contentroot>/uploads/{folder}
            var uploadsRoot = Path.Combine(env.ContentRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            // For training, URL is relative; in real apps this might be CDN or static host
            var url = $"/uploads/{folder}/{fileName}";

            var updated = await service.UpdateProfileFieldAsync(id, fieldname, url, ct);
            if (updated is null)
            {
                return Results.NotFound(new { message = $"User with id {id} not found." });
            }

            return Results.Ok(updated);
        }).DisableAntiforgery();

        // 9. Delete user - DELETE /api/users/id/{id}
        group.MapDelete("/id/{id:long}", async (
            long id,
            [FromServices] UserService service,
            CancellationToken ct) =>
        {
            var success = await service.DeleteAsync(id, ct);
            return success ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}
