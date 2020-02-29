using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Builder;
using Stratis.Bitcoin.Builder.Feature;
using Stratis.Bitcoin.Configuration.Logging;
using Stratis.Bitcoin.Utilities;
using TracerAttributes;

[assembly: InternalsVisibleTo("x42.Features.xServer.Tests")]

namespace x42.Features.xServer
{
    public class xServerFeature : FullNodeFeature
    {
        /// <summary>Instance logger.</summary>
        private readonly ILogger logger;

        public xServerFeature(
            ILoggerFactory loggerFactory,
            INodeStats nodeStats)
        {
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
            this.logger.LogInformation("xServer Network Activated.");
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            this.logger.LogInformation("Stopping xServer Feature.");
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
