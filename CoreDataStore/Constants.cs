﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreDataStore
{
    [Flags]
    public enum DBColumOrderRecordType : byte
    {
        Add,
        Set,
        Get
    }
}
