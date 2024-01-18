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
using Microsoft.Extensions.Configuration;
using System.IO;

namespace tardis.Commands
{
    internal class StartUDPTardis
    {
        private Overlay _ovl;
        private IConfiguration _config;
        private System.Threading.Timer _timer;
        public static List<Dictionary<string, string>> states = new List<Dictionary<string, string>>();

        public StartUDPTardis(IConfiguration config) {
            this._config = config;
        }

        public Form Run()
        {
            this._ovl = new Overlay(this._config);
            TimerCallback tcb = CheckOverlayStatus;

            Task.Run(() => SendBroadcast(Int32.Parse(_config["UDPSettings:port"])));
            Task.Run(() => ReceiveBroadcast(Int32.Parse(_config["UDPSettings:port"])));

            _timer = new System.Threading.Timer(tcb, null, 0, Int32.Parse(_config["UDPSettings:pomodoroFocusCheckFrequency"])); // Comprueba cada minuto
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
                        { "name", nodeName },
                        { "datetime", dt.ToString() }
                    };

                    var message = JsonConvert.SerializeObject(state);
                    var bytes = Encoding.ASCII.GetBytes(message);

                    await client.SendAsync(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, port));

                    await Task.Delay(TimeSpan.FromSeconds(Int32.Parse(_config["UDPSettings:broadcastFrequency"]))); // Espera x segundos antes de enviar el siguiente aviso
                }
            }
        }

        private void CheckOverlayStatus(object state)
        {
            if (this._ovl.statusValue == StatusEnum.Concentrado && DateTime.Now - this._ovl.lastStatusUpdate > TimeSpan.FromMinutes(Int32.Parse(_config["UDPSettings:podoroFocusLimit"])))
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