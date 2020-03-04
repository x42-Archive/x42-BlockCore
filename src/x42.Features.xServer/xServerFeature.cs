﻿using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Builder;
using Stratis.Bitcoin.Builder.Feature;
using Stratis.Bitcoin.Configuration.Logging;
using Stratis.Bitcoin.Utilities;
using x42.Features.xServer.Interfaces;
using TracerAttributes;
using System.Linq;

[assembly: InternalsVisibleTo("x42.Features.xServer.Tests")]

namespace x42.Features.xServer
{
    public class xServerFeature : FullNodeFeature
    {
        /// <summary>Instance logger.</summary>
        private readonly ILogger logger;

        /// <summary>
        /// Manager for xServers
        /// </summary>
        private readonly IxServerManager xServerManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="xServerFeature"/> class with the xServers.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="nodeStats">The node stats.</param>
        /// <param name="xServerManager">The wallet manager.</param>
        public xServerFeature(
            ILoggerFactory loggerFactory,
            INodeStats nodeStats,
            IxServerManager xServerManager)
        {
            Guard.NotNull(xServerManager, nameof(IxServerManager));

            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);
            this.xServerManager = xServerManager;

            nodeStats.RegisterStats(AddInlineStats, StatsType.Component, this.GetType().Name);
        }

        [NoTrace]
        private void AddInlineStats(StringBuilder log)
        {
            var connectedPeers = this.xServerManager.ConnectedSeeds;
            var builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine($"====== xServer Network ({connectedPeers.Count()}) ======");
            foreach (var peer in connectedPeers)
            {
                builder.AppendLine(
                    ($"{peer.Name}: {peer.Address}:{peer.Port}").PadRight(LoggingConfiguration.ColumnLength + 15)
                    + ($"Response Time: {peer.ResponseTime} ms").PadRight(LoggingConfiguration.ColumnLength + 14)
                    + ($"Version: {peer.Version}").PadRight(LoggingConfiguration.ColumnLength + 7)
                    );
            }

            log.AppendLine(builder.ToString());
        }

        /// <summary>
        /// Prints command-line help. Invoked via reflection.
        /// </summary>
        /// <param name="network">The network to extract values from.</param>
        public static void PrintHelp(Network network)
        {
            xServerSettings.PrintHelp(network);
        }

        /// <summary>
        /// Get the default configuration. Invoked via reflection.
        /// </summary>
        /// <param name="builder">The string builder to add the settings to.</param>
        /// <param name="network">The network to base the defaults off.</param>
        public static void BuildDefaultConfigurationFile(StringBuilder builder, Network network)
        {
            xServerSettings.BuildDefaultConfigurationFile(builder, network);
        }

        public override Task InitializeAsync()
        {
            this.xServerManager.Start();
            this.logger.LogInformation("xServer Network Activated.");
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            this.logger.LogInformation("Stopping xServer Feature.");

            this.xServerManager.Stop();
        }
    }

    /// <summary>
    /// A class providing extension methods for <see cref="IFullNodeBuilder"/>.
    /// </summary>
    public static class FullNodeBuilderxServerExtension
    {
        public static IFullNodeBuilder UsexServer(this IFullNodeBuilder fullNodeBuilder)
        {
            LoggingConfiguration.RegisterFeatureNamespace<xServerFeature>("xserver");

            fullNodeBuilder.ConfigureFeature(features =>
            {
                features
                .AddFeature<xServerFeature>()
                .FeatureServices(services =>
                    {
                        services.AddSingleton<xServerSettings>();
                        services.AddSingleton<IxServerManager, xServerManager>();
                    });
            });

            return fullNodeBuilder;
        }
    }
}
