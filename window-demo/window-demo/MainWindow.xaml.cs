using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;

namespace window_demo
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }
        /*
         * Later i need to remove the CancelClick . 
         */
        private void cancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void okClick(object sender, RoutedEventArgs e)
        {
            if (passwordBox.Password.Length == 0)
            {
                errormessage.Text = "Please enter your password.";
                passwordBox.Focus();
            }

            if (employeeIDBox.Text.Length == 0)
            {
                errormessage.Text = "Please enter your Employee ID.";
                employeeIDBox.Focus();
            }

            else
            {

                if (passwordBox.Password.Length == 0)
                {
                    errormessage.Text = "Please enter your password.";
                    passwordBox.Focus();
                }

                else
                {
                    Global.empId = employeeIDBox.Text; //get Id from form
                    string password = passwordBox.Password; //get password from form


                    String str = @"server=localhost;database=users;userid=root;password=;";
                    MySqlConnection con = null;
                    MySqlDataReader reader = null;

                    String pass = null;
                    try
                    {
                        con = new MySqlConnection(str);
                        con.Open(); //open the connection

                        MySqlCommand cmdOne = new MySqlCommand("SELECT Password, isAdmin, EmployeeId, UserName FROM employeetable WHERE EmployeeId=" + Global.empId, con);

                        cmdOne.ExecuteNonQuery();
                        reader = cmdOne.ExecuteReader();

                        while (reader.Read())
                        {

                            pass = reader.GetString(0);
                            bool admin = reader.GetBoolean(1);
                            Global.isadmin = admin;
                            String Id = reader.GetString(2);
                            String userName = reader.GetString(3);
                            Global.empName = userName;

                            if (pass == password && admin == true)
                            {

                                MessageBox.Show("Welcome " + userName + "!");
                                AdminWindow window = new AdminWindow();
                                Close();
                                window.setCreatingForm = this;
                                window.Show();

                            }
                            else if(pass != password)
                            {
                                errormessage.Text = "You have entered the wrong password, try again";
                                passwordBox.Focus();

                            }
                            
                            else if (pass == password && admin == false)
                            {
                                MessageBox.Show("Welcome " + userName + "!");
                                UserWindow userWindow = new UserWindow();
                                Close();
                                userWindow.Show();
                            }
                        }

                        if (pass == null)
                        {
                            errormessage.Text = "Employee ID does not exist, try again";
                        }

                    }
                    catch (MySqlException err) //capture and display any MySql errors that will occur
                    {
                        // Exception is connecting to the database 
                        //MessageBox.Show("Error: " + err.ToString());
                        MessageBox.Show("There is some error trying to connect to the database");
                    }
                    finally
                    {
                        if (con != null)
                        {
                            con.Close(); //safely close the connection
                        }
                    }

                }

            }
        }
    }
}
