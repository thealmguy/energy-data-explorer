var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.EnergyDataExplorer_Api>("api");

builder.AddProject<Projects.EnergyDataExplorer_Web>("web")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
