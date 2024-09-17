using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace server
{
    public class Client
    {
        public string Username { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }

        public Client(TcpClient client) 
        {
            ClientSocket = client;
            UID = Guid.NewGuid();
            StreamReader reader = new StreamReader(ClientSocket.GetStream());
            var receivedData = reader.ReadLine();
            string[] parts = receivedData.Split('|'); // Assuming "|" is a delimiter between OpCode, sender and message
            if (parts.Length >= 2)
            {
                Username = parts[1];
            }
        }
    }
}