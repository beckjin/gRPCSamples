using Google.Protobuf;
using Grpc.Core;
using GrpcStream;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Server
{
    public class GrpcStreamService : StreamTest.StreamTestBase
    {
        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="requestStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ClientStreamResponse> ClientStream(IAsyncStreamReader<ClientStreamRequest> requestStream, ServerCallContext context)
        {
            var i = 1;
            while (await requestStream.MoveNext())
            {
                Console.WriteLine(i++);
                using var fs = new FileStream(requestStream.Current.FilePath, FileMode.Append);
                requestStream.Current.Data.WriteTo(fs);
            }
            return new ClientStreamResponse { Success = true };
        }

        /// <summary>
        /// 读文件
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task ServerStream(ServerStreamRequest request, IServerStreamWriter<ServerStreamResponse> responseStream, ServerCallContext context)
        {
            using var fs = File.Open(request.FilePath, FileMode.Open);
            var leftSize = fs.Length;
            var buff = new byte[1048576]; // 1M 分批返回
            while (leftSize > 0)
            {
                var len = await fs.ReadAsync(buff);
                leftSize -= len;
                Console.WriteLine($"response {len} bytes");
                await responseStream.WriteAsync(new ServerStreamResponse
                {
                    Data = ByteString.CopyFrom(buff, 0, len)
                });
            }
        }

        /// <summary>
        /// 批量读文件
        /// </summary>
        /// <param name="requestStream"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task BidirectionalStream(IAsyncStreamReader<BidirectionalStreamRequest> requestStream, IServerStreamWriter<BidirectionalStreamResponse> responseStream, ServerCallContext context)
        {
            var i = 1;
            while (await requestStream.MoveNext())
            {
                Console.WriteLine(i++);
                using var fs = File.Open(requestStream.Current.FilePath, FileMode.Open);
                var leftSize = fs.Length;
                var buff = new byte[1048576]; // 1M 分批返回
                while (leftSize > 0)
                {
                    var len = await fs.ReadAsync(buff);
                    leftSize -= len;
                    Console.WriteLine($"response {requestStream.Current.FilePath} {len} bytes");
                    await responseStream.WriteAsync(new BidirectionalStreamResponse
                    {
                        FilePath = requestStream.Current.FilePath,
                        Data = ByteString.CopyFrom(buff, 0, len)
                    });
                }
            }
        }
    }
}
