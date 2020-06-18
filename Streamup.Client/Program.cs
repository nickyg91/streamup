using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Camera;

namespace Streamup.Client
{
    public class Program
    {
        private static CameraVideoSettings _videoSettings;
        private static Socket _clientSocket;
        public static async Task Main(string[] args)
        {
            _videoSettings = new CameraVideoSettings
            {
                CaptureWidth = 1280,
                CaptureHeight = 720,
                CaptureDisplayPreview = false,
                CaptureExposure = CameraExposureMode.Auto,
                ImageFlipVertically = true,
                CaptureTimeoutMilliseconds = 5000
            };
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Udp);
            try
            {
                await _clientSocket.ConnectAsync(IPAddress.Parse("192.168.1.196"), 9000);
                var buffer = new MemoryStream(new byte[4096]);
                var byteCount = 0;
                Pi.Camera.OpenVideoStream(_videoSettings, (byte[] bytes) =>
                {
                    if (byteCount == 4096)
                    {
                        _clientSocket.SendAsync(buffer.GetBuffer(), SocketFlags.Broadcast);
                        buffer.SetLength(0);
                    }
                    byteCount += bytes.Length;
                    buffer.Write(bytes);
                },
                () =>
                {
                    _clientSocket.Close();
                    Console.WriteLine("Connection stopped to server.");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Hit enter to exit...");
                Console.ReadLine();
            }

        }


    }
}
