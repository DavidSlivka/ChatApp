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

class Program
{
    private static StreamReader reader;
    private static StreamWriter writer;

    public static void StartClient()
    {
        try
        {
            Client client = new Client(new TcpClient("127.0.0.1", 8888));
            NetworkStream stream = client.ClientSocket.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, Encoding.UTF8);
            writer.AutoFlush = true;

            Console.WriteLine("Connected to server...");
            client.SetUsername();

            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();

            SendMessage(OpCode.Connected, client.Username, "");

            while (true)
            {
                Console.Write("Enter a message: ");

                string? message = Console.ReadLine();

                if (message == "/exit") 
                {
                    SendMessage(OpCode.Disconnect, client.Username, "");
                    break;
                }
                if (!string.IsNullOrEmpty(message))
                {
                    // set cursor position to rewrite previous line
                    Console.SetCursorPosition(0, Console.CursorTop-1);

                    // Send a message with OpCode 2 (Message) and the actual message
                    SendMessage(OpCode.Message, client.Username, message);
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
    }

    private static void ReceiveMessages()
    {
        try
        {
            while (true)
            {
                string? receivedData = reader.ReadLine();

                if (receivedData == null)
                    break;

                // Parse to Opcode | Sender | Message
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
    }

    private static void HandleOpCode(OpCode opCode, string sender, string message)
    {
        ClearCurrentConsoleLine();
        switch (opCode)
        {
            case OpCode.Connected:
                Console.WriteLine($"New user {sender} entered the chat");
                break;
            case OpCode.Message:
                Console.WriteLine($"[{DateTime.Now}] [{sender}]: {message}");
                break;
            case OpCode.Disconnect:
                Console.WriteLine($"[{DateTime.Now}] [{sender}] Disconnected!");
                break;
            default:
                Console.WriteLine("[Server]: Message was not loaded properly");
                break;
        }
        Console.Write("Enter a message: ");
    }

    static void ClearCurrentConsoleLine()
    {
        int currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, currentLineCursor);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLineCursor);
    }

    public static void Main()
    {
        StartClient();
    }
}