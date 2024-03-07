using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HighBerry
{
    public partial class AddBatch : Form
    {
        ConnectionDB database = new ConnectionDB();

        public AddBatch()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;

            // Заповнення комбінованого списку даними з таблиці Culture
            FillCultureComboBox();
        }

        private void FillCultureComboBox()
        {
            try
            {
                database.openConnection();

                string query = "SELECT culture_id, culture_name FROM Culture";
                SqlCommand cmd = new SqlCommand(query, database.getConnection());
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int cultureId = reader.GetInt32(0);
                    string cultureName = reader.GetString(1);
                    comboBox_Culture.Items.Add(new CultureItem(cultureId, cultureName));
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні культур: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                database.closeConnection();
            }
        }

        private void button_save_Click_1(object sender, EventArgs e)
        {
            database.openConnection();

            var harvest_id = textBox_HarvestId.Text;

            // Отримання значення culture_id із обраного елемента комбінованого списку
            int cultureId = ((CultureItem)comboBox_Culture.SelectedItem).Id;

            int amount;
            var collection_date = textBox_CollectionDate.Text;

            if (int.TryParse(textBox_amount.Text, out amount))
            {
                var addQuery = $"INSERT INTO Harvest (harvest_id, culture_id, amount, collection_date) VALUES ('{harvest_id}', {cultureId}, '{amount}', N'{collection_date}')";

                var command = new SqlCommand(addQuery, database.getConnection());
                command.ExecuteNonQuery();

                MessageBox.Show("Запис успішно створений!", "Успішно!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Вага повинна мати числовий формат!", "Помилка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            database.closeConnection();
        }
    }

    // Клас для представлення елемента комбінованого списку
    public class CultureItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public CultureItem(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}