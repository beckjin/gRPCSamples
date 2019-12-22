using Grpc.Core;
using Grpc.Core.Interceptors;
using System;

namespace Client.Interceptors
{
    public class ClientInterceptor : Interceptor
    {
        /// <summary>
        /// 同步简单调用
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            Console.WriteLine("客户端调用执行开始");
            var response = continuation(request, context);
            Console.WriteLine("客户端调用执行结束");
            return response;

        }

        /// <summary>
        /// 异步简单调用
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            Console.WriteLine("客户端调用执行开始");
            var responseCon = continuation(request, context);
            var response = new AsyncUnaryCall<TResponse>(responseCon.ResponseAsync, responseCon.ResponseHeadersAsync, responseCon.GetStatus, responseCon.GetTrailers, responseCon.Dispose);
            Console.WriteLine("客户端调用执行结束");
            return response;
        }
    }
}
