var builder = WebApplication.CreateBuilder(args);

// Adiciona controllers
builder.Services.AddControllers();

//Adiciona Swagger para documentacao da API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registra os módulos e demais serviços
builder.AddRabbitMqService();
builder.AddClientModule();
builder.AddCreditCardModule();
builder.AddCreditProposalModule();

var app = builder.Build();

// Configura a pipeline de HTTP request.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
