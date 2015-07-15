﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DeconTools.Backend.Data
{
    public abstract class Exporter<T>
    {

        protected abstract string headerLine { get; set; }

        protected abstract char delimiter { get; set; }

        public abstract void Export(T results);

        protected string DblToString(double value, byte digitsOfPrecision)
        {
            return PNNLOmics.Utilities.StringUtilities.DblToString(value, digitsOfPrecision);
        }
   
    }


}
