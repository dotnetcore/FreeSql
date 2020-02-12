using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kwlib
{
    public class department_userinfo
    {
        public int departmentsId { get; set; }
        public int userinfoId { get; set; }

        [Navigate("departmentsId")]
        public departments dept { get; set; }


        [Navigate("userinfoId")]
        public userinfo emp { get; set; }
    }
}
