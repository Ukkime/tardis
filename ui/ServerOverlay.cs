using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using tardis.Services;
using tardis.ui;


namespace ui
{
    public partial class ServerOverlay : Overlay
    {
        // Variables para almacenar el desplazamiento, el diámetro y el estado del color
        private Point offset;
        private int diameter;
        private Color StatusColor;
        private StatusEnum StatusValue;
        private DateTime LastStatusUpdate;
        private String NodeName;
        private IConfiguration _config;
        List<Dictionary<string, string>> Nstates;

        // Variables para los gráficos
        private GraphicsPath path = new GraphicsPath();
        private GraphicsPath path2 = new GraphicsPath();

        private System.Timers.Timer _timer;
        private System.Timers.Timer _timerClipboard;


        // Constructor
        public ServerOverlay(IConfiguration config) : base(config)
        {
            this._config = config;
            // Inicialización de componentes y configuración de la apariencia del formulario
            //InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(100, 100);
            this.Left = Screen.PrimaryScreen.WorkingArea.Right - 5;
            this.Top = Screen.PrimaryScreen.WorkingArea.Height / 4 - this.Height / 2;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;
            this.Opacity = 0.8;
            this.TopMost = true;

            // Configuración del formulario para que sea circular
            this.diameter = 50;
            this.Region = new Region();
            this.Width = diameter;
            this.Height = diameter;
            this.ShowInTaskbar = false;
            ShowInTaskbar = false;

            this.Nstates = new List<Dictionary<string, string>>();

            // Configuración del estado inicial
            ChangeStatus(StatusEnum.Disponible);
            NodeName = _config["GeneralSettings:nodeName"];

            _timer = new System.Timers.Timer(Int32.Parse(_config["ServerSettings:updateFrequency"]) * 1000);
            _timer.Elapsed += async (sender, e) => await UpdateNeighborsAsync();
            _timer.Start();

            _timerClipboard = new System.Timers.Timer(Int32.Parse(_config["ServerSettings:updateClipboardFrequency"]) * 1000);
            _timerClipboard.Elapsed += CheckClipboardContent;
            _timerClipboard.Start();

            // Crear un menú contextual y agregarlo al formulario
            UpdateContextMenu();

            // Ajuste del formulario a la pantalla
            SnapToEdge();
        }

        public override async void ChangeStatus(StatusEnum value)
        {
            this.StatusValue = value;
            this.LastStatusUpdate = DateTime.Now;
            switch (value)
            {
                case StatusEnum.Disponible:
                    StatusColor = Color.Green;
                    break;
                case StatusEnum.Ocupado:
                    StatusColor = Color.DarkRed;
                    break;
                case StatusEnum.Concentrado:
                    StatusColor = Color.Red;
                    break;
                case StatusEnum.Ausente:
                    StatusColor = Color.Gray;
                    break;
                case StatusEnum.Descanso:
                    StatusColor = Color.Blue;
                    break;
                case StatusEnum.Interacción:
                    StatusColor = Color.Orange;
                    break;
            }
            this.Invalidate();

            // Avisamos a la API del cambio
            if(this._config != null)
            {
                string ressp = await new RestService(this._config).UpdateNodeStatusAsync(_config["ServerSettings:restApiNeighborCode"], _config["GeneralSettings:nodeName"], value.ToString());
            }
         
        }

        public async Task UpdateNeighborsAsync()
        {

            Boolean needScreenUpdate = false;
            DateTime now = DateTime.Now;

            // Get json data
            var responseString = await new RestService(this._config).GetGroupAndNodeAsync(_config["ServerSettings:restApiNeighborCode"], _config["GeneralSettings:nodeName"]);
            var neighbor = JsonConvert.DeserializeObject<Neighbor>(responseString);

            if (neighbor != null)
            {
                foreach (var neighborNode in neighbor.NeighborNodes)
                {
                    var existingState = Nstates.FirstOrDefault(n => n["name"] == neighborNode.Name);
                    if (existingState != null)
                    {
                        if (existingState["status"] != neighborNode.Status)
                        {
                            existingState["updatetime"] = neighborNode.Updatetime.ToString();
                            existingState["datetime"] = neighborNode.Datetime.ToString();
                            needScreenUpdate = true;
                        }
                        // Si el estado ya existe, actualiza sus valores.
                        existingState["status"] = neighborNode.Status;
                        existingState["updatetime"] = neighborNode.Updatetime.ToString();
                    }
                    else
                    {
                        // Si el estado no existe y es diferent del nodo local, lo agrega a Nstates.
                        if (neighborNode.Name != NodeName)
                        {
                            Dictionary<string, string> tmpDict = new Dictionary<string, string>();

                            tmpDict.Add("name", neighborNode.Name);
                            tmpDict.Add("status", neighborNode.Status);
                            tmpDict.Add("updatetime", neighborNode.Updatetime.ToString());
                            tmpDict.Add("datetime", neighborNode.Datetime.ToString());

                            Nstates.Add(tmpDict);
                            needScreenUpdate = true;
                        }
                    }
                    if (needScreenUpdate)
                    {
                        break;
                    }
                }

                List<Dictionary<string, string>> toRemove = new List<Dictionary<string, string>>();
                foreach (var n in Nstates)
                {
                    double t = (now - DateTime.Parse(n["updatetime"])).TotalMinutes;
                    if (t > 5)
                    {
                        toRemove.Add(n);
                    }
                }

                foreach (var n in toRemove)
                {
                    Nstates.RemoveAll(state => state["name"] == n["name"]);
                }

                if (toRemove.Count > 0)
                {
                    needScreenUpdate = true;
                }

                if (needScreenUpdate)
                {
                    this.Invoke(new Action(() =>
                    {
                        SnapToEdge();
                    }));
                }
                this.Invoke(new Action(() =>
                {
                    UpdateContextMenu();
                }));
            }
        }

