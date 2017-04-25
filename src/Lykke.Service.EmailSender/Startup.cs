using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.EmailSender.Log;
using Lykke.Service.EmailSender.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;

namespace Lykke.Service.EmailSender
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IContainer ApplicationContainer { get; private set; }

        public IConfigurationRoot Configuration { get; }

        public string ApiVersion => "1.0";
        public string ApiTitle => "Email Sender Service";

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ILog log = new LogToConsole();
            try
            {
                // Add framework services.
                services.AddMvc();

                // Add swagger generator
                services.AddSwaggerGen(x => { x.SwaggerDoc(ApiVersion, new Info { Title = ApiTitle, Version = ApiVersion }); });

                // Load settings
                var settingsUrl = Configuration["SettingsUrl"];
                var httpClient = new HttpClient();
                var response = httpClient.GetAsync(settingsUrl).Result;
                var settingsString = response.Content.ReadAsStringAsync().Result;
                var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<AppSettings>(settingsString);

                if (null != settings.EmailSenderSettings.Log)
                {
                    var logToTable = new LogToTable(settings.EmailSenderSettings.Log.ConnectionString,
                        settings.EmailSenderSettings.Log.TableName, log);
                    log = new LogToAll(log, logToTable);
                }

                // Register dependencies, populate the services from
                // the collection, and build the container. If you want
                // to dispose of the container at the end of the app,
                // be sure to keep a reference to it as a property or field.
                var builder = new ContainerBuilder();

                builder.RegisterInstance(
                        new AzureTableStorage<PartnerSmtpSettings>(settings.EmailSenderSettings.Partners.ConnectionString,
                            settings.EmailSenderSettings.Partners.TableName, log))
                    .As<INoSQLTableStorage<PartnerSmtpSettings>>()
                    .SingleInstance();
                builder.Populate(services);
                ApplicationContainer = builder.Build();

                // Create the IServiceProvider based on the container.
                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                log.WriteFatalErrorAsync(nameof(EmailSender), nameof(Startup), nameof(ConfigureServices), ex, DateTime.UtcNow).Wait();
                throw;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseMvcWithDefaultRoute();

            app.UseSwagger();
            app.UseSwaggerUI(x =>
            {
                x.SwaggerEndpoint($"/swagger/{ApiVersion}/swagger.json", $"{ApiTitle} {ApiVersion}");
            });
        }
    }
}
