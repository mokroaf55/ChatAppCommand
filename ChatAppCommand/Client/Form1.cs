using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatClientApp
{
    public partial class ClientForm : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private Thread receiveThread;
        private bool isConnected;

        public ClientForm()
        {
            InitializeComponent();
            btnDisconnect.Enabled = false;
            btnSend.Enabled = false;
            txtMessage.Enabled = false;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                string serverIP = txtServerIP.Text;
                int port = int.Parse(txtPort.Text);
                string username = txtUsername.Text;

                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Будь ласка, введіть ім'я користувача");
                    return;
                }

                client = new TcpClient();
                client.Connect(serverIP, port);
                stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                string welcomeMessage = reader.ReadLine();
                AppendToChat(welcomeMessage);

                writer.WriteLine(username);

                isConnected = true;
                receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
                btnSend.Enabled = true;
                txtMessage.Enabled = true;
                txtServerIP.Enabled = false;
                txtPort.Enabled = false;
                txtUsername.Enabled = false;

                AppendToChat($"Підключено до сервера {serverIP}:{port}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка підключення: {ex.Message}");
            }
        }

        private void ReceiveMessages()
        {
            while (isConnected)
            {
                try
                {
                    string message = reader.ReadLine();
                    if (message == null) break;

                    if (InvokeRequired)
                    {
                        Invoke(new Action<string>(AppendToChat), message);
                    }
                    else
                    {
                        AppendToChat(message);
                    }
                }
                catch
                {
                    break;
                }
            }

            if (isConnected)
            {
                Disconnect();
                if (InvokeRequired)
                {
                    Invoke(new Action(() => AppendToChat("Відключено від сервера")));
                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void txtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SendMessage();
                e.Handled = true;
            }
        }

        private void SendMessage()
        {
            if (!isConnected || string.IsNullOrEmpty(txtMessage.Text))
                return;

            try
            {
                writer.WriteLine(txtMessage.Text);
                txtMessage.Clear();
            }
            catch (Exception ex)
            {
                AppendToChat($"Помилка відправки: {ex.Message}");
            }
        }
        private void AppendToChat(string message)
        {
            if (txtChat.InvokeRequired)
            {
                txtChat.Invoke(new Action<string>(AppendToChat), message);
            }
            else
            {
                txtChat.AppendText(message + Environment.NewLine);
                txtChat.ScrollToCaret();
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void Disconnect()
        {
            isConnected = false;

            try
            {
                writer?.Close();
                reader?.Close();
                stream?.Close();
                client?.Close();
                receiveThread?.Abort();
            }
            catch { }

            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
            btnSend.Enabled = false;
            txtMessage.Enabled = false;
            txtServerIP.Enabled = true;
            txtPort.Enabled = true;
            txtUsername.Enabled = true;

            AppendToChat("Відключено від сервера");
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }
    }
}