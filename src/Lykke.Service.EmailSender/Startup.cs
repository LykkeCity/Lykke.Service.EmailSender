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
using Lykke.WebServices.EmailSender.Log;
using Lykke.WebServices.EmailSender.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lykke.WebServices.EmailSender
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ILog log = new LogToConsole();
            try
            {
                // Add framework services.
                services.AddMvc();

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

                // Create the container builder.
                var builder = new ContainerBuilder();

                // Register dependencies, populate the services from
                // the collection, and build the container. If you want
                // to dispose of the container at the end of the app,
                // be sure to keep a reference to it as a property or field.

                builder.RegisterInstance(
                        new AzureTableStorage<PartnerSmtpSettings>(settings.EmailSenderSettings.Partners.ConnectionString,
                            settings.EmailSenderSettings.Partners.TableName, log))
                    .As<INoSQLTableStorage<PartnerSmtpSettings>>()
                    .SingleInstance();
                builder.Populate(services);
                this.ApplicationContainer = builder.Build();

                // Create the IServiceProvider based on the container.
                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                log.WriteFatalErrorAsync(nameof(Startup), nameof(EmailSender), nameof(ConfigureServices), ex, DateTime.UtcNow);
                throw;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }
}
