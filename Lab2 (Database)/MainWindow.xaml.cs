using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace Lab_2.Database
{
    public partial class MainWindow : Window
    {
        SQLiteConnection m_dbConntection = new SQLiteConnection($"Data source=students.db");
        SQLiteCommand command = new SQLiteCommand();
        public MainWindow()
        {
            InitializeComponent();

            m_dbConntection.Open();
            command.Connection = m_dbConntection;
            try
            {
                command.CommandText = "CREATE TABLE PersonalData " +
                        "(ID INTEGER PRIMARY KEY UNIQUE, Name TEXT, Stipend INTEGER);" +
                        "CREATE TABLE Marks " +
                        "(ID INTEGER PRIMARY KEY UNIQUE, Physic INTEGER, Math INTEGER)";
                command.ExecuteNonQuery();
            }
            catch { LoadGrid(); }
        }
        private void EnterData(object sender, EventArgs e)
        {
            string warning = "";
            bool condition = true;

            if (!(int.TryParse(idBox.Text, out int id) && id > -1))
            {
                warning += "ID может содержать только цифры";
                condition = false;
            }
            else if (studentsGrid.SelectedItem is null)
            {
                try
                {
                    command.CommandText = $"INSERT INTO PersonalData (ID) VALUES ({id}); " +
                $"INSERT INTO Marks (ID) VALUES ({id})";
                    command.ExecuteNonQuery();
                }
                catch
                {
                    warning += "Введённый ID уже занят.";
                    condition = false;
                }
            }

            if ((!(phMarkBox.Text == "") && !(int.TryParse(phMarkBox.Text, out int mark) && mark > 0 && mark < 6))
                || (!(mathMarkBox.Text == "") && !(int.TryParse(mathMarkBox.Text, out mark) && mark > 0 && mark < 6)))
            {
                warning += "\nВ поля для оценок могут быть внесены только целые числа от 1 до 5";
                condition = false;
            }

            if (condition)
            {
                command.CommandText = $"UPDATE PersonalData SET Name='{nameBox.Text}', " +
                        $"Stipend={stipendBox.IsChecked} WHERE ID={id}; ";

                if (phMarkBox.Text == "") command.CommandText += $"UPDATE Marks SET Physic=null WHERE ID={id}; ";
                else command.CommandText += $"UPDATE Marks SET Physic={phMarkBox.Text} WHERE ID={id}; ";

                if (mathMarkBox.Text == "") command.CommandText += $"UPDATE Marks SET Math=null WHERE ID={id}; ";
                else command.CommandText += $"UPDATE Marks SET Math={mathMarkBox.Text} WHERE ID={id}; ";

                command.ExecuteNonQuery();
                LoadGrid();
            }
            else MessageBox.Show(warning);
        }
        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Student student = (Student)studentsGrid.SelectedItem;
            if (student != null)
            {
                enterData.Content = "Редактировать";
                idBox.IsEnabled = false;
                editButton.IsEnabled = true;
                deleteButton.IsEnabled = true;

                idBox.Text = student.ID;
                nameBox.Text = student.name;
                phMarkBox.Text = student.phMark;
                mathMarkBox.Text = student.mathMark;
                stipendBox.IsChecked = student.stipend;
            }
        }
        private void EndEdit(object sender, EventArgs e) => LoadGrid();
        private void Delete(object sender, EventArgs e)
        {
            Student student = (Student)studentsGrid.SelectedItem;
            command.CommandText = $"DELETE FROM PersonalData WHERE ID={student.ID};" +
                $"DELETE FROM Marks WHERE ID={student.ID};";
            command.ExecuteNonQuery();
            LoadGrid();
        }
        private void LoadGrid()
        {
            idBox.IsEnabled = true;
            editButton.IsEnabled = false;
            deleteButton.IsEnabled = false;
            enterData.Content = "Добавить";

            idBox.Text = null;
            nameBox.Text = null;
            phMarkBox.Text = null;
            mathMarkBox.Text = null;
            stipendBox.IsChecked = false;

            studentsGrid.Items.Clear();

            command.CommandText = "SELECT * FROM PersonalData, Marks " +
                "WHERE PersonalData.ID = Marks.ID";
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                var data = new Student
                {
                    ID = reader["ID"].ToString(),
                    name = reader["Name"].ToString(),
                    stipend = Convert.ToBoolean(int.Parse(reader["Stipend"].ToString())),
                    phMark = reader["Physic"].ToString(),
                    mathMark = reader["Math"].ToString()
                };
                studentsGrid.Items.Add(data);
            }
        }
        public class Student
        {
            public string ID { get; set; }
            public string name { get; set; }
            public bool stipend { get; set; }
            public string phMark { get; set; }
            public string mathMark { get; set; }
        }
    }
}
