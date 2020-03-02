using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace x42.Features.xServer
{
    /// <summary>
    /// This interface defines a xServer peers file used to cache the whitelisted peers discovered by the xServer feature service.
    /// </summary>
    public class xServerPeers
    {
        private string XServerPeersFilePath { get; set; }

        public xServerPeers(string filePath)
        {
            this.XServerPeersFilePath = filePath;
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            else if (File.Exists(filePath))
            {
                Load();
            }
        }

        /// <summary>
        /// The xServer peers file.
        /// </summary>
        public ConcurrentBag<xServerPeer> Peers = new ConcurrentBag<xServerPeer>();

        /// <summary>
        /// Loads the saved xServer peers file from the specified stream.
        /// </summary>
        private void Load()
        {
            using (var peersFile = new StreamReader(this.XServerPeersFilePath))
            {
                string peersData = peersFile.ReadToEnd();
                this.Peers = JsonConvert.DeserializeObject<ConcurrentBag<xServerPeer>>(peersData);
            }
        }

        /// <summary>
        /// Saves the cached xServer peers file to the specified stream.
        /// </summary>
        public Task Save()
        {
            using (StreamWriter file = File.CreateText(this.XServerPeersFilePath))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, this.Peers);
            }
            return Task.CompletedTask;
        }
    }
}