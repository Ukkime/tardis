using System;
using System.Threading;
using System.Windows.Forms;
using tardis.ui;
using ui;

namespace tardis.Commands
{
    internal class StartTardis
    {
        private Overlay _ovl;
        private System.Threading.Timer _timer;
        public StartTardis() {}

        public Form Run()
        {
            this._ovl = new Overlay();
            TimerCallback tcb = CheckOverlayStatus;
            _timer = new System.Threading.Timer(tcb, null, 0, 1000); // Comprueba cada minuto
            return this._ovl;
        }

        private void CheckOverlayStatus(object state)
        {
            if (this._ovl.statusValue == StatusEnum.Concentrado && DateTime.Now - this._ovl.lastStatusUpdate > TimeSpan.FromMinutes(90))
            {
                this._ovl.SetStatus(StatusEnum.Descanso);
                System.Windows.Forms.MessageBox.Show("Llevas 1:30h concentrado, considera descansar un poco o dedicar unos minutos a otras tareas.");
            } 
        }

    }
}
