using System;
using System.Collections.Generic;
using System.Text;

namespace x42.Features.xServer.Interfaces
{
    /// <summary>
    /// Interface for a manager providing operations for xServers.
    /// </summary>
    public interface IxServerManager
    {
        /// <summary>
        /// Starts any processes for the xServer manager.
        /// </summary>
        void Start();

        /// <summary>
        /// Runs any steps nessesary to stop the xServer manager.
        /// </summary>
        void Stop();

        /// <summary>
        /// A count of connected xServer seeds.
        /// </summary>
        List<xServerPeer> ConnectedSeeds { get; }
    }
}
