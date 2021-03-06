﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;


namespace M2MCloud.Workshops.Azure.IoT.NetCore
{
    class Program
    {
        private static readonly Random Rand = new Random();
        //attendee to change #1
        private const string DeviceConnectionString = "YOUR CONNECTION STRING";
        //attendee to change #2
        private static string deviceId = "956584572537578";
        private static Microsoft.Azure.Devices.Client.DeviceClient deviceClient;
        private static string pressAnyKeyToReturnToTheMainMenu = "\nPress any key to return to the main menu";
        private static ConsoleColor originalConsoleColour;

        static async Task Main(string[] args)
        {
            while (true)
            {
                try
                {
                    await InitialiseDevice();
                    InitialiseApp();
                    Console.Clear();
                    await ShowDeviceMenu();
                    Console.ReadKey();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something bad happened unexpectedly.\n{ex.Message}");
                    Console.WriteLine(pressAnyKeyToReturnToTheMainMenu);
                    Console.ReadKey();
                    await deviceClient?.CloseAsync();
                }
            }
        }

        private static void InitialiseApp()
        {
            originalConsoleColour = Console.ForegroundColor;
        }

        private static async Task InitialiseDevice()
        {
            deviceClient = Microsoft.Azure.Devices.Client.DeviceClient.CreateFromConnectionString(DeviceConnectionString, TransportType.Mqtt_WebSocket_Only);
            await deviceClient.OpenAsync();
            await deviceClient.SetMethodHandlerAsync("stop", StopThePumpNow, null);
            await deviceClient.SetMethodHandlerAsync("start", StartThePumpNow, null);
        }

        static async Task ShowDeviceMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("*** CENSIS Workshop Device - M2M Model X2711 ***");
                Console.WriteLine("\nDevice Menu\n");
                Console.WriteLine("1. Send Device to Cloud Message(s)");
                Console.WriteLine("2. Send large Device Message file");
                Console.WriteLine("3. Read Device Twin Properties");
                Console.WriteLine("4. Write Device Twin Reported Properties");
                Console.WriteLine("5. Receive Cloud to Device Message");

                Console.WriteLine("\nReady for Direct Method");

