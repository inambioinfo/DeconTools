﻿
namespace DeconTools.Backend.Core
{
    public class O16O18TargetedResultObject : TargetedResultBase
    {
        #region Constructors
        public O16O18TargetedResultObject() : base() { }

        public O16O18TargetedResultObject(TargetBase target) : base(target) { }
        #endregion

        #region Properties

        public double RatioO16O18 { get; set; }
        public double IntensityI4Adjusted { get; set; }
        #endregion

  
   

        
    }
}
