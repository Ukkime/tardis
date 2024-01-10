using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using static System.Windows.Forms.AxHost;
using tardis.ui;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ui
{
    public partial class Overlay : Form
    {
        // Variables para almacenar el desplazamiento, el diámetro y el estado del color
        private Point offset;
        private int diameter;
        private Color StatusColor;
        private StatusEnum StatusValue;

        // Variables para los gráficos
        private GraphicsPath path = new GraphicsPath();
        private GraphicsPath path2 = new GraphicsPath();

        // Constructor
        public Overlay()
        {
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

            // Crear un menú contextual y agregarlo al formulario
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            foreach (StatusEnum st in Enum.GetValues(typeof(StatusEnum)))
            {
                contextMenu.Items.Add(new ToolStripMenuItem(Enum.GetName(typeof(StatusEnum), st), null, (s, e) => ChangeStatus(st)));
            }
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(new ToolStripMenuItem("Salir", null, (s, e) => Application.Exit()));

            this.ContextMenuStrip = contextMenu;

            // Configuración del estado inicial
            ChangeStatus(StatusEnum.Disponible);

            // Ajuste del formulario a la pantalla
            SnapToEdge();
        }

        // Método para cambiar el estado del color
        public void ChangeStatus(StatusEnum value)
        {
            switch (value)
            {
                case StatusEnum.Disponible:
                    StatusColor = Color.Green;
                    break;
                case StatusEnum.Ocupado:
                    StatusColor = Color.DarkRed;
                    break;
                case StatusEnum.Ausente:
                    StatusColor = Color.Gray;
                    break;
                case StatusEnum.Descanso:
                    StatusColor = Color.Blue;
                    break;
            }
            this.Invalidate();
        }

        // Método para mover el formulario arrastrándolo con el ratón
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                // Obtener la posición actual del ratón
                int mouseX = Control.MousePosition.X;
                int mouseY = Control.MousePosition.Y;

                // Obtener la posición actual del formulario
                int formX = this.Location.X;
                int formY = this.Location.Y;

                // Calcular el desplazamiento entre el ratón y el formulario
                int offsetX = mouseX - formX;
                int offsetY = mouseY - formY;

                // Almacenar el desplazamiento en una variable de nivel de clase
                this.offset = new Point(offsetX, offsetY);

                // Mover el formulario con el ratón
                this.MouseMove += MouseMoveHandler;
            }
        }

        // Método para mover el formulario con el ratón
        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            // Obtener la nueva posición del ratón
            int mouseX = Control.MousePosition.X;
            int mouseY = Control.MousePosition.Y;

            // Establecer la nueva posición del formulario
            this.Location = new Point(mouseX - this.offset.X, mouseY - this.offset.Y);
        }

        // Método para liberar el ratón cuando se suelta el botón
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                // Eliminar el controlador de eventos de movimiento del ratón
                this.MouseMove -= MouseMoveHandler;

                // Ajustar el formulario al borde más cercano de la pantalla
                SnapToEdge();
            }
        }

        // Método para ajustar el formulario al borde más cercano de la pantalla
        private void SnapToEdge()
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

            // Crear una nueva región y actualizar la región del formulario
            Region tmpregion = new Region(path);
            tmpregion.Union(path2);
            this.Region = tmpregion;

            // Asegurarse de que el formulario se redibuje
            this.Invalidate();
        }

        // Método para dibujar el círculo en la región definida
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
    }
}
