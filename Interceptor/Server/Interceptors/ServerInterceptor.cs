using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Threading.Tasks;

namespace Server.Interceptors
{
    public class ServerInterceptor : Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            Console.WriteLine("服务端执行开始");
            var response = await continuation(request, context);
            Console.WriteLine("服务端执行结束");
            return response;
        }
    }
}
