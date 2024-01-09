using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ui
{
    public partial class Overlay : Form
    {
        private Point offset; // Variable para almacenar el desplazamiento
        private int diameter;
        // Constructor
        public Overlay()
        {
            //InitializeComponent();
            // Set the form's size and position
 
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(100, 100);
            this.Left = Screen.PrimaryScreen.WorkingArea.Right - 5;
            this.Top = Screen.PrimaryScreen.WorkingArea.Height / 4 - this.Height / 2;
            // Set the form's style and appearance
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Red;
            this.Opacity = 0.8;
            this.TopMost = true;
            // Make the form circular
            GraphicsPath path = new GraphicsPath();
            this.diameter = 50;
            path.AddEllipse(0, 0, diameter, diameter);
            this.Region = new Region(path);

            this.Width = diameter;
            this.Height = diameter;
            SnapToEdge();
        }

        // Method to move the form by dragging it with the mouse
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                // Get the current mouse position
                int mouseX = Control.MousePosition.X;
                int mouseY = Control.MousePosition.Y;
                // Get the current form position
                int formX = this.Location.X;
                int formY = this.Location.Y;
                // Calculate the offset between the mouse and the form
                int offsetX = mouseX - formX;
                int offsetY = mouseY - formY;
                // Store the offset in a class-level variable
                this.offset = new Point(offsetX, offsetY);
                // Move the form with the mouse
                this.MouseMove += Form1_MouseMove;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the new mouse position
            int mouseX = Control.MousePosition.X;
            int mouseY = Control.MousePosition.Y;
            // Set the new form position
            this.Location = new Point(mouseX - this.offset.X, mouseY - this.offset.Y);
        }

        // Method to release the mouse when the button is up
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                // Remove the mouse move event handler
                this.MouseMove -= Form1_MouseMove;
                // Snap the form to the nearest edge of the screen
                SnapToEdge();
            }
        }

        // Method to snap the form to the nearest edge of the screen
        private void SnapToEdge()
        {
            // Get the screen working area
            Rectangle screen = Screen.PrimaryScreen.WorkingArea;
            // Get the form bounds
            Rectangle form = this.Bounds;
            // Calculate the distances to each edge of the screen
            int left = form.Left - screen.Left;
            int right = screen.Right - form.Right;
            int top = form.Top - screen.Top;
            int bottom = screen.Bottom - form.Bottom;
            // Find the minimum distance
            int min = Math.Min(Math.Min(left, right), Math.Min(top, bottom));
            // Move the form to the edge with the minimum distance
            if (min == left)
            {
                this.Left = screen.Left;
            }
            else if (min == right)
            {
                this.Left = screen.Right - this.diameter;
            }
            else if (min == top)
            {
                this.Top = screen.Top;
            }
            else if (min == bottom)
            {
                this.Top = screen.Bottom - this.Height;
            }
        }
    }
}
