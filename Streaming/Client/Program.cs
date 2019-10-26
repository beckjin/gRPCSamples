using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcStream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace T.GrpcStreamClient
{
    class Program
    {
        static readonly Dictionary<string, string> fileDic = new Dictionary<string, string>()
        {
                {@"d:\dapr\daprd_windows_amd64.zip", @"d:\dapr\daprd_windows_amd64_new.zip" },
                {@"d:\dapr\injector_windows_amd64.zip", @"d:\dapr\injector_windows_amd64_new.zip" },
        };

        static StreamTest.StreamTestClient client;

        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");

            client = new StreamTest.StreamTestClient(channel);

            await ClientStreamTestAsync();

            await ServerStreamTestAsync();

            await BidirectionalStreamTestAsync();

            Console.ReadKey();
        }

        static async Task ClientStreamTestAsync()
        {
            var filePath = @"d:\dapr\client.txt";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            using var call = client.ClientStream();
            var rand = new Random();
            for (int i = 0; i < 10; i++)
            {
                await call.RequestStream.WriteAsync(new ClientStreamRequest
                {
                    FilePath = filePath,
                    Data = ByteString.CopyFromUtf8(Guid.NewGuid().ToString() + Environment.NewLine)
                });
                await Task.Delay(rand.Next(200));
            }
            await call.RequestStream.CompleteAsync();
            var response = await call.ResponseAsync;
            Console.WriteLine(response.Success);
        }

        static async Task ServerStreamTestAsync()
        {
            var firstFile = fileDic.First();
            if (File.Exists(firstFile.Value))
            {
                File.Delete(firstFile.Value);
            }

            var result = client.ServerStream(new ServerStreamRequest
            {
                FilePath = firstFile.Key
            });
            var iterator = result.ResponseStream;
            using var fs = new FileStream(firstFile.Value, FileMode.Create);
            while (await iterator.MoveNext())
            {
                Console.WriteLine($"write to new file {iterator.Current.Data.Length} bytes");
                iterator.Current.Data.WriteTo(fs);
            }
        }

        static async Task BidirectionalStreamTestAsync()
        {
            foreach (var newFilePath in fileDic.Values)
            {
                if (File.Exists(newFilePath))
                {
                    File.Delete(newFilePath);
                }
            }

            using var call = client.BidirectionalStream();
            var responseTask = Task.Run(async () =>
            {
                var iterator = call.ResponseStream;
                while (await iterator.MoveNext())
                {
                    Console.WriteLine($"write to new file {fileDic[iterator.Current.FilePath]} {iterator.Current.Data.Length} bytes");
                    using var fs = new FileStream(fileDic[iterator.Current.FilePath], FileMode.Append);
                    iterator.Current.Data.WriteTo(fs);
                }
            });

            var rand = new Random();
            foreach (var item in fileDic)
            {
                await call.RequestStream.WriteAsync(new BidirectionalStreamRequest
                {
                    FilePath = item.Key
                });
                await Task.Delay(rand.Next(200));
            }
            await call.RequestStream.CompleteAsync();
            await responseTask;
        }
    }
}
