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
        public string? Username { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }

        public Client(TcpClient client) 
        {
            ClientSocket = client;
            UID = Guid.NewGuid();
        }

        public void SetUsername()
        {
            Console.Write("Enter your username: ");
            Username = Console.ReadLine();
            while (string.IsNullOrEmpty(Username)) {
                Console.Write("Enter your username: ");
                Username = Console.ReadLine();
            }
        }
    }
}