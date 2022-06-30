using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dataq.Installer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string sql = null;
            try
            {
                sql = new WebClient().DownloadString("https://dataqio.s3.amazonaws.com/mysql_setup.sql");
                if (string.IsNullOrEmpty(sql))
                {
                    MessageBox.Show("Invalid SQL code!");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot retrieve the SQL code from the internet!\n" + ex.Message);
                return;
            }

            MySqlConnection conn;

            try
            {
                conn = new MySqlConnection();
                conn.ConnectionString = "server=127.0.0.1;uid=root;pwd=dataq;";// textBox1.Text; //"uid=root;pwd=dataq;"; ;//"server=127.0.0.1;uid=root;pwd=dataq;";
                conn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect to the server!\n" + ex.Message);
                return;
            }

            MessageBox.Show("Connected to the server!\n" );
            return;

            try
            {
                var cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not create database!\n" + ex.Message);
                return;
            }

            MessageBox.Show("Database schema created!");
        }
    }
   
}
