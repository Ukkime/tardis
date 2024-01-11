using ui;
using System;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace tardis;

class Program
{
    [STAThread]
    static void Main()
    {
        var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("config.json", optional: true, reloadOnChange: true);

        IConfiguration _config = builder.Build();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Ejecutamos la modalidad tardis que queremos

        switch(_config["GeneralSettings:neighborMode"]) {
            case "UDP":
                Application.Run(new Commands.StartUDPTardis(_config).Run());
                break;
            case "SERVER":
                Application.Run(new Commands.StartClientTardis(_config).Run());
                break;
            default:
                System.Windows.Forms.MessageBox.Show("No se reconoce el modo definido en el config.json, por favor, revisa la configuración.");
                Application.Exit();
                break;
        }

        
    }
}
