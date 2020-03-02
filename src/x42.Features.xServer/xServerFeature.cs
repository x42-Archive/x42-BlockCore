using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NBitcoin.Protocol;
using RestSharp;
using Stratis.Bitcoin.AsyncWork;
using Stratis.Bitcoin.Builder;
using Stratis.Bitcoin.Builder.Feature;
using Stratis.Bitcoin.Configuration;
using Stratis.Bitcoin.Configuration.Logging;
using Stratis.Bitcoin.P2P;
using Stratis.Bitcoin.Utilities;
using Stratis.Bitcoin.Utilities.Extensions;
using TracerAttributes;
using x42.Features.xServer.Models;

[assembly: InternalsVisibleTo("x42.Features.xServer.Tests")]

namespace x42.Features.xServer
{
    public class xServerFeature : FullNodeFeature
    {
        /// <summary>
        /// Defines the name of the xServer peers on disk.
        /// </summary>
        public const string xServerPeersFileName = "xserverpeers.json";

        /// <summary>
        /// Sets the period by which the xServer discovery check occurs (secs).
        /// </summary>
        private const int CheckXServerRate = 600;

        /// <summary>Instance logger.</summary>
        private readonly ILogger logger;

        /// <summary>
        /// Defines the data folders of the system.
        /// </summary>
        private readonly DataFolder dataFolders;

        /// <summary>
        /// Will discover and manage xServers.
        /// </summary>
        private IAsyncLoop xServerDiscoveryLoop;

        /// <summary>
        /// Provider for creating and managing async loops.
        /// </summary>
        private readonly IAsyncProvider asyncProvider;

        /// <summary>
        /// Defines a node lifetime object.
        /// </summary>
        private readonly INodeLifetime nodeLifetime;

        /// <summary>The network the node is running on.</summary>
        private readonly Network network;

        /// <summary>
        /// Initializes a new instance of the <see cref="xServerFeature"/> class with the xServers.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="nodeStats">The node stats.</param>
        /// <param name="dataFolders">The data folders of the system.</param>
        /// <param name="asyncProvider">The async loop factory.</param>
        public xServerFeature(
            ILoggerFactory loggerFactory,
            INodeStats nodeStats,
            DataFolder dataFolders,
            IAsyncProvider asyncProvider,
            INodeLifetime nodeLifetime,
            Network network)
        {
            Guard.NotNull(dataFolders, nameof(dataFolders));
            Guard.NotNull(asyncProvider, nameof(asyncProvider));
            Guard.NotNull(nodeLifetime, nameof(nodeLifetime));
            Guard.NotNull(network, nameof(network));

            this.dataFolders = dataFolders;
            this.asyncProvider = asyncProvider;
            this.nodeLifetime = nodeLifetime;
            this.network = network;

            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);

            nodeStats.RegisterStats(this.AddInlineStats, StatsType.Component, this.GetType().Name);
        }

        [NoTrace]
        private void AddInlineStats(StringBuilder log)
        {
            var builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine("======xServer Network======");
            builder.AppendLine("Connected xServers: 0");

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
            StartxServerDiscoveryLoop();
            this.logger.LogInformation("xServer Network Activated.");
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            this.logger.LogInformation("Stopping xServer Feature.");

            this.xServerDiscoveryLoop?.Dispose();
        }

        /// <summary>
        /// Starts the loop to save the masterfile.
        /// </summary>
        private void StartxServerDiscoveryLoop()
        {
            string path = Path.Combine(this.dataFolders.xServerAppsPath, xServerPeersFileName);
            xServerPeers xServerPeerList = new xServerPeers(path);

            this.xServerDiscoveryLoop = this.asyncProvider.CreateAndRunAsyncLoop($"{nameof(xServerFeature)}.WhitelistRefreshLoop", async token =>
            {
                await XServerDiscoveryAsync(xServerPeerList).ConfigureAwait(false);
                this.logger.LogInformation($"Saving cached xServer Seeds to {path}");
                await xServerPeerList.Save().ConfigureAwait(false);
            },
            this.nodeLifetime.ApplicationStopping,
            repeatEvery: TimeSpan.FromSeconds(CheckXServerRate));
        }

        private async Task XServerDiscoveryAsync(xServerPeers xServerPeerList)
        {
            int topResult = 10;
            await this.network.XServerSeedNodes.ForEachAsync(10, this.nodeLifetime.ApplicationStopping, async (networkAddress, cancellation) =>
            {
                if (this.nodeLifetime.ApplicationStopping.IsCancellationRequested)
                    return;

                string xServerURL = $"{(networkAddress.IsSSL ? "https" : "http")}://{networkAddress.PublicAddress}:{networkAddress.Port}";

                this.logger.LogDebug($"Attempting connection to {xServerURL}.");

                var client = new RestClient(xServerURL);
                var topXServersRequest = new RestRequest("/gettop/", Method.GET);
                topXServersRequest.AddParameter("top", topResult);
                var topXServerResult = await client.ExecuteAsync<TopResult>(topXServersRequest, cancellation).ConfigureAwait(false);
                if (topXServerResult.StatusCode == HttpStatusCode.OK)
                {
                    if (topXServerResult.Data?.XServers?.Count > 0)
                    {
                        var xServers = topXServerResult.Data.XServers;
                        foreach (var xServer in xServers)
                        {
                            var pingRequest = new RestRequest("/ping/", Method.GET);
                            var pingResponseTime = Stopwatch.StartNew();
                            var pingResult = await client.ExecuteAsync<PingResult>(pingRequest, cancellation).ConfigureAwait(false);
                            pingResponseTime.Stop();
                            if (topXServerResult.StatusCode == HttpStatusCode.OK)
                            {
                                var ping = pingResult.Data;
                                xServerPeerList.Peers.Add(new xServerPeer()
                                {
                                    Name = xServer.Name,
                                    Address = xServer.Address,
                                    Port = xServer.Port,
                                    Priority = xServer.Priotiry,
                                    Version = ping.Version,
                                    ResponseTime = pingResponseTime.ElapsedMilliseconds
                                });
                            }
                        }
                    }
                }

            }).ConfigureAwait(false);
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
                    });
            });

            return fullNodeBuilder;
        }
    }
}
