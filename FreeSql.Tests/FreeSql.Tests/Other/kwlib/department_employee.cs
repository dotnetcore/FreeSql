using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kwlib
{
    public class department_employee
    {
        public int departmentId { get; set; }
        public int employeeId { get; set; }

        [Navigate("departmentId")]
        public department dept { get; set; }
        [Navigate("employeeId")]
        public employee empe { get; set; }
    }
}
