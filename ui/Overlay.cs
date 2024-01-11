using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using tardis.ui;

namespace ui
{
    public partial class Overlay : Form
    {
        // Variables para almacenar el desplazamiento, el di�metro y el estado del color
        private Point offset;
        private int diameter;
        private Color StatusColor;
        private StatusEnum StatusValue;
        private DateTime LastStatusUpdate;
        private String NodeName;
        List<Dictionary<string, string>> Nstates;

        // Variables para los gr�ficos
        private GraphicsPath path = new GraphicsPath();
        private GraphicsPath path2 = new GraphicsPath();

        public StatusEnum statusValue { get => StatusValue; }
        public DateTime lastStatusUpdate { get => LastStatusUpdate; }
        public string nodeName { get => NodeName; set => NodeName = value; }

        // Constructor
        public Overlay()
        {
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

            // Crear un men� contextual y agregarlo al formulario
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

            this.ContextMenuStrip = contextMenu;
            this.Nstates = new List<Dictionary<string, string>>();

            // Configuraci�n del estado inicial
            ChangeStatus(StatusEnum.Disponible);
            NodeName = "Nodo1";

            // Ajuste del formulario a la pantalla
            SnapToEdge();
        }

        // M�todo para cambiar el estado del color
        public void ChangeStatus(StatusEnum value)
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
                case StatusEnum.Interacci�n:
                    StatusColor = Color.Orange;
                    break;
            }
            this.Invalidate();
        }

        // M�todo para mover el formulario arrastr�ndolo con el rat�n
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                // Obtener la posici�n actual del rat�n
                int mouseX = Control.MousePosition.X;
                int mouseY = Control.MousePosition.Y;

                // Obtener la posici�n actual del formulario
                int formX = this.Location.X;
                int formY = this.Location.Y;

                // Calcular el desplazamiento entre el rat�n y el formulario
                int offsetX = mouseX - formX;
                int offsetY = mouseY - formY;

                // Almacenar el desplazamiento en una variable de nivel de clase
                this.offset = new Point(offsetX, offsetY);

                // Mover el formulario con el rat�n
                this.MouseMove += MouseMoveHandler;
            }
        }

        // M�todo para mover el formulario con el rat�n
        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            // Obtener la nueva posici�n del rat�n
            int mouseX = Control.MousePosition.X;
            int mouseY = Control.MousePosition.Y;

            // Establecer la nueva posici�n del formulario
            this.Location = new Point(mouseX - this.offset.X, mouseY - this.offset.Y);
        }

        // M�todo para liberar el rat�n cuando se suelta el bot�n
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                // Eliminar el controlador de eventos de movimiento del rat�n
                this.MouseMove -= MouseMoveHandler;

                // Ajustar el formulario al borde m�s cercano de la pantalla
                SnapToEdge();
            }
        }

        // M�todo para ajustar el formulario al borde m�s cercano de la pantalla
        private void SnapToEdge()
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
                //pathtmpelipse.AddEllipse(0, diameter + 1 + i * 15, 15, 15);
                RectangleF node = new RectangleF(0, diameter + 1 + i * 15, 50, 15);

                pathnode.AddRectangle(node);

                // Crea un nuevo GraphicsPath.
                pathtmptext = new GraphicsPath();

                // Define la elipse en la que se va a escribir el texto.
                RectangleF rect = new RectangleF(0, diameter + 1 + i * 15, 100, 200);

                // Agrega el texto al GraphicsPath.
                pathtmptext.AddString(
                    state["id"], // El texto que se va a escribir.
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
                    switch(state["status"])
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

        // M�todo para dibujar el c�rculo en la regi�n definida
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

        internal void SetStatus(StatusEnum descanso)
        {
            this.ChangeStatus(descanso);
        }

        public void UpdateNeighbors(List<Dictionary<string, string>> states)
        {
            DateTime now = DateTime.Now;

            foreach (var state in states)
            {
                var existingState = Nstates.FirstOrDefault(n => n["id"] == state["id"]);
                if (existingState != null)
                {
                    // Si el estado ya existe, actualiza sus valores.
                    existingState["status"] = state["status"];
                    existingState["id"] = state["id"];
                    existingState["datetime"] = state["datetime"];
                }
                else
                {
                    // Si el estado no existe, lo agrega a Nstates.
                    Nstates.Add(state);
                }
            }

            // Elimina los registros que llevan m�s de 1 minuto sin comunicar.
            Nstates.RemoveAll(n => (now - DateTime.Parse(n["datetime"])).TotalMinutes > 1);

            this.Invoke(new Action(() =>
            {
                SnapToEdge();
            }));
        }


    }
}

