using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FriendlyRemoteClientWinForms
{
    public partial class ReceiverForm : Form
    {
        private int _messageSize = 4096;
        private Queue<byte[]> _drawQueue = new Queue<byte[]>();

        private async Task ReceiveImage()
        {
            Uri uri = new Uri("wss://localhost:7263/receiver");

            using (ClientWebSocket ws = new ClientWebSocket())
            {
                ws.ConnectAsync(uri, default).GetAwaiter().GetResult();


                List<byte> incomingMessage = new List<byte>();
                byte[] bytes = new byte[_messageSize];
                while (true)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(bytes), CancellationToken.None);
                    if (result.EndOfMessage)
                    {
                        
                        incomingMessage.AddRange(bytes.Take(result.Count));

                        // draw!
                        _drawQueue.Enqueue(incomingMessage.ToArray());
                        incomingMessage.Clear();
                    }
                    else
                    {
                        incomingMessage.AddRange(bytes);
                    }
                }
            }
        }

        private void DrawThread()
        {
            while (true)
            {
                
                if (_drawQueue.Count > 0)
                {
                    using (var ms = new MemoryStream(_drawQueue.Dequeue()))
                    {
                        var img = new Bitmap(ms);

                        // var bmp = ScreenCapture.ResizeImage(img, picWidth, picHeight);
                        if (this.pictureBox1 != null)
                            this.pictureBox1.Image = img;
                    }
                }
                else
                {
                    Thread.Sleep(60);
                }
            }
        }

        public ReceiverForm()
        {
            InitializeComponent();
            var pictureBox = this.pictureBox1;
            this.pictureBox1.Width = ClientSize.Width;
            this.pictureBox1.Height = ClientSize.Height;
            Task.Run(ReceiveImage);
            Task.Run(DrawThread);
            // Thread t = new Thread(ReceiveImage);
            // Thread t2 = new Thread(DrawThread);
            // t.Start();
            // t2.Start();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.pictureBox1.Width = ClientSize.Width;
            this.pictureBox1.Height = ClientSize.Height;
        }
    }
}