using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using client;

public enum OpCode
{
    Connected=1,
    Message=2,
    Disconnect=3
}

class Client
{
    private static TcpClient client;
    private static StreamReader reader;
    private static StreamWriter writer;

    public static void StartClient()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 8888);
            NetworkStream stream = client.GetStream();
            reader = new StreamReader(stream, System.Text.Encoding.UTF8);
            writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
            writer.AutoFlush = true;

            Console.WriteLine("Connected to server...");
            var username = Console.ReadLine();
            SendMessage(OpCode.Connected, username, username);

            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();

            while (true)
            {
                string message = Console.ReadLine();
                if (!string.IsNullOrEmpty(message))
                {
                    // Send a message with OpCode 2 (Message) and the actual message
                    SendMessage(OpCode.Message, username, message);
                }
                if (message == "/exit") {
                    receiveThread.Abort();
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
    }

    private static void SendMessage(OpCode opCode, string sender, string message)
    {
        writer.WriteLine($"{(int)opCode}|{sender}|{message}"); // OpCode|Sender|Message format
        writer.AutoFlush = true;
    }

    private static void ReceiveMessages()
    {
        try
        {
            while (true)
            {
                string receivedData = reader.ReadLine();

                if (receivedData == null)
                    break;

                // Parse OpCode and message
                string[] parts = receivedData.Split('|');
                if (parts.Length >= 2)
                {
                    if (!string.IsNullOrEmpty(parts[0]) && parts[0][0] == '\uFEFF')
                    {
                        parts[0] = parts[0].Substring(1);  // Remove the ByteOrderMark
                    }
                    OpCode opCode = (OpCode)int.Parse(parts[0].Trim());
                    string sender = parts[1];
                    string message = parts[2];

                    HandleOpCode(opCode, sender, message);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
        finally
        {
            client.Close();
        }
    }

    private static void HandleOpCode(OpCode opCode, string sender, string message)
    {
        switch (opCode)
        {
            case OpCode.Connected:
                Console.WriteLine($"New user {sender} entered the chat");
                break;
            case OpCode.Message:
                Console.WriteLine($"[{DateTime.Now}] [{sender}]: {message}");
                break;
            default:
                Console.WriteLine("Unknown OpCode");
                break;
        }
    }

    public static void Main()
    {
        StartClient();
    }
}