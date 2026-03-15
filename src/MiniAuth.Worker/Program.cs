using MiniAuth.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Registra o EmailService no container de DI
builder.Services.AddSingleton<EmailService>();

// Registra o Worker como HostedService (roda em background)
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
