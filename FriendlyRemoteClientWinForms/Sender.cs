using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FriendlyRemoteClientWinForms
{
    public partial class Form1 : Form
    {
        private int _messageSize = 4096;

        private Queue<byte[]> _imageRefreshQueue = new Queue<byte[]>();
        private Queue<byte[]> _messageQueue = new Queue<byte[]>();

        public static byte[] ImageToByte(Bitmap image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }


        public Form1()
        {
            InitializeComponent();
            // Should we be using Thread or task here I still don't know?
            Task.Run(ConsumeImageQueue);
            Task.Run(SendImage);
            Task.Run(EnqueueScreenImages);
            // Thread consume = new Thread(ConsumeImageQueue);
            // Thread send = new Thread(SendImage);
            // Thread t = new Thread(EnqueueScreenImages);
            // consume.Start();
            // t.Start();
            // send.Start();
        }

        private async void SendImage()
        {
            Uri uri = new Uri("wss://localhost:7263/sender");

            using (ClientWebSocket ws = new ClientWebSocket())
            {
                ws.ConnectAsync(uri, default).GetAwaiter().GetResult();

                while (true)
                {
                    if (_messageQueue.Count > 0)
                    {
                        byte[] ba = _messageQueue.Dequeue();
                        byte[] sendBuffer = new byte[_messageSize];
                        using (var ms = new MemoryStream(ba, false))
                        {
                            for (int i = 0; ba.Length > _messageSize * i; i++)
                            {
                                var lastMessage = ba.Length <= _messageSize * (i + 1);

                                await ms.ReadAsync(sendBuffer, 0, _messageSize);
                                await ws.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Binary, lastMessage,
                                    CancellationToken.None);
                            }
                        }
                    }
                    else
                    {
                        await Task.Delay(500);
                    }
                }
            }
        }

        private async void ConsumeImageQueue()
        {
            

                // var bytes = new byte[messageSize];
                // var arrSeg = new ArraySegment<byte>(bytes);
                // var result = ws.ReceiveAsync(arrSeg, default).GetAwaiter().GetResult();
                // string res = Encoding.UTF8.GetString(bytes, 0, result.Count);


                while (true)
                {
                    Thread.Sleep(40);

                    if (_imageRefreshQueue.Count > 0)
                    {
                        byte[] ba = _imageRefreshQueue.Dequeue();

                        // richTextBox1.Text = $"{ba.Length.ToString()} number of bytes, items in queue: {_messageQueue.Count}";
                        //


                        // await ws.SendAsync(new ArraySegment<byte>(ba), WebSocketMessageType.Binary, true,
                        //     CancellationToken.None);
                        //

                        // var result = await ws.ReceiveAsync();


                        using (var ms = new MemoryStream(ba))
                        {
                            var img = new Bitmap(ms);
                            if (this.pictureBox1 != null)
                                this.pictureBox1.Image = img;
                        }
                    }
                }

        }

        private void EnqueueScreenImages()
        {
            while (true)
            {
                try
                {
                    var pictureBox = this.pictureBox1;
                    this.pictureBox1.Width = ClientSize.Width;
                    this.pictureBox1.Height = ClientSize.Height;
                    var picWidth = this.pictureBox1.Width;
                    var picHeight = this.pictureBox1.Height;

                    Thread.Sleep(40);

                    var bmp = ScreenCapture.ResizeImage(ScreenCapture.CaptureDesktop(), picWidth, picHeight);
                    var image = ImageToByte(bmp, ImageFormat.Bmp);
                    _imageRefreshQueue.Enqueue(image);
                    _messageQueue.Enqueue(image);
                }
                catch (Exception e)
                {
                }
            }
        }

        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("User32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            // throw new System.NotImplementedException();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}