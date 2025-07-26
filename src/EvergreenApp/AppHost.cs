var builder = DistributedApplication.CreateBuilder(args);

var dbPassword = builder.AddResource(ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, "dbPassword"));

var ejabberd = builder.AddEJabberd("ejabberd");
var memcached = builder.AddMemcached("memcached");
var opensrf = builder.AddOpenSrfRouter("opensrf-router");
var db = builder.AddEvergreenDatabase("db", dbPassword);

builder.Build().Run();
