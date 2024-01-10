using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ui;

namespace tardis.ui
{
    public partial class Settings : Form
    {
        private Overlay _ovl;
        public Settings(global::ui.Overlay overlay)
        {
            this._ovl = overlay;
            InitializeComponent();
            this.Visible = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this._ovl.nodeName = this.nodeName.Text;
            this.Close();
        }
    }
}
