using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal static class Extensions
{
    public static IResourceBuilder<ContainerResource> AddMemcached(this IDistributedApplicationBuilder builder, string name)
    {
        return builder.AddContainer(name, "memcached", "1.6.38")
            .WithEndpoint(name: "tcp", targetPort: 11211);
    }

    public static IResourceBuilder<ContainerResource> AddEJabberd(this IDistributedApplicationBuilder builder, string name)
    {
        return builder.AddContainer(name, "ejabberd/ecs", "25.07")
            .WithEndpoint(name: "c2s", targetPort: 5222)
            .WithEndpoint(name: "s2s", targetPort: 5269)
            .WithHttpEndpoint(targetPort: 5280)
            .WithHttpsEndpoint(targetPort: 5443);
    }

    public static IResourceBuilder<ContainerResource> AddOpenSrfRouter(this IDistributedApplicationBuilder builder, string name)
    {
        return builder.AddEvergreen(name, "router");
    }

    public static IResourceBuilder<ContainerResource> AddEvergreenDatabase(this IDistributedApplicationBuilder builder, string name, IResourceBuilder<ParameterResource> password)
    {
        var adminPassword = builder.AddResource(ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, "dbAdminPassword"));

        var postgres = builder.AddPostgres(name, password: adminPassword)
            .WithDockerfile(@"C:\Users\twsou\projects\evergreen\build", stage: "postgres-ils");

        var db = postgres.AddDatabase("evergreen");

        adminPassword.WithParentRelationship(postgres);

        var dbInit = builder.AddEvergreen("db-init", "db-init")
            .WithEnvironment("POSTGRES_USER", "postgres")
            .WithEnvironment("POSTGRES_PASSWORD", adminPassword)
            .WithEnvironment("POSTGRES_ADMIN_USER", "postgres")
            .WithEnvironment("POSTGRES_ADMIN_PASSWORD", adminPassword)
            .WithEnvironment("POSTGRES_DB", db.Resource.DatabaseName)
            .WithEnvironment(ctx =>
            {
                ctx.EnvironmentVariables["POSTGRES_PORT"] = postgres.GetEndpoint("tcp").TargetPort!;
                ctx.EnvironmentVariables["POSTGRES_HOST"] = name;
            })
            .WithReference(postgres)
            .WithParentRelationship(postgres)
            .WaitFor(postgres, WaitBehavior.WaitOnResourceUnavailable);

        return postgres;
    }

    private static IResourceBuilder<ContainerResource> AddEvergreen(this IDistributedApplicationBuilder builder, string name, string stage)
    {
        return builder.AddDockerfile(name, @"C:\Users\twsou\projects\evergreen\build", stage: stage);
    }
}
