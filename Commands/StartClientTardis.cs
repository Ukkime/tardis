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
    internal class StartClientTardis
    {
        private Overlay _ovl;
        private IConfiguration _config;
        private System.Threading.Timer _timer;
        public static List<Dictionary<string, string>> states = new List<Dictionary<string, string>>();

        public StartClientTardis(IConfiguration config)
        {
            this._config = config;
        }

        public Form Run()
        {
            this._ovl = new ServerOverlay(this._config);
            TimerCallback tcb = CheckOverlayStatus;

            _timer = new System.Threading.Timer(tcb, null, 0, Int32.Parse(_config["UDPSettings:pomodoroFocusCheckFrequency"])); // Comprueba cada minuto
            return this._ovl;
        }

        private void CheckOverlayStatus(object state)
        {
            if (this._ovl.statusValue == StatusEnum.Concentrado && DateTime.Now - this._ovl.lastStatusUpdate > TimeSpan.FromMinutes(Int32.Parse(_config["UDPSettings:podoroFocusLimit"])))
            {
                this._ovl.SetStatus(StatusEnum.Descanso);
                System.Windows.Forms.MessageBox.Show("Llevas 1:30h concentrado, considera descansar un poco o dedicar unos minutos a otras tareas.");
            }
        }
    }
}