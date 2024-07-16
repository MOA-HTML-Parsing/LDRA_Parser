﻿using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDRA_Parser.Model
{
    public class ViolationItem
    {
        public string ViolationNumber { get; set; }
        public string Location { get; set; }

        public bool IsSame(ViolationItem item)
        {

            if (this.ViolationNumber != item.ViolationNumber) return false;

            else if (this.Location != item.Location) return false;

            return true;

        }
    }

   
}