        protected override void SnapToEdge()
        {
            // Obtener el área de trabajo de la pantalla
            Rectangle screen = Screen.PrimaryScreen.WorkingArea;

            // Obtener los límites del formulario
            Rectangle form = this.Bounds;

            // Calcular las distancias a cada borde de la pantalla
            int left = form.Left - screen.Left;
            int right = screen.Right - form.Right;
            int top = form.Top - screen.Top;
            int bottom = screen.Bottom - form.Bottom;

            // Encontrar la distancia mínima
            int min = Math.Min(Math.Min(left, right), Math.Min(top, bottom));

            // Mover el formulario al borde con la distancia mínima
            if (min == left)
            {
                this.Left = screen.Left;
                path2.Reset();
                path.Reset();
                path.AddEllipse(0, 0, diameter, diameter);
                path2.AddRectangle(new Rectangle(0, 0, diameter / 2, diameter));
            }
            else if (min == right)
            {
                this.Left = screen.Right - this.diameter;
                path2.Reset();
                path.Reset();
                path.AddEllipse(0, 0, diameter, diameter);
                path2.AddRectangle(new Rectangle(diameter / 2, 0, diameter / 2, diameter));
            }
            else if (min == top)
            {
                this.Top = screen.Top;
                path2.Reset();
                path.Reset();
                path.AddEllipse(0, 0, diameter, diameter);
                path2.AddRectangle(new Rectangle(0, 0, diameter, diameter / 2));
            }
            else if (min == bottom)
            {
                this.Top = screen.Bottom - this.Height;
                path2.Reset();
                path.Reset();
                path.AddEllipse(0, 0, diameter, diameter);
                path2.AddRectangle(new Rectangle(0, diameter / 2, diameter, diameter / 2));
            }


            Region tmpregion = new Region(path);
            tmpregion.Union(path2);

            int i = 0;
            GraphicsPath pathtmptext = new GraphicsPath();
            GraphicsPath pathnode = new GraphicsPath();
            foreach (var state in Nstates)
            {
                RectangleF node = new RectangleF(0, diameter + 1 + i * 15, 50, 15);

                pathnode.AddRectangle(node);

                // Crea un nuevo GraphicsPath.
                pathtmptext = new GraphicsPath();

                // Define la elipse en la que se va a escribir el texto.
                RectangleF rect = new RectangleF(0, diameter + 1 + i * 15, 100, 200);

                // Agrega el texto al GraphicsPath.
                pathtmptext.AddString(
                    state["name"], // El texto que se va a escribir.
                    FontFamily.GenericSansSerif, // La fuente del texto.
                    (int)FontStyle.Regular, // El estilo de la fuente.
                    12, // El tamaño de la fuente.
                    rect, // La elipse en la que se va a escribir el texto.
                    StringFormat.GenericDefault); // El formato del texto.

                // Dibuja el GraphicsPath en el formulario.
                i++;

                tmpregion.Union(pathnode);
                tmpregion.Union(pathtmptext);
                this.Paint += (s, e) =>
                {
                    Brush cl;
                    switch (state["status"])
                    {
                        case "Disponible":
                            cl = Brushes.Green;
                            break;
                        case "Ocupado":
                            cl = Brushes.DarkRed;
                            break;
                        case "Ausente":
                            cl = Brushes.Gray;
                            break;
                        case "Descanso":
                            cl = Brushes.Blue;
                            break;
                        case "Concentrado":
                            cl = Brushes.Red;
                            break;
                        default:
                            cl = Brushes.Gray;
                            break;
                    }


                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(cl, pathtmptext);

                };
            }

            this.Height = diameter + 1 + i * 15;

            // Crear una nueva región y actualizar la región del formulario

            this.Region = tmpregion;

            // Asegurarse de que el formulario se redibuje
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);