                var menuItemResult = Console.ReadKey();
                switch (menuItemResult.KeyChar.ToString())
                {
                    case "1":
                        await SendEvent();
                        break;
                    case "2":
                        await UploadDeviceFile();
                        break;
                    case "3":
                        await ReadDeviceTwin();
                        break;
                    case "4":
                        await WriteDeviceTwin();
                        break;
                    case "5":
                        await ReceiveCommands();
                        break;
                    default:
                        Console.WriteLine("Sorry, don't recognise that menu item number.");
                        break;
                }
            }
        }

        private static async Task UploadDeviceFile()
        {
            Console.WriteLine("\nWhat is the full file path of the massive Device Message samples?");
            var path = Console.ReadLine();
            if (path == null)
                throw new InvalidOperationException("Please input a valid file path");
            path = path.Replace("\"", "");
            var file = new FileInfo(path);

            if (!file.Exists)
                throw new InvalidOperationException("Can't find that file. ");

            using (var sourceData = new FileStream(path, FileMode.Open))
            {
                await deviceClient.UploadToBlobAsync(file.Name, sourceData);
            }
        }

        static async Task ReadDeviceTwin()
        {
            var twin = await deviceClient.GetTwinAsync();
            Console.WriteLine("\nDevice *Desired* Properites");
            Console.WriteLine($"\n{twin.Properties.Desired}");
            Console.WriteLine("\nDevice *Reported* Properites");
            Console.WriteLine($"\n{twin.Properties.Reported}");
            Console.WriteLine(pressAnyKeyToReturnToTheMainMenu);
            Console.ReadKey();
        }

        static async Task WriteDeviceTwin()
        {
            Console.WriteLine("\nWhat is the name of the Key for the Device Twin Property?");
            var key = Console.ReadLine();

            Console.WriteLine("\nWhat is Value of the Key?");
            var value = Console.ReadLine();

            var twinProps = new TwinCollection { [key] = value };
            await deviceClient.UpdateReportedPropertiesAsync(twinProps);
            Console.WriteLine(pressAnyKeyToReturnToTheMainMenu);
            Console.ReadKey();
        }

        static async Task SendEvent()
        {
            Console.WriteLine("\nHow many messages to do you want to send?");
            var numberMessagesToSendString = Console.ReadLine();
            var numberMessagesToSend = int.Parse(numberMessagesToSendString ?? throw new InvalidOperationException("Please input a number"));

            Console.WriteLine("\nDevice sending {0} messages to IoTHub...\n", numberMessagesToSend);

            for (var count = 0; count < numberMessagesToSend; count++)
            {
                var dataBuffer = ConstructDataSample(count);
                var eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
                Console.WriteLine($"\n> Sending message: {count}, Data: [{dataBuffer}]");
                await deviceClient.SendEventAsync(eventMessage);
            }
            Console.WriteLine(pressAnyKeyToReturnToTheMainMenu);
            Console.ReadKey();
        }

        //attendee to change (optional) - apply your own domain if you like and create a custom JSON shape that has:
        //1. A numerical value e.g. "oilLevel: 99"
        //2. A digital value e.g. "pumpEngineActive: true"
        private static string ConstructDataSample(int count)
        {
            double temperature = Rand.Next(20, 35);
            double humidity = Rand.Next(60, 80);
            //attendee to change #3 (optional)
            var dataBuffer = $"{{\"deviceId\":\"{deviceId}\",\"messageId\":{count},\"temperature\":{temperature},\"humidity\":{humidity}, \"motorActive\":true}}";
            return dataBuffer;
        }

        static async Task ReceiveCommands()
        {
            Console.WriteLine("\nDevice waiting for commands from IoTHub. Press escape to return to the main menu.");
            Message receivedMessage = null;

            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                try
                {
                    receivedMessage = await deviceClient.ReceiveAsync(TimeSpan.FromSeconds(1));
                    if (receivedMessage == null) continue;
                    var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    Console.WriteLine("\n\t{0}> Received message: {1}", DateTime.Now.ToLocalTime(), messageData);
                    if (receivedMessage.Properties.Any())
                    {
                        Console.WriteLine("\tMessage Properties");
                        foreach (var receivedMessageProperty in receivedMessage.Properties)
                        {
                            Console.WriteLine(
                                $"\t - Key:{receivedMessageProperty.Key} Value: {receivedMessageProperty.Value}");
                        }
                    }
                    await deviceClient.CompleteAsync(receivedMessage);
                }
                finally
                {
                    receivedMessage?.Dispose();
                }
            }
        }

        //attendee to change - you can call this method whatever you like according to you domain - just make 
        //sure it's reflected in the Direct Method invocation
        static Task<MethodResponse> StopThePumpNow(MethodRequest methodRequest, object userContext)
        {
            var reason = methodRequest.DataAsJson == "null" ? "" : $"Reason:{methodRequest.DataAsJson}";
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nThe pump has been switched off. {reason}");
            var status = $"{{\"status\":\"Device confirming pump has been switched off at {DateTime.UtcNow}\"}}";
            Console.ForegroundColor = originalConsoleColour;
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(status), 200));
        }

        static Task<MethodResponse> StartThePumpNow(MethodRequest methodRequest, object userContext)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            var reason = methodRequest.DataAsJson == "null" ? "" : $"Reason:{methodRequest.DataAsJson}";
            Console.WriteLine($"\nThe pump has been switched on. {reason}");
            var status = $"{{\"status\":\"Device confirming pump has been switched on at {DateTime.UtcNow}\"}}";
            Console.ForegroundColor = originalConsoleColour;
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(status), 200));
        }

    }
}
