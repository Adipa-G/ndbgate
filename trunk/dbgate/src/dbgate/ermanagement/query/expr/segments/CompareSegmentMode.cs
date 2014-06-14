﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbgate.ermanagement.query.expr.segments
{
    public enum CompareSegmentMode
    {
        Eq,
	  	Neq,
	  	Gt,
	  	Ge,
	  	Lt,
	  	Le,
	  	Like,
        Between,
        In,
        Exists,
        NotExists
    }
}