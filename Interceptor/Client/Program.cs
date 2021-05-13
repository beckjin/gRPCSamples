using Client.Interceptors;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Server;
using System;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("http://localhost:3333");
            var invoker = channel.Intercept(new ClientInterceptor());
            var client = new Greeter.GreeterClient(invoker);
            var reply = await client.SayHelloAsync(new HelloRequest { Name = "Beck" });
            Console.WriteLine(reply.Message);
            Console.ReadKey();
        }
    }
}
