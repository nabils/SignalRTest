using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SignalR.Hubs;

namespace WebApplication2
{
    public class Chat : Hub, IDisconnect, IConnected
    {
        private static readonly ConcurrentDictionary<string, UserConnection> Users = new ConcurrentDictionary<string, UserConnection>();

        public Task Disconnect()
        {
            UserConnection val;
            Users.TryRemove(Context.ConnectionId, out val);
            return Clients.NoOfUsersUpdated(new UserConnectionInfo { NoOfConnectedUsers = GetUserCount(), UserConnections = Users.Select(kv => kv.Value).ToArray() });
        }

        public Task Connect()
        {
            return null;
        }

        public Task Reconnect(IEnumerable<string> groups)
        {
            return Clients.NoOfUsersUpdated(new UserConnectionInfo { NoOfConnectedUsers = GetUserCount(), UserConnections = Users.Select(kv => kv.Value).ToArray() });
        }

        public void RequestScreenshot(string connectionId)
        {
            Clients[connectionId].TakeScreenshot();
        }

        public void ScreenshotTaken(byte[] imagePng)
        {
            var username = Users[Context.ConnectionId].UserName;
            File.WriteAllBytes(string.Format(@"C:\Users\nabils\Documents\visual studio 11\Projects\SignalRTest\{0}.png", username), imagePng);
        }

        public void UserJoined(string userName)
        {
            Users.TryAdd(Context.ConnectionId, new UserConnection {
                UserName   = userName,
                ConnectedSinceUtcDate = DateTime.UtcNow,
                ConnectionId = Context.ConnectionId
            });
            
            Clients.NoOfUsersUpdated(new UserConnectionInfo { NoOfConnectedUsers = GetUserCount(), UserConnections = Users.Select(kv => kv.Value).ToArray() });
        }

        private static int GetUserCount()
        {
            return Users.Select(kv => kv.Value.UserName).Distinct().Count();
        }
    }

    public class UserConnection
    {
        public string ConnectionId { get; set; }
        public string UserName { get; set; }
        public DateTime ConnectedSinceUtcDate { get; set; }
    }

    public class UserConnectionInfo
    {
        public int NoOfConnectedUsers { get; set; }
        public UserConnection[] UserConnections { get; set; }
    }
}