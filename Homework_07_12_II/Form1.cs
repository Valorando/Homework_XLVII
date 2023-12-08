using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace Homework_07_12_II
{
    public partial class Form1 : Form
    {
        private Socket c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public Form1()
        {
            InitializeComponent();
            textBox1.Enabled = false;
            button1.Enabled = false;
        }

        private async Task ConnectToServer()
        {
            try
            {
                IPAddress ip = IPAddress.Parse("127.0.0.1");
                int port = 8888;

                await c.ConnectAsync(new IPEndPoint(ip, port));
                listBox1.Items.Add("Вы присоединились к игре");

                string nickname = textBox2.Text;
                byte[] nicknameData = Encoding.UTF8.GetBytes(nickname);
                await c.SendAsync(new ArraySegment<byte>(nicknameData), SocketFlags.None);

                _ = ReceiveMessages();
            }
            catch (Exception ex)
            {
                listBox1.Items.Add(ex.Message);
                textBox1.Enabled = false;
                button1.Enabled = false;
                textBox2.Enabled = true;
                button2.Enabled = true;
            }
        }

        private async Task ReceiveMessages()
        {
            try
            {
                while (true)
                {
                    if (!c.Connected)
                    {
                        break;
                    }

                    byte[] data = new byte[256];
                    int bytesRead = await c.ReceiveAsync(new ArraySegment<byte>(data), SocketFlags.None);
                    string message = Encoding.UTF8.GetString(data, 0, bytesRead);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    listBox1.Items.Add(message);
                }
            }
            catch (Exception ex)
            {
                listBox1.Items.Add(ex.Message);
            }
            finally
            {
                listBox1.Items.Add("Игра завершена");
                textBox1.Enabled = false;
                button1.Enabled = false;
                textBox2.Enabled = false;
                button2.Enabled = false;
            }
        }

        private async Task SendMessage()
        {
            try
            {
                string message = textBox1.Text;
                byte[] data = Encoding.UTF8.GetBytes(message);
                await c.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
            }
            catch (Exception ex)
            {
                listBox1.Items.Add(ex.Message);
                textBox2.Enabled = true;
                button2.Enabled = true;
                textBox1.Enabled = false;
                button1.Enabled = false;
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await ConnectToServer();
            textBox1.Enabled = true;
            button1.Enabled = true;
            textBox2.Enabled = false;
            button2.Enabled = false;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await SendMessage();
            textBox1.Clear();
        }
    }
}
