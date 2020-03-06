using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql;

public class dept_user: BaseEntity<dept_user>
    {
        public int deptid { get; set; }
        public int userid { get; set; }

        [Navigate("deptid")]
        public DEPARTMENTS dept { get; set; }


        [Navigate("userid")]
        public userinfo emp { get; set; }
    }

