using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

builder.Services.AddAuthorization(o =>
{
    // Create and authorisation policy based upon a specific scope to say hello
    o.AddPolicy("HelloCaller", new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .RequireClaim("http://schemas.microsoft.com/identity/claims/scope", builder.Configuration["AzureAd:HelloScope"])
        .Build());
});

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Base home endpoint that is unsecured
app.MapGet("/", (HttpContext httpContext) =>
{
    return $"Hello world";
});

// Hello endpoint which implements the HelloCaller authorisation policy
app.MapGet("/hello", (HttpContext httpContext) =>
{
    var name = httpContext.User.Claims.First(c => c.Type == "name").Value;

    return $"Hello {name}";
})
.RequireAuthorization("HelloCaller");

// Goodbye endpoint which uses an alternative mechanism (VerifyUserHasAnyAcceptedScope) to check for the required scope
app.MapGet("/goodbye", (HttpContext httpContext) =>
{
    httpContext.VerifyUserHasAnyAcceptedScope(new string[] { builder.Configuration["AzureAd:GoodbyeScope"] });

    var name = httpContext.User.Claims.First(c => c.Type == "name").Value;

    return "Goodbye {name}";
})
.RequireAuthorization();

app.Run();