            // Dibujar el círculo en la región definida
            using (Brush brush = new SolidBrush(this.StatusColor)) // Elige el color que prefieras
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, diameter / 11f, diameter / 11f, diameter / 1.2f, diameter / 1.2f);
            }
        }

        protected override void UpdateContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            foreach (StatusEnum st in Enum.GetValues(typeof(StatusEnum)))
            {
                contextMenu.Items.Add(new ToolStripMenuItem(Enum.GetName(typeof(StatusEnum), st), null, (s, e) => ChangeStatus(st)));
            }
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(new ToolStripMenuItem("Salir", null, (s, e) => Application.Exit()));
            contextMenu.Items.Add(new ToolStripSeparator());

            if(this.Nstates == null)
            {
                this.Nstates = new List<Dictionary<string, string>>();
            }

            if (this.Nstates.Count > 0)
            {
                foreach (var neighbor in this.Nstates)
                {
                    if (neighbor["name"] != this.NodeName)
                    {
                        string updateTime = neighbor["datetime"].Substring(11, neighbor["datetime"].Length - 11);
                        DateTime updateTimeAsDateTime = DateTime.ParseExact(updateTime, "HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime now = DateTime.Now;
                        TimeSpan timeSpan = now - updateTimeAsDateTime;

                        // timeSpan contiene la diferencia de tiempo
                        double hoursPassed = timeSpan.TotalHours; // Total de horas pasadas
                        double minutesPassed = timeSpan.TotalMinutes; // Total de minutos pasados
                        double secondsPassed = timeSpan.TotalSeconds; // Total de segundos pasados

                        string menuItemText = $"{neighbor["name"]}: {neighbor["status"]} desde las {neighbor["datetime"].Substring(11, neighbor["datetime"].Length - 11)} (hace {Math.Floor(minutesPassed)} min)";
                        
                        ToolStripMenuItem mainMenuItem = new ToolStripMenuItem(menuItemText);
                        ToolStripMenuItem subMenuItem = new ToolStripMenuItem("Enviar clipboard", null, (s, e) => SendClipboardAsync(neighbor["name"]));

                        mainMenuItem.DropDownItems.Add(subMenuItem);
                        contextMenu.Items.Add(mainMenuItem);
                    }
                }
            }
            else
            {
                contextMenu.Items.Add(new ToolStripMenuItem("No se han detectado compañeros"));

                //ToolStripMenuItem mainMenuItem = new ToolStripMenuItem("test");
                //ToolStripMenuItem subMenuItem = new ToolStripMenuItem("Enviar clipboard", null, (s, e) => SendClipboardAsync(NodeName));

                //mainMenuItem.DropDownItems.Add(subMenuItem);
                //contextMenu.Items.Add(mainMenuItem);
            }

            this.ContextMenuStrip = contextMenu;
        }

        protected async Task SendClipboardAsync(string destNode)
        {
            string result = await new RestService(this._config).SendClipboardToNeighborAsync(_config["ServerSettings:restApiNeighborCode"], _config["GeneralSettings:nodeName"], _config["GeneralSettings:nodeName"], GetClipboardText());
        }

        private async void CheckClipboardContent(object source, ElapsedEventArgs e)
        {
            var clipboardContent = await new RestService(_config).RetrieveClipboardAsync(_config["ServerSettings:restApiNeighborCode"], NodeName);
            if (clipboardContent != null)
            {
                // Muestra un cuadro de diálogo preguntando al usuario si quiere cargar el contenido del portapapeles
                var result = MessageBox.Show("¿Quieres cargar este contenido en tu portapapeles?\n\n" + clipboardContent["clipboardContent"], clipboardContent["sender"] + " te envia su clipboard", MessageBoxButtons.YesNo);

                // Si el usuario elige 'Yes', carga el contenido del portapapeles
                if (result == DialogResult.Yes)
                {
                    SetClipboardText(clipboardContent["clipboardContent"]);
                }
            }
        }

        private static string GetClipboardText()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = "Get-Clipboard",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = startInfo };
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }

        private static void SetClipboardText(string text)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"Set-Clipboard -Value '{text}'",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = startInfo };
            process.Start();
            process.WaitForExit();
        }
    }
}

