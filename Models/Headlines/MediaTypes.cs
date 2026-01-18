using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMK.Models.Headlines
{
    [Table("MediaTypes")]
    public class MediaTypes
    {
        [PrimaryKey, AutoIncrement]
        public int ID_medType { get; set; }

        [NotNull]
        public string TypeName {  get; set; }
    }
}
