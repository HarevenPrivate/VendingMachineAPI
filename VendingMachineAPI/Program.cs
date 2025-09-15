using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VendingMachineAPI.Hubs;
using VendingMachineAPI.Interface;
using VendingMachineAPI.Services;

var builder = WebApplication.CreateBuilder(args);


// Add controllers
builder.Services.AddControllers();

// Add the vending machine service (singleton)
builder.Services.AddSignalR();
builder.Services.AddSingleton<IPanelService, PanelService>();
builder.Services.AddSingleton<IProductRepository,ProductRepositoryService>();
builder.Services.AddSingleton<IThermostatService, ThermostatService>();
builder.Services.AddSingleton<IMoneyDeviceService, MoneyDeviceService>();
builder.Services.AddSingleton<IVendingMachineService,VendingMachineService>();

//this line is for the keyboard controller it should not consume the entire service but still uses the same singlton instance
//and not create a new one
builder.Services.AddSingleton<IKeyboardService>(sp => sp.GetRequiredService<IVendingMachineService>());


// Add swagger for quick testing
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PanelHub>("/hubs/panel");
app.MapHub<MoneyHub>("/hubs/money");



app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();

