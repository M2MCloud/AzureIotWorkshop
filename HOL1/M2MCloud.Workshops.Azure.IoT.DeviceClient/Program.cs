﻿using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Shared;

namespace M2MCloud.Workshops.Azure.IoT.DeviceClient
{
    class Program
    {
        //attendee to change
        private const string DeviceConnectionString = "HostName=M2M-Office.azure-devices.net;DeviceId=000356865805524;SharedAccessKey=myLoxhifW+dri1KFjkWpXvt70ODd/0A/WJKTnElN/M8=";
        //attendee to change
        private const string IoTHubStorageAccountName = "fileupload";
        //attendee to change
        private static String deviceId = "000356865805524";

        private static Microsoft.Azure.Devices.Client.DeviceClient deviceClient;
        private static string pressAnyKeyToReturnToTheMainMenu = "\nPress any key to return to the main menu";

        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    InitialiseDevice().Wait();
                    Console.Clear();
                    ShowDeviceMenu().Wait();
                    //ReceiveCommands(deviceClient).Wait();
                    Console.ReadKey();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something bad happened unexpectedly.\n{ex.Message}");
                    Console.WriteLine(pressAnyKeyToReturnToTheMainMenu);
                    Console.ReadKey();
                }
            }


        }

        private static async Task InitialiseDevice()
        {
            deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString, TransportType.Mqtt_WebSocket_Only);
            await deviceClient.OpenAsync();
            await deviceClient.SetMethodHandlerAsync("stop", StopThePumpNow, null);
            await deviceClient.SetMethodHandlerAsync("start", StartThePumpNow, null);
        }

        static async Task ShowDeviceMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("*** Device - M2M Model X1 ***");
                Console.WriteLine("\nDevice Menu\n");
                Console.WriteLine("1. Send Device to Cloud Message(s)");
                Console.WriteLine("2. Read Device Twin Properties");
                Console.WriteLine("3. Write Device Twin Reported Properties");
                Console.WriteLine("4. Send MASSIVE Device Message sample");
                Console.WriteLine("5. Receive Cloud to Device Message");

                Console.WriteLine("\nReady for Direct Method");

                var menuItemResult = Console.ReadKey();
                if (menuItemResult.KeyChar.ToString() == "1")
                    await SendEvent();
                if (menuItemResult.KeyChar.ToString() == "2")
                    await ReadDeviceTwin();
                if (menuItemResult.KeyChar.ToString() == "3")
                    await WriteDeviceTwin();
                if (menuItemResult.KeyChar.ToString() == "4")
                    await UploadDeviceMessages();
                if (menuItemResult.KeyChar.ToString() == "5")
                    await ReceiveCommands();
            }
        }

        private static async Task UploadDeviceMessages()
        {
            Console.WriteLine("What is the full file path of the massive Device Message samples?");
            var path = Console.ReadLine();
            path = path.Replace("\"", "");
            using (var sourceData = new FileStream(path, FileMode.Open))
            {
                await deviceClient.UploadToBlobAsync(IoTHubStorageAccountName, sourceData);
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
            Console.WriteLine("\nWhat is Key the Device should update?");
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
            var numberMessagesToSend = int.Parse(numberMessagesToSendString);

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

        //attendee to change - apply your own domain if you like and create a custom JSON shape that has:
        //1. A numerical value e.g. "oilLevel: 99"
        //2. A digital value e.g. "pumpEngineActive: true"
        private static string ConstructDataSample(int count)
        {
            var rnd = new Random();
            double temperature;
            double humidity;
            temperature = rnd.Next(20, 35);
            humidity = rnd.Next(60, 80);
            var dataBuffer = $"{{\"deviceId\":\"{deviceId}\",\"messageId\":{count},\"temperature\":{temperature},\"humidity\":{humidity}, \"motorActive\":true}}";
            return dataBuffer;
        }

        static async Task ReceiveCommands()
        {
            Console.WriteLine("\nDevice waiting for commands from IoTHub. Press escape to return to the main menu.\n");
            Message receivedMessage = null;

            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                try
                {
                    receivedMessage = await deviceClient.ReceiveAsync(TimeSpan.FromSeconds(1));
                    if (receivedMessage == null) continue;
                    var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    Console.WriteLine("\t{0}> Received message: {1}", DateTime.Now.ToLocalTime(), messageData);
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
            Console.WriteLine($"\nThe pump has been switched off. {reason}");
            var status = $"{{\"status\":\"Device confirming pump has been switched off at {DateTime.UtcNow}\"}}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(status), 200));
        }

        static Task<MethodResponse> StartThePumpNow(MethodRequest methodRequest, object userContext)
        {
            var reason = methodRequest.DataAsJson == "null" ? "" : $"Reason:{methodRequest.DataAsJson}";
            Console.WriteLine($"\nThe pump has been switched on. {reason}");
            var status = $"{{\"status\":\"Device confirming pump has been switched on at {DateTime.UtcNow}\"}}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(status), 200));
        }
    }
}