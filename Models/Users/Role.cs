using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMK.Models.Users
{
    [Table("Role")]
    public class Role
    {
        [PrimaryKey, AutoIncrement]
        public int ID_role { get; set; }

        [NotNull]
        public string Title { get; set; }
    }
}
