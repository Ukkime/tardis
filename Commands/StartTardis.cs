using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using tardis.ui;
using ui;
using Newtonsoft.Json;
using System.Diagnostics;

namespace tardis.Commands
{
    internal class StartTardis
    {
        private Overlay _ovl;
        private System.Threading.Timer _timer;
        public static List<Dictionary<string, string>> states = new List<Dictionary<string, string>>();

        public StartTardis() { }

        public Form Run()
        {
            this._ovl = new Overlay();
            TimerCallback tcb = CheckOverlayStatus;

            Task.Run(() => SendBroadcast(61456));
            Task.Run(() => ReceiveBroadcast(61456));

            _timer = new System.Threading.Timer(tcb, null, 0, 1000); // Comprueba cada minuto
            return this._ovl;
        }

        private async Task SendBroadcast(int port)
        {

            using (var client = new UdpClient())
            {

                client.EnableBroadcast = true;

                while (true)
                {
                    string nodeName = this._ovl.nodeName;
                    string status = this._ovl.statusValue.ToString();
                    DateTime dt = DateTime.Now;
                    var state = new Dictionary<string, string>
                    {
                        { "status", status },
                        { "id", nodeName },
                        { "datetime", dt.ToString() }
                    };

                    var message = JsonConvert.SerializeObject(state);
                    var bytes = Encoding.ASCII.GetBytes(message);

                    await client.SendAsync(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, port));

                    await Task.Delay(TimeSpan.FromSeconds(30)); // Espera 10 segundos antes de enviar el siguiente aviso
                }
            }
        }

        private void CheckOverlayStatus(object state)
        {
            if (this._ovl.statusValue == StatusEnum.Concentrado && DateTime.Now - this._ovl.lastStatusUpdate > TimeSpan.FromMinutes(90))
            {
                this._ovl.SetStatus(StatusEnum.Descanso);
                System.Windows.Forms.MessageBox.Show("Llevas 1:30h concentrado, considera descansar un poco o dedicar unos minutos a otras tareas.");
            }
        }
        private async Task ReceiveBroadcast(int port)
        {

            using (var client = new UdpClient(port))
            {
                while (true)
                {
                    try
                    {
                        states.Clear();
                        var result = await client.ReceiveAsync();
                        var message = Encoding.ASCII.GetString(result.Buffer);
                        var state = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                        state.Add("updatetime", DateTime.Now.ToString());

                        states.Add(state);

                        this._ovl.UpdateNeighbors(states);
                    }
                    catch (Exception ex)
                    {
                        // nothing to do
                    }
                }
            }

        }
    }
}