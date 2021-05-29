using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Data.SQLite;

namespace AgProgramlama
{


    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process p1 = new Process();
            p1.StartInfo.FileName = "C:/Users/melih/source/repos/ConsoleApp2/ConsoleApp2/bin/Release/netcoreapp3.1/ConsoleApp2.exe";   // actual file name
            p1.StartInfo.UseShellExecute = true;
            p1.StartInfo.RedirectStandardOutput = false;
            //p1.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            p1.Start();
            toolStripProgressBar1.Value = 100;
            toolStripStatusLabel1.Text = "Paket Yakalama Başlatıldı..";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SQLiteConnection con = new SQLiteConnection(@"Data Source = C:\DB Browser for SQLite\deneme.db");
            if (radioButton1.Checked == true)
            {
                
                con.Open();
                string girdi = textBox1.Text;
                SQLiteDataAdapter adapter = new SQLiteDataAdapter("SELECT * FROM Islem WHERE Kaynak = @kaynak", con);
                adapter.SelectCommand.Parameters.AddWithValue("@kaynak", girdi);
                DataSet dataset = new DataSet();
                adapter.Fill(dataset, "info");
                dataGridView1.DataSource = dataset.Tables[0];
                

            }
            else if (radioButton2.Checked == true)
            {
                con.Open();
                string girdi = textBox1.Text;
                SQLiteDataAdapter adapter = new SQLiteDataAdapter("SELECT * FROM Islem WHERE Port = @port", con);
                adapter.SelectCommand.Parameters.AddWithValue("@port", girdi);
                DataSet dataset = new DataSet();
                adapter.Fill(dataset, "info");
                dataGridView1.DataSource = dataset.Tables[0];
            }
            else if (radioButton3.Checked == true)
            {
                con.Open();
                string girdi = textBox1.Text;
                SQLiteDataAdapter adapter = new SQLiteDataAdapter("SELECT * FROM Islem WHERE Protokol = @protokol", con);
                adapter.SelectCommand.Parameters.AddWithValue("@protokol", girdi);
                DataSet dataset = new DataSet();
                adapter.Fill(dataset, "info");
                dataGridView1.DataSource = dataset.Tables[0];
            }
            else
                label2.Text = "Lütfen seçim yapınız..";
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1.ReadOnly = true; 
            dataGridView1.AllowUserToDeleteRows = false; 
            dataGridView1.ColumnCount = 3; 
            dataGridView1.Columns[0].Name = "Zaman";
            dataGridView1.Columns[1].Name = "Kaynak";
            dataGridView1.Columns[2].Name = "Hedef Port";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DateTime baslangic = dateTimePicker1.Value.Date +
                    dateTimePicker2.Value.TimeOfDay;
            label5.Text = baslangic.ToString();
            DateTime bitis = dateTimePicker3.Value.Date +
                    dateTimePicker4.Value.TimeOfDay;
            string a = Convert.ToDateTime(baslangic).ToString("yyyy-MM-dd hh:mm");
            string b = Convert.ToDateTime(bitis).ToString("yyyy-MM-dd hh:mm");
            Form2 ff = new Form2(a, b);
            ff.Show();

        }

    }
}
