using System;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WinFormsMain.Invoicing;
using WinFormsMain.VehicleLookup;

namespace WinFormsMain
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var builder = Host.CreateApplicationBuilder();
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            builder.Services
                .AddLogging()
                .AddOptions<MotorWebOptions>()
                .Bind(builder.Configuration.GetSection(MotorWebOptions.SectionName))
                .ValidateDataAnnotations()
                .Services
                .AddOptions<EmailOptions>()
                .Bind(builder.Configuration.GetSection(EmailOptions.SectionName))
                .ValidateDataAnnotations()
                .Services
                .AddHttpClient<MotorWebClient>()
                .Services
                .AddSingleton<VehicleLookupService>()
                .AddSingleton<VehicleLookupController>()
                .AddSingleton<PdfInvoiceService>()
                .AddSingleton<EmailService>()
                .AddSingleton<MainForm>();

            using var host = builder.Build();

            Application.Run(host.Services.GetRequiredService<MainForm>());
        }
    }
}
