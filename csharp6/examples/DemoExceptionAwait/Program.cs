using System;
using System.IO;
using System.Threading.Tasks;

namespace DemoExceptionAwait
{
    class Program
    {
        // 注意：C# 7.1 以後才支援 async Main。
        public static async Task Main() 
        {
            var s = await HttpGetStrAsync("http://localhost:12345");
            Console.WriteLine(s);
        }

        static async Task<string> HttpGetStrAsync(string url)
        {
            var client = new System.Net.Http.HttpClient();
            var streamTask = client.GetStringAsync(url);
            try
            {
                var responseText = await streamTask;
                return responseText;
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                await LogAsync($"失敗: {ex.Message}");
                return ex.Message;
            }
            finally
            {
                client.Dispose();
                await LogAsync("離開 HttpGHetAsync() 方法。");
            }
        }

        static async Task LogAsync(string s)
        {
            // 一般來說，此函式不應該再拋出任何例外。
            await File.AppendAllTextAsync(@"c:\temp\log.txt", s);
        }
    }
}
