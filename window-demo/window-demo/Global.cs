using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace window_demo
{
    /*
     * This is a Global class and can be used to save global variable across all forms in the application.  
     */
    public static class Global
    {
        private static string employeeId = "";
        private static bool isAdmin;
        public static string empname = "";

        public static string empId
        {
            get { return employeeId; }
            set { employeeId = value; }
        }
        public static bool isadmin
        {
            get { return isAdmin; }
            set { isAdmin = value; }
        }
        public static string empName
        {
            get { return empname; }
            set { empname = value; }
        }
        
    }
}
