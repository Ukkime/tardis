using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        // Variables para almacenar el desplazamiento, el di�metro y el estado del color
        private Point offset;
        private int diameter;
        private Color StatusColor;
        private StatusEnum StatusValue;
        private DateTime LastStatusUpdate;
        private String NodeName;
        private IConfiguration _config;
        List<Dictionary<string, string>> Nstates;

        // Variables para los gr�ficos
        private GraphicsPath path = new GraphicsPath();
        private GraphicsPath path2 = new GraphicsPath();

        private System.Timers.Timer _timer;

        // Constructor
        public ServerOverlay(IConfiguration config) : base(config)
        {
            this._config = config;
            // Inicializaci�n de componentes y configuraci�n de la apariencia del formulario
            //InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(100, 100);
            this.Left = Screen.PrimaryScreen.WorkingArea.Right - 5;
            this.Top = Screen.PrimaryScreen.WorkingArea.Height / 4 - this.Height / 2;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;
            this.Opacity = 0.8;
            this.TopMost = true;

            // Configuraci�n del formulario para que sea circular
            this.diameter = 50;
            this.Region = new Region();
            this.Width = diameter;
            this.Height = diameter;
            this.ShowInTaskbar = false;
            ShowInTaskbar = false;

            this.Nstates = new List<Dictionary<string, string>>();

            // Configuraci�n del estado inicial
            ChangeStatus(StatusEnum.Disponible);
            NodeName = _config["GeneralSettings:nodeName"];

            _timer = new System.Timers.Timer(1000); // Intervalo de 10 segundos
            _timer.Elapsed += async (sender, e) => await UpdateNeighborsAsync();
            _timer.Start();

            // Crear un men� contextual y agregarlo al formulario
            UpdateContextMenu();

            // Ajuste del formulario a la pantalla
            SnapToEdge();
        }

        public async Task UpdateNeighborsAsync()
        {

                Boolean needScreenUpdate = false;
                DateTime now = DateTime.Now;

                // Get json data
                var responseString = await new RestService(this._config).GetNeighborsAsync("5e884898da280f36e7c310dd233371204884883bfe2a5094b5e3b3ebc3d60f20");
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
                                existingState["updatetime"] = neighborNode.LastCommunication.ToString();
                                needScreenUpdate = true;
                            }
                            // Si el estado ya existe, actualiza sus valores.
                            existingState["status"] = neighborNode.Status;
                            existingState["datetime"] = neighborNode.LastCommunication.ToString();
                        }
                        else
                        {
                            // Si el estado no existe y es diferent del nodo local, lo agrega a Nstates.
                            if (neighborNode.Name != NodeName)
                            {
                                Dictionary<string, string> tmpDict = new Dictionary<string, string>();

                                tmpDict.Add("name", neighborNode.Name);
                                tmpDict.Add("status", neighborNode.Status);
                                tmpDict.Add("updatetime", neighborNode.LastCommunication.ToString());
                                tmpDict.Add("datetime", neighborNode.LastCommunication.ToString());

                                Nstates.Add(tmpDict);
                                needScreenUpdate = true;
                            }
                        }
                        if(needScreenUpdate)
                    {
                        break;
                    }
                    }

                    // Elimina los registros que llevan m�s de 1 minuto sin comunicar.
                    //if (Nstates.RemoveAll(n => (now - DateTime.Parse(n["datetime"])).TotalMinutes > 1) > 0)
                    //{
                    //    needScreenUpdate = true;
                    //}

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
            // Obtener el �rea de trabajo de la pantalla
            Rectangle screen = Screen.PrimaryScreen.WorkingArea;

            // Obtener los l�mites del formulario
            Rectangle form = this.Bounds;

            // Calcular las distancias a cada borde de la pantalla
            int left = form.Left - screen.Left;
            int right = screen.Right - form.Right;
            int top = form.Top - screen.Top;
            int bottom = screen.Bottom - form.Bottom;

            // Encontrar la distancia m�nima
            int min = Math.Min(Math.Min(left, right), Math.Min(top, bottom));

            // Mover el formulario al borde con la distancia m�nima
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
                    12, // El tama�o de la fuente.
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

            // Crear una nueva regi�n y actualizar la regi�n del formulario

            this.Region = tmpregion;

            // Asegurarse de que el formulario se redibuje
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);


            // Dibujar el c�rculo en la regi�n definida
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
            contextMenu.Items.Add(new ToolStripMenuItem("Configuraci�n", null, (s, e) => new Settings(this)));
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
                        string updateTime = neighbor["updatetime"].Substring(11, neighbor["updatetime"].Length - 11);
                        DateTime updateTimeAsDateTime = DateTime.ParseExact(updateTime, "HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime now = DateTime.Now;
                        TimeSpan timeSpan = now - updateTimeAsDateTime;

                        // timeSpan contiene la diferencia de tiempo
                        double hoursPassed = timeSpan.TotalHours; // Total de horas pasadas
                        double minutesPassed = timeSpan.TotalMinutes; // Total de minutos pasados
                        double secondsPassed = timeSpan.TotalSeconds; // Total de segundos pasados

                        string menuItemText = $"{neighbor["name"]}: {neighbor["status"]} desde las {neighbor["updatetime"].Substring(11, neighbor["updatetime"].Length - 11)} (hace {Math.Floor(minutesPassed)} min)";
                        contextMenu.Items.Add(new ToolStripMenuItem(menuItemText));
                    }
                }
            }
            else
            {
                contextMenu.Items.Add(new ToolStripMenuItem("No se han detectado compa�eros"));
            }

            this.ContextMenuStrip = contextMenu;
        }
    }
}

