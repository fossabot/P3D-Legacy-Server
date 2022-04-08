﻿using Aragas.Extensions.Options.FluentValidation.Extensions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using P3D.Legacy.Server.Abstractions.Options;
using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.Application.Extensions;
using P3D.Legacy.Server.Client.P3D.Extensions;
using P3D.Legacy.Server.Client.P3D.Options;
using P3D.Legacy.Server.CommunicationAPI.Extensions;
using P3D.Legacy.Server.DiscordBot.Extensions;
using P3D.Legacy.Server.DiscordBot.Options;
using P3D.Legacy.Server.Extensions;
using P3D.Legacy.Server.GameCommands.Extensions;
using P3D.Legacy.Server.GUI.Extensions;
using P3D.Legacy.Server.Infrastructure.Extensions;
using P3D.Legacy.Server.Infrastructure.Options;
using P3D.Legacy.Server.InternalAPI.Extensions;
using P3D.Legacy.Server.InternalAPI.Options;
using P3D.Legacy.Server.Options;
using P3D.Legacy.Server.Statistics.Extensions;

using Serilog;
using Serilog.Events;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace P3D.Legacy.Server
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting P3D-Legacy Server");
                await CreateHostBuilder(args).Build().RunAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "P3D-Legacy Server terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureServices((ctx, services) =>
            {
                services.Configure<ConsoleLifetimeOptions>(opts => opts.SuppressStatusMessages = true);

                services.AddValidatedOptions<ServerOptions, ServerOptionsValidator>(ctx.Configuration.GetSection("Server"));
                services.AddValidatedOptionsWithHttp<P3DSiteOptions, P3DSiteOptionsValidator>(ctx.Configuration.GetSection("OfficialSite"));
                services.AddValidatedOptions<P3DServerOptions, P3DServerOptionsValidator>(ctx.Configuration.GetSection("P3DServer"));
                services.AddValidatedOptions<DiscordOptions, DiscordOptionsValidator>(ctx.Configuration.GetSection("DiscordBot"));
                services.AddValidatedOptions<LiteDbOptions, LiteDbOptionsValidator>(ctx.Configuration.GetSection("LiteDb"));
                services.AddValidatedOptions<JwtOptions, JwtOptionsValidator>(ctx.Configuration.GetSection("Jwt"));
                services.AddValidatedOptions<OtlpOptions, OtlpOptionsValidator>(ctx.Configuration.GetSection("Otlp"));

                services.AddMediatRInternal();
                using (var requestRegistrar = new RequestRegistrar(services))
                using (var notificationRegistrar = new NotificationRegistrar(services))
                {
                    services.AddHostMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddApplicationMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddClientP3DMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddCommunicationAPIMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddDiscordBotMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddGameCommandsMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddInfrastructureMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddInternalAPIMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddStatisticsMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                    services.AddGUIMediatR(ctx.Configuration, requestRegistrar, notificationRegistrar);
                }

                services.AddHost(ctx.Configuration);
                services.AddApplication(ctx.Configuration);
                services.AddClientP3D(ctx.Configuration);
                services.AddCommunicationAPI(ctx.Configuration);
                services.AddDiscordBot(ctx.Configuration);
                services.AddGameCommands(ctx.Configuration);
                services.AddInfrastructure(ctx.Configuration);
                services.AddInternalAPI(ctx.Configuration);
                services.AddStatistics(ctx.Configuration);
                services.AddGUI(ctx.Configuration);

                services.AddOpenTelemetryMetrics(b => b.Configure((sp, builder) =>
                {
                    var options = sp.GetRequiredService<IOptions<OtlpOptions>>().Value;
                    if (options.Enabled)
                    {
                        builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("P3D.Legacy.Server"));

                        builder.AddAspNetCoreInstrumentation();
                        builder.AddHttpClientInstrumentation();

                        builder.AddStatisticsInstrumentation();

                        builder.AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(options.Host);
                        });
                    }
                }));
                services.AddOpenTelemetryTracing(b => b.Configure((sp, builder) =>
                {
                    var options = sp.GetRequiredService<IOptions<OtlpOptions>>().Value;
                    if (options.Enabled)
                    {
                        builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("P3D.Legacy.Server"));

                        builder.AddAspNetCoreInstrumentation();
                        builder.AddHttpClientInstrumentation();

                        builder.AddHostInstrumentation();
                        builder.AddApplicationInstrumentation();
                        builder.AddClientP3DInstrumentation();
                        builder.AddCommunicationAPIInstrumentation();
                        builder.AddDiscordBotInstrumentation();
                        builder.AddGameCommandsInstrumentation();
                        builder.AddInfrastructureInstrumentation();
                        builder.AddInternalAPIInstrumentation();
                        builder.AddStatisticsInstrumentation();

                        builder.AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(options.Host);
                        });
                    }
                }));
            })
            .AddP3DServer()
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
            .UseSerilog((context, services, configuration) => configuration
                .WriteTo.Console()
                .WriteTo.UI(services)
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services))
        ;
    }
}