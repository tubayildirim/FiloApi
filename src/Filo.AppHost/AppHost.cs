var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Filo_Api>("filo-api");

builder.Build().Run();
