using Microsoft.AspNetCore.SignalR;

namespace CloseConnectv1.Hubs
{
    public class ConversationHub : Hub
    {
        public readonly static Dictionary<string, string> _clientConnections = new Dictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            string clientId = GetClientIdFromContext(Context); // Get the client ID from the connection context
            string connectionId = Context.ConnectionId; // Get the connection ID

            lock (_clientConnections)
            {
                // Associate the client ID with the connection ID
                _clientConnections[clientId] = connectionId;
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            string clientId = GetClientIdFromContext(Context); // Get the client ID from the connection context

            lock (_clientConnections)
            {
                // Remove the association when the client disconnects
                if (_clientConnections.ContainsKey(clientId))
                {
                    _clientConnections.Remove(clientId);
                }
            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(string clientId, string message)
        {
            string connectionId;
            lock (_clientConnections)
            {
                // Retrieve the connection ID associated with the client ID
                if (_clientConnections.TryGetValue(clientId, out connectionId))
                {
                    // Send the notification to the specific client using the connection ID
                    Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
                }
            }
        }

        private string GetClientIdFromContext(HubCallerContext context)
        {
            // Implement your logic to extract the client ID from the connection context
            // This could be based on authentication or any other means of identifying the client
            // For example:
            string clientId = context.GetHttpContext().Request.Query["clientId"];

            // Return the client ID
            return clientId;
            //return string.Empty; // Replace with your implementation
        }
    }
}
