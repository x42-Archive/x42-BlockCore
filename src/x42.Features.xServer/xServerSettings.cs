using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Configuration;
using Stratis.Bitcoin.Utilities;
using Stratis.Bitcoin.Utilities.Extensions;

namespace x42.Features.xServer
{
    /// <summary>
    /// Configuration related to storage of transactions.
    /// </summary>
    public class xServerSettings
    {
        /// <summary>Instance logger.</summary>
        private readonly ILogger logger;

        /// <summary>List of end points that the node should try to connect to.</summary>
        /// <remarks>All access should be protected under <see cref="addxServerNodeLock"/></remarks>
        private readonly List<IPEndPoint> addxServerNode;

        /// <summary>
        /// Protects access to the list of addnode endpoints.
        /// </summary>
        private readonly object addxServerNodeLock;

        /// <summary>
        /// Initializes an instance of the object from the node configuration.
        /// </summary>
        /// <param name="nodeSettings">The node configuration.</param>
        public xServerSettings(NodeSettings nodeSettings)
        {
            Guard.NotNull(nodeSettings, nameof(nodeSettings));

            this.logger = nodeSettings.LoggerFactory.CreateLogger(typeof(xServerSettings).FullName);

            TextFileConfiguration config = nodeSettings.ConfigReader;

            this.addxServerNodeLock = new object();

            lock (this.addxServerNodeLock)
            {
                this.addxServerNode = new List<IPEndPoint>();
            }

            try
            {
                foreach (IPEndPoint addNode in config.GetAll("addxservernode", this.logger).Select(c => c.ToIPEndPoint(nodeSettings.Network.DefaultPort)))
                    this.AddAddNode(addNode);
            }
            catch (FormatException)
            {
                throw new ConfigurationException("Invalid 'addxservernode' parameter.");
            }
        }

        public void AddAddNode(IPEndPoint addNode)
        {
            lock (this.addxServerNodeLock)
            {
                this.addxServerNode.Add(addNode);
            }
        }

        public void RemoveAddNode(IPEndPoint addNode)
        {
            lock (this.addxServerNodeLock)
            {
                this.addxServerNode.Remove(addNode);
            }
        }

        /// <summary>Prints the help information on how to configure the block store settings to the logger.</summary>
        public static void PrintHelp(Network network)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"-addxservernode=<ip:port>        Add a xServer node to connect to and attempt to keep the connection open. Can be specified multiple times.");

            NodeSettings.Default(network).Logger.LogInformation(builder.ToString());
        }

        /// <summary>
        /// Get the default configuration.
        /// </summary>
        /// <param name="builder">The string builder to add the settings to.</param>
        /// <param name="network">The network to base the defaults off.</param>
        public static void BuildDefaultConfigurationFile(StringBuilder builder, Network network)
        {
            builder.AppendLine("####xServer Settings####");
            builder.AppendLine($"#Add a xServer node to connect to and attempt to keep the connection open. Can be specified multiple times.");
            builder.AppendLine($"#addxservernode=<ip:port>");
        }
    }
}