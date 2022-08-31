using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {
        public static TcpListener server;
        public static bool shouldListen;
        public static bool checkingOn;
        public static TcpClient firstClient;
        public static TcpClient secondClient;
        static void Main(string[] args)
        {

            Console.WriteLine("Started to listen");
            
            server = new TcpListener(IPAddress.Any, 50002);
            
            server.Start();
            
            Console.WriteLine("Waiting for 2 clients to connect");



            StartLooking();
            


            Console.Read();
            
        }

        public static void sendToClients(string message, int whichStream)
        {
            
            byte[] buf = Encoding.ASCII.GetBytes(message);
            int count = Encoding.ASCII.GetByteCount(message);
            byte[] byteCount = BitConverter.GetBytes(count);

            if (whichStream == 1) //first client only
            {
                var firstStream = firstClient.GetStream();
                firstStream.Write(byteCount);
                firstStream.Write(buf, 0, buf.Length);
                Console.WriteLine("ONE");
              

            }
            
            else if(whichStream == 2) //second client only
            {
                var secondStream = secondClient.GetStream();
                secondStream.Write(byteCount);
                secondStream.Write(buf, 0, buf.Length);
                Console.WriteLine("TWO");
            }

            else if(whichStream == 0) //both clients
            {
                var firstStream = firstClient.GetStream();
                firstStream.Write(byteCount);
                firstStream.Write(buf);
                var secondStream = secondClient.GetStream();
                secondStream.Write(byteCount);
                secondStream.Write(buf, 0, buf.Length);
                Console.WriteLine("ZERO");
            }
            
        }

        public static async void ReadFromOne()
        {
            await Task.Run(() =>
            {
               byte[] messageBytes = new byte[4]; 
               byte[] buf = new byte[50000];
               var firstStream = firstClient.GetStream();
               var secondStream = secondClient.GetStream();

                while (firstClient.Connected)
               {
                   
                   try
                   {
                       if (firstStream.DataAvailable && firstStream.CanRead)
                       {

                            firstStream.Read(messageBytes, 0 , 4);
                            firstStream.Read(buf);
                           
                            int howMany = BitConverter.ToInt32(messageBytes);

                            secondStream.Write(messageBytes, 0, 4);
                            secondStream.Write(buf, 0, howMany);
                            Array.Clear(buf, 0, buf.Length);
                            Array.Clear(messageBytes, 0, messageBytes.Length);
                           
                       }
                   }
                   catch
                   {

                   }
                   
               }
        
           });
        }

        public static void DoneWithReadingFirst(IAsyncResult res)
        {
            byte[] whatFirstSaid = (byte[])res.AsyncState;
            string message = Encoding.ASCII.GetString(whatFirstSaid);
            


           
        }



        public static async void ReadFromTwo()
        {
            await Task.Run(() =>
            {
                byte[] messageBytes = new byte[4];
                byte[] buf = new byte[50000];
                var firstStream = firstClient.GetStream();
                var secondStream = secondClient.GetStream();

                while (firstClient.Connected)
                {

                    try
                    {
                        if (secondStream.DataAvailable && secondStream.CanRead)
                        {

                            secondStream.Read(messageBytes, 0, 4);
                            secondStream.Read(buf);

                            int howMany = BitConverter.ToInt32(messageBytes);

                            firstStream.Write(messageBytes, 0, 4);
                            firstStream.Write(buf, 0, howMany);
                            Array.Clear(buf, 0, buf.Length);
                            Array.Clear(messageBytes, 0, messageBytes.Length);

                        }
                    }
                    catch
                    {

                    }

                }

            });
        }

        public static void DoneWithReadingSecond(IAsyncResult res)
        {
            byte[] whatSecondSaid = (byte[])res.AsyncState;

            string message = Encoding.ASCII.GetString(whatSecondSaid);
           
        }


        public static async void StartCheckingForDisconnections()  //happening twice
        {
            
            await Task.Run(() =>
            {
                

                byte[] buffer = new byte[0];
                int i = 0;
                while (true)
                {
                    
                    if (firstClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        if (firstClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            // Client disconnected
                            Console.WriteLine("clients disconnected");
                            firstClient.Close();
                            secondClient.Close();
                            StartLooking();
                            break;
                            
                            

                        }
                        else
                        {
                           
                        }
                    }
                    else
                    {
                        Console.WriteLine("try block #: " + i);
                        i++;
                    }
                    Thread.Sleep(500);
                }
            });
        }

        public static void StartLooking()
        {
            Console.WriteLine("started");
            firstClient = server.AcceptTcpClient();
            Console.WriteLine("connected to one - IPADRESS: " + firstClient.Client.RemoteEndPoint);
            sendToClients("Connected to the server!, waiting for another client to connect...\n", 1);

            secondClient = server.AcceptTcpClient();
            Console.WriteLine("connected to both");
            sendToClients("Both clients connected. Begin Chatting! -- (type your message and press enter to send!)\n", 0);

            ReadFromOne();
            ReadFromTwo();
           
           
            StartCheckingForDisconnections();
            
            
            


        }
    }

    
}
