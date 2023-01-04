using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoAccess;
using MongoAccess.Model;
using MongoAccess.DataAccess;
using MQTTnet;
using MQTTnet.AspNetCore;
using MQTTnet.AspNetCore.Extensions;
using MQTTnet.Client.Options;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTTopicManager;
using Microsoft.AspNetCore.SignalR;
using RealtimeSignal;
 


namespace MQTTWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            MongoDbSet mongoDeviceObjectDbSet = new MongoDbSet
            {
                ConnectionString = Configuration["MongoDeviceObjectDatabaseSettings:ConnectionString"],
                DatabaseName = Configuration["MongoDeviceObjectDatabaseSettings:DatabaseName"],
                CollectionName = Configuration["MongoDeviceObjectDatabaseSettings:CollectionName1"]
            };

            MongoDbSet mongoTimeSeriesDbSet = new MongoDbSet
            {
                ConnectionString = Configuration["MongoDeviceObjectDatabaseSettings:ConnectionString"],
                DatabaseName = Configuration["MongoDeviceObjectDatabaseSettings:DatabaseName"],
                CollectionName = Configuration["MongoDeviceObjectDatabaseSettings:CollectionName2"]
            };

            DeviceMongoDriver<DeviceObject> deviceMongoDriverDeviceObject = new DeviceMongoDriver<DeviceObject>(mongoDeviceObjectDbSet);
            DeviceMongoDriver<TimeSeriesObject> deviceMongoDriverTimeSeriesObject = new DeviceMongoDriver<TimeSeriesObject>(mongoTimeSeriesDbSet);
            services.AddSignalR();

            services.AddSingleton<DeviceMongoDriver<DeviceObject>>(deviceMongoDriverDeviceObject);
            services.AddSingleton<DeviceMongoDriver<TimeSeriesObject>>(deviceMongoDriverTimeSeriesObject);
    
            // string ConnectionStringSignalR = Configuration["SignalRHostSettings:URL"];
            //TimeSpan keepaliveinterval = new TimeSpan(0,0, 10);

            var optionsClient = new MqttClientOptionsBuilder()
                .WithClientId(Configuration["MosquittoBrokerClientSettings:Id"])
                .WithTcpServer(Configuration["MosquittoBrokerHostSettings:Host"], Convert.ToInt32(Configuration["MosquittoBrokerHostSettings:Port"]))
                .WithCredentials(Configuration["MosquittoBrokerClientSettings:Username"], Configuration["MosquittoBrokerClientSettings:Password"])
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                .WithCleanSession(false)
                .Build();
            ManagedMqttClientOptions options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(2))                
                .WithClientOptions(optionsClient)
                .Build();
            TopicManager topicManager = new TopicManager(options, deviceMongoDriverDeviceObject, deviceMongoDriverTimeSeriesObject);       
            services.AddSingleton<ITopicManager>(topicManager);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapHub<RealtimeSignalHub>("/chathub");

            });

            //lifetime.ApplicationStarted.Register(() => RegisterSignalRWithRabbitMQ(app.ApplicationServices));

            var vITopicManager = (ITopicManager)app.ApplicationServices.GetService(typeof(ITopicManager));

            vITopicManager.TopicManagerInit(app.ApplicationServices);
            
        }
    }
}
