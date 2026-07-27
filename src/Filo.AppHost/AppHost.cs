var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Filo_Api>("filo-api")
    .WithHttpsEndpoint(port: 7010, name: "https")
    .WithHttpEndpoint(port: 5221, name: "http");

builder.Build().Run();
