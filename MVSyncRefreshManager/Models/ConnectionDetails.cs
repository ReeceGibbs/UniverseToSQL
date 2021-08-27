using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVSyncRefreshManager.Models
{
    class ConnectionDetails
    {
        public string HostName { get; set; }
        public int UniRpcPort { get; set; }
        public int SshPort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SourceAccount { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
    }
}
