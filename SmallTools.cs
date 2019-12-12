using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace toolPlus
{
    public partial class SmallTools : Form
    {
        public SmallTools()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {

            string tempstr = "";
            try
            {
                dataGridView1.Rows[0].Cells[3].Value = (Convert.ToDouble(dataGridView1.Rows[0].Cells[1].Value) * Convert.ToDouble(textBox1.Text)).ToString();
                dataGridView1.Rows[0].Cells[4].Value = (Convert.ToDouble(dataGridView1.Rows[0].Cells[2].Value) * Convert.ToDouble(textBox1.Text)).ToString();

                dataGridView1.Rows[1].Cells[3].Value = (Convert.ToDouble(dataGridView1.Rows[1].Cells[1].Value) * Convert.ToDouble(textBox1.Text)).ToString();
                dataGridView1.Rows[1].Cells[4].Value = (Convert.ToDouble(dataGridView1.Rows[1].Cells[2].Value) * Convert.ToDouble(textBox1.Text)).ToString();

                dataGridView1.Rows[2].Cells[3].Value = (Convert.ToDouble(dataGridView1.Rows[2].Cells[1].Value) * Convert.ToDouble(textBox1.Text)).ToString();
                dataGridView1.Rows[2].Cells[4].Value = (Convert.ToDouble(dataGridView1.Rows[2].Cells[2].Value) * Convert.ToDouble(textBox1.Text)).ToString();

                dataGridView1.Rows[3].Cells[3].Value = (Convert.ToDouble(dataGridView1.Rows[3].Cells[1].Value) * Convert.ToDouble(textBox1.Text)).ToString();
                dataGridView1.Rows[3].Cells[4].Value = (Convert.ToDouble(dataGridView1.Rows[3].Cells[2].Value) * Convert.ToDouble(textBox1.Text)).ToString();

                dataGridView1.Rows[4].Cells[3].Value = (Convert.ToDouble(dataGridView1.Rows[4].Cells[1].Value) * Convert.ToDouble(textBox1.Text)).ToString();
                dataGridView1.Rows[4].Cells[4].Value = (Convert.ToDouble(dataGridView1.Rows[4].Cells[2].Value) * Convert.ToDouble(textBox1.Text)).ToString();

                tempstr += "#define POS_M1 " + dataGridView1.Rows[0].Cells[3].Value + "  //" + dataGridView1.Rows[0].Cells[1].Value + "cm\r\n";
                tempstr += "#define POS_M2 " + dataGridView1.Rows[0].Cells[4].Value +"  //" + dataGridView1.Rows[0].Cells[2].Value + "cm\r\n\r\n";

                tempstr += "#define LOUNGE_M1 " + dataGridView1.Rows[1].Cells[3].Value + "  //" + dataGridView1.Rows[1].Cells[1].Value + "cm\r\n";
                tempstr += "#define LOUNGE_M2 " + dataGridView1.Rows[1].Cells[4].Value + "  //" + dataGridView1.Rows[1].Cells[2].Value + "cm\r\n\r\n";

                tempstr += "#define TV_PC_M1 " + dataGridView1.Rows[2].Cells[3].Value + "  //" + dataGridView1.Rows[2].Cells[1].Value + "cm\r\n";
                tempstr += "#define TV_PC_M2 " + dataGridView1.Rows[2].Cells[4].Value + "  //" + dataGridView1.Rows[2].Cells[2].Value + "cm\r\n\r\n";

                tempstr += "#define ANTI_SNORE_M1 " + dataGridView1.Rows[3].Cells[3].Value + "  //" + dataGridView1.Rows[3].Cells[1].Value + "cm\r\n";
                tempstr += "#define ANTI_SNORE_M2 " + dataGridView1.Rows[3].Cells[4].Value + "  //" + dataGridView1.Rows[3].Cells[2].Value + "cm\r\n\r\n";

                tempstr += "#define WORK_M1 " + dataGridView1.Rows[4].Cells[3].Value + "  //" + dataGridView1.Rows[4].Cells[1].Value + "cm\r\n";
                tempstr += "#define WORK_M2 " + dataGridView1.Rows[4].Cells[4].Value + "  //" + dataGridView1.Rows[4].Cells[2].Value + "cm\r\n";



                richTextBox1.Text = tempstr;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            

          
        }

        private void SmallTools_Load(object sender, EventArgs e)
        {
            int i ;

            for(i = 0; i <4;i++)
             dataGridView1.Rows.Add();

            dataGridView1.Rows[0].Cells[0].Value = "ZERO-G";
            dataGridView1.Rows[0].Cells[5].Value = "POS";

            dataGridView1.Rows[1].Cells[0].Value = "休闲";
            dataGridView1.Rows[1].Cells[5].Value = "LOUNGE";

            dataGridView1.Rows[2].Cells[0].Value = "TV";
            dataGridView1.Rows[2].Cells[5].Value = "TV";

            dataGridView1.Rows[3].Cells[0].Value = "打鼾干预";
            dataGridView1.Rows[3].Cells[5].Value = "ANTI_SNORE";

            dataGridView1.Rows[4].Cells[0].Value = "音乐";
            dataGridView1.Rows[4].Cells[5].Value = "WORK";

        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text == "JLDQ.14.B.301.153")
            {
                textBox1.Text = "84";
            }
        }
    }
}
