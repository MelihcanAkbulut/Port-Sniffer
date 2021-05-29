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
    public partial class Form2 : Form
    {
        public Form2(string baslangic, string bitis)
        {
            InitializeComponent();
            SQLiteConnection con = new SQLiteConnection(@"Data Source = C:\DB Browser for SQLite\deneme.db");
            con.Open();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter("SELECT * FROM Islem WHERE Zaman BETWEEN @baslangic and @bitis", con);
            adapter.SelectCommand.Parameters.AddWithValue("@baslangic", baslangic);
            adapter.SelectCommand.Parameters.AddWithValue("@bitis", bitis);
            DataSet dataset = new DataSet();
            adapter.Fill(dataset, "info");
            label1.Text = "Seçilen Tarihler İçin .txt Uzantılı Rapor Oluşturulmuştur. Dilerseniz Yukarıdan da İnceleyebilirsiniz...";
            dataGridView1.DataSource = dataset.Tables[0];
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "txt dosyası|0.txt";
            DateTime now = DateTime.Now;
            string aa = Convert.ToDateTime(now).ToString("yyyy-MM-dd hh.mm");
            sf.FileName = ""+aa;
            if (sf.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(sf.FileName, FileMode.OpenOrCreate, FileAccess.Write);
                fs.Close();
                string bas = "\tZAMAN \t\t\t KAYNAK \t     PROTOKOL-PORT";
                File.AppendAllText(sf.FileName, bas + Environment.NewLine);
                for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                {
                    object zaman = dataGridView1.Rows[i].Cells["Zaman"].Value;
                    object kaynak = dataGridView1.Rows[i].Cells["Kaynak"].Value;
                    object protokol = dataGridView1.Rows[i].Cells["Protokol"].Value;
                    object port = dataGridView1.Rows[i].Cells["Port"].Value;
                    string icerik = zaman + " / " + kaynak + " / " + protokol + "-" + port;
                    File.AppendAllText(sf.FileName, icerik + Environment.NewLine);
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1.ReadOnly = true; // sadece okunabilir olması yani veri düzenleme kapalı
            dataGridView1.AllowUserToDeleteRows = false; // satırların silinmesi engelleniyor
            dataGridView1.ColumnCount = 3; //Kaç kolon olacağı belirleniyor…
            dataGridView1.Columns[0].Name = "Zaman";//Kolonların adı belirleniyor
            dataGridView1.Columns[1].Name = "Kaynak";
            dataGridView1.Columns[2].Name = "Hedef Port";
        }
    }
}
