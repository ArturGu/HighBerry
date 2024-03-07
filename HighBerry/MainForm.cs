using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HighBerry
{
    enum RowState
    {
        Existed,
        New,
        Modified,
        ModifiedNew,
        Deleted
    }

    public partial class MainForm : Form
    {
        ConnectionDB database = new ConnectionDB();

        int selectedRow;

        public MainForm()
        {
            InitializeComponent();
        }

        private void ClearFields()
        {
            textBox_harvest.Text = "";
            comboBox1.Text = "";
            textBox_amount.Text = "";
            textBox_collectionDate.Text = "";
        }

        private void CreateColumns()
        {
            dataGridView1.Columns.Add("harvest_id", "Код партії");
            dataGridView1.Columns.Add("culture_id", "Культура");
            dataGridView1.Columns.Add("amount", "Вага (кг)");
            dataGridView1.Columns.Add("collection_date", "Дата збору");
            dataGridView1.Columns.Add("IsNew", String.Empty);

            // Приховати стовбець за індексом
            dataGridView1.Columns[4].Visible = false;

        }

        private void ReadSingleRow(DataGridView dgw, IDataRecord record)
        {
            dgw.Rows.Add(record.GetInt32(0), record.GetInt32(1), record.GetDecimal(2), record.GetDateTime(3), RowState.ModifiedNew);
        }


        private void RefreshDataGrid(DataGridView dgw)
        {
            dgw.Rows.Clear();

            string queryString = $"select * from Harvest";

            SqlCommand command = new SqlCommand(queryString, database.getConnection());

            database.openConnection();

            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ReadSingleRow(dgw, reader);
            }

            reader.Close();

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CreateColumns();
            RefreshDataGrid(dataGridView1);

            string iconPath = "HighBerry.Resources.logo_mainform.ico"; // замініть "YourNamespace" на простір імен вашого проекту

            // Отримайте потік до ресурсу
            using (Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(iconPath))
            {
                if (iconStream != null)
                {
                    // Змініть розмір іконки
                    int newWidth = 48; // нова ширина
                    int newHeight = 48; // нова висота

                    // Створіть іконку з потоку та зміненими розмірами
                    Icon customIcon = new Icon(iconStream, newWidth, newHeight);

                    // Встановіть іконку для форми
                    this.Icon = customIcon;
                }
            }
        }


        private void dataGridView1_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            selectedRow = e.RowIndex;

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[selectedRow];

                textBox_harvest.Text = row.Cells[0].Value.ToString();
                comboBox1.Text = row.Cells[1].Value.ToString();
                textBox_amount.Text = row.Cells[2].Value.ToString();
                textBox_collectionDate.Text = row.Cells[3].Value.ToString();
            }
        }

        private void button_refresh1_Click(object sender, EventArgs e)
        {
            RefreshDataGrid(dataGridView1);
            ClearFields();
        }

        private void Search(DataGridView dgw)
        {
            dgw.Rows.Clear();

            string searchString = $"select * from Harvest where concat (harvest_id, culture_id, amount, collection_date) like N'%" + textBox_search1.Text + "%'";

            SqlCommand command = new SqlCommand(searchString, database.getConnection());

            database.openConnection();

            SqlDataReader read = command.ExecuteReader();

            while (read.Read())
            {
                ReadSingleRow(dgw, read);
            }

            read.Close();
        }

        private void textBox_search1_TextChanged(object sender, EventArgs e)
        {
            Search(dataGridView1);
        }

        private void deleteRow()
        {
            int index = dataGridView1.CurrentCell.RowIndex;

            if (index >= 0)
            {
                var result = MessageBox.Show("Ви впевнені, що хочете видалити цей запис?", "Попередження!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    dataGridView1.Rows[index].Visible = false;

                    if (dataGridView1.Rows[index].Cells[0].Value.ToString() == string.Empty)
                    {
                        dataGridView1.Rows[index].Cells[4].Value = RowState.Deleted;
                        return;
                    }

                    dataGridView1.Rows[index].Cells[4].Value = RowState.Deleted;
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, оберіть рядок для видалення.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private new void Update()
        {
            database.openConnection();

            for (int index = 0; index < dataGridView1.Rows.Count; index++)
            {
                var rowState = (RowState)dataGridView1.Rows[index].Cells[4].Value;

                if (rowState == RowState.Existed)
                {
                    continue;
                }

                if (rowState == RowState.Deleted)
                {
                    var harvest_id = Convert.ToInt32(dataGridView1.Rows[index].Cells[0].Value);
                    var deleteQuery = $"DELETE FROM Harvest WHERE harvest_id = {harvest_id}";

                    var command = new SqlCommand(deleteQuery, database.getConnection());
                    command.ExecuteNonQuery();
                }

                if (rowState == RowState.Modified)
                {
                    var harvest_id = dataGridView1.Rows[index].Cells[0].Value.ToString();
                    var culture_id = dataGridView1.Rows[index].Cells[1].Value.ToString();

                    // Змінено на використання параметрів для запобігання SQL-ін'єкцій
                    var amount = dataGridView1.Rows[index].Cells[2].Value;
                    var collection_date = dataGridView1.Rows[index].Cells[3].Value;

                    // Використовуйте параметри для запобігання SQL-ін'єкцій
                    var changeQuery = "UPDATE Harvest SET culture_id = @CultureId, amount = @Amount, collection_date = @CollectionDate WHERE harvest_id = @HarvestId";

                    var command = new SqlCommand(changeQuery, database.getConnection());

                    // Додаємо параметри
                    command.Parameters.AddWithValue("@CultureId", culture_id);

                    // Використовуйте відповідний тип параметра для 'amount'
                    command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = Convert.ToDecimal(amount);

                    // Використовуйте відповідний тип параметра для 'collection_date'
                    command.Parameters.Add("@CollectionDate", SqlDbType.DateTime).Value = Convert.ToDateTime(collection_date);

                    command.Parameters.AddWithValue("@HarvestId", harvest_id);

                    // Виконати команду
                    command.ExecuteNonQuery();
                }
            }

            database.closeConnection();
        }



        private void Change()
        {
            var selectedRowIndex = dataGridView1.CurrentCell.RowIndex;

            var harvest_id = textBox_harvest.Text;
            var culture_id = comboBox1.Text;
            decimal amount;
            var collection_date = textBox_collectionDate.Text; // Змінено на рядок, оскільки введення надходить як рядок

            if (dataGridView1.Rows[selectedRowIndex].Cells[0].Value.ToString() != string.Empty)
            {
                if (decimal.TryParse(textBox_amount.Text, out amount))
                {
                    dataGridView1.Rows[selectedRowIndex].SetValues(harvest_id, culture_id, amount, collection_date);
                    dataGridView1.Rows[selectedRowIndex].Cells[4].Value = RowState.Modified;
                }
                else
                {
                    MessageBox.Show("Вага повинна мати числовий формат!");
                }
            }
        }


        private void button_trash1_Click(object sender, EventArgs e)
        {
            deleteRow();
            ClearFields();
            Update();
        }

        private void button_pen1_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Ви впевнені, що хочете зберегти зміни?", "Підтвердження!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Change();
                Update();
            }
        }


        private void button_plus1_Click(object sender, EventArgs e)
        {
            AddBatch add_form = new AddBatch();
            add_form.Show();
        }

    }  
}
