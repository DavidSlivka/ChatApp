using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using server;

public enum OpCode
{
    Connected=1,
    Message=2,
    Disconnect=3
}

class Server
{
    private static TcpListener listener;
    private static List<Client> clients = new List<Client>();

    public static void StartServer()
    {
        
        listener = new TcpListener(IPAddress.Any, 8888);
        listener.Start();
        Console.WriteLine("Server started...");

        while (true)
        {
            Client client = new Client(listener.AcceptTcpClient());
            clients.Add(client);

            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();

            Console.WriteLine($"New client {client.Username} connected...");
        }
    }

    private static void HandleClient(Client client)
    {
        NetworkStream stream = client.ClientSocket.GetStream();
        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
        StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
        writer.AutoFlush = true;

        try
        {
            while (true)
            {
                string receivedData = reader.ReadLine();
                if (!string.IsNullOrEmpty(receivedData))
                {
                    string[] parts = receivedData.Split('|'); // Assuming "|" is a delimiter between OpCode and message
                    if (parts.Length >= 2)
                    {
                        if (!string.IsNullOrEmpty(parts[0]) && parts[0][0] == '\uFEFF')
                        {
                            parts[0] = parts[0].Substring(1);  // Remove the ByeOrderMark
                        }
                        OpCode opCode = (OpCode)int.Parse(parts[0].Trim());
                        string sender = parts[1];
                        string message = parts[2];

                        HandleOpCode(opCode, message, client);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
        finally
        {
            Console.WriteLine($"Client [{client.Username}] disconnected...");
            client.ClientSocket.Close();
            clients.Remove(client);
        }
    }
    private static void HandleOpCode(OpCode opCode, string message, Client client)
    {
        switch (opCode)
        {
            case OpCode.Connected:
                Console.WriteLine($"New client [{client.Username}] established connection...");
                break;
            case OpCode.Message:
                Console.WriteLine($"[{DateTime.Now}] [{client.Username}] sent: {message}");
                BroadcastMessage(client, message);
                break;
            case OpCode.Disconnect:
                Console.WriteLine($"Client [{client.Username}] requested disconnect");
                clients.Remove(client);
                client.ClientSocket.Close();
                break;
            default:
                Console.WriteLine("Unknown OpCode");
                break;
        }
    }

    private static void BroadcastMessage(Client sender, string message)
    {
        foreach (var client in clients)
        {
            if (sender.UID == client.UID) continue;
            if (client.ClientSocket.Connected)
            {
                try
                {
                    StreamWriter writer = new StreamWriter(client.ClientSocket.GetStream(), Encoding.UTF8);
                    writer.AutoFlush = true;
                    // OpCode 2 (Message) and message content
                    writer.WriteLine($"{(int)OpCode.Message}|{sender.Username}|{message}");
                    writer.AutoFlush = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }
    }

    public static void Main()
    {
        StartServer();
    }
}