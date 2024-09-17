using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;


namespace client
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
            Username = Console.ReadLine();
            Console.WriteLine($"[{DateTime.Now}]: Client connected with username: {Username}");
        }
    }
}