using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Linq;

namespace Messaging_app
{
    class Class1
    {
        public static int i;

        public static TcpClient client = new TcpClient();


        public static string retry = "";

        public static bool shouldBeReading;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnExit);
            shouldBeReading = true;

            tryConnecting(); //connects to the server

            //Only keeps executing if connected to the server

            //Console.WriteLine("Connected to Server!\n\n");




            readData(); //keeps reading in data asynchonouysly 

            //Loop that keeps sending messages
            while (client.Connected)
            {

                string message = Console.ReadLine();
                if (message == "")
                {
                    Console.WriteLine("Please write something and then press enter to send");

                }
                else
                {
                    message = "Other Person: " + message;
                    int bytes = Encoding.ASCII.GetByteCount(message);
                    byte[] HowManyBytes = BitConverter.GetBytes(bytes);

                    var stream = client.GetStream();




                    byte[] bufferForMessage = Encoding.ASCII.GetBytes(message);


                    stream.Write(HowManyBytes, 0, HowManyBytes.Length);
                    stream.Write(bufferForMessage, 0, bufferForMessage.Length);
                    //Console.WriteLine("wrote " + bytes + " bytes");
                }



            }


            Console.Read();




        }

        /*public static void GotMessage(IAsyncResult res) //gets called asynchornously
        {
            object[] o = (object[])res.AsyncState;
            byte[] buf = (byte[])o[0];
            var stream = (NetworkStream)o[1];
            buf = buf.Where(b => b != 0).ToArray();


            stream.EndRead(res);


            string message = Encoding.ASCII.GetString(buf);

            

            try
            {
                
                string[] finalMessages = message.Split(":");
                string label = finalMessages[0];
                string content = finalMessages[1];

                if(content == "exitted")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Other person has disconnected :((");
                    Console.WriteLine("Closing app in 5 seconds");
                    Console.ForegroundColor = ConsoleColor.White;
                    client.Close();
                    Thread.Sleep(5000);
                    Environment.Exit(0);
                    
                }

                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(label + " : ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(content);
                    Console.ForegroundColor = ConsoleColor.White;
                    
                }
            }

            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ForegroundColor = ConsoleColor.White;
            }

        hbj


            
            
            i++;

            
        }*/

        public static void tryConnecting()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Trying to connect to the server...\n");
            Console.ForegroundColor = ConsoleColor.White;
            try
            {
                //client.Connect("messagingappserver.westus2.cloudapp.azure.com", 50002); 
                client.Connect("127.0.0.1", 50002);

            }
            catch //if client is opened when server is not running
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Server is currently not online");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Would you like to retry to connect? (y/n)");
                retry = Console.ReadLine(); //ask user if they want to retry to connect

                if (retry == "y" || retry == "Y" || retry == "yes" || retry == "Yes" || retry == "YES")
                {
                    tryConnecting();
                }

                else
                {
                    Console.WriteLine("Exitting Application...");
                    Thread.Sleep(500);
                    Environment.Exit(0); //if dont want to retry, then exit
                }
            }
        }

        public static async void readData()
        {


            await Task.Run(() =>
            {



                var stream = client.GetStream();

                //Console.WriteLine("only oncwe ");
                while (client.Connected)
                {
                    byte[] buf = new byte[50000];
                    byte[] numberOfBytes = new byte[4];


                    if (stream.DataAvailable && stream.CanRead)
                    {





                        stream.Read(numberOfBytes, 0, 4); //set 4 bytes to the numberOfBytes buffer



                        int number = BitConverter.ToInt32(numberOfBytes); //set int number to whatever the integer of the 4 bytes was


                        try
                        {
                            stream.Read(buf, 0, number); //read the rest of the stream (actaul messasge)
                        }
                        catch
                        {

                        }

                        buf = buf.Where(b => b != 0).ToArray();

                        string message = Encoding.ASCII.GetString(buf);
                        try
                        {
                            if (message == "exitted") //if other person disconnected and sent "exitted" then close application
                            {
                                stream.Close();

                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Other person has disconnected :((");
                                Console.WriteLine("Closing app in 5 seconds");
                                Console.ForegroundColor = ConsoleColor.White;
                                client.Client.Close();

                                client.Close();
                                Thread.Sleep(5000);
                                Environment.Exit(0);
                            }

                            else
                            {
                                string[] strings = message.Split(":");

                                string label = strings[0];
                                string content = strings[1];

                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.Write(label + " : ");
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write(content + "\n");
                                //Console.ForegroundColor = ConsoleColor.White;


                            }
                        }

                        catch
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(message);
                            Console.ForegroundColor = ConsoleColor.White;
                        }




                        Array.Clear(buf, 0, buf.Length);
                        Array.Clear(numberOfBytes, 0, numberOfBytes.Length);

                    }


                }
            });

        }


        static void OnExit(object sender, EventArgs e)
        {
            if (client.Connected)
            {
                try
                {

                    var stream = client.GetStream();
                    byte[] bufferForMessage = Encoding.ASCII.GetBytes("exitted");
                    int numberBytes = Encoding.ASCII.GetByteCount("exitted");
                    byte[] bytes = BitConverter.GetBytes(numberBytes);

                    stream.Write(bytes, 0, 4);
                    stream.Write(bufferForMessage);
                    client.Client.Close();
                    stream.Close();
                    client.Close();
                }

                catch
                {
                    client.Close();
                }


            }
        }
    }
}
