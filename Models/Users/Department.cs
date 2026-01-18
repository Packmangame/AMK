using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMK.Models.Users
{
    [Table("Department")]
    public class Department
    {
        [PrimaryKey, AutoIncrement]
        public int ID_depart { get; set; }

        [NotNull]
        public string DepartmentName { get; set; }
    }
}
