using ui;
using System;
using System.Windows.Forms;

namespace tardis;

class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Commands.StartTardis().Run());
    }
}
