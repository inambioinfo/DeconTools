﻿#if !Disable_DeconToolsV2
using System;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Runs
{
    [Serializable]
    public abstract class DeconToolsRun : Run
    {
        public DeconToolsV2.Readers.clsRawData RawData { get; set; }

        protected DeconToolsRun()
        {
            XYData = new XYData();
        }

        public override XYData XYData {get;set;}


        public override int GetNumMSScans()
        {
            if (RawData == null) return 0;
            return RawData.GetNumScans();
        }

        public override string GetScanInfo(int scanNum)
        {
            if (RawData == null)
            {
                return base.GetScanInfo(scanNum);
            }
            else
            {
                return RawData.GetScanDescription(scanNum);
            }
        }

        public override double GetTime(int scanNum)
        {
            return RawData.GetScanTime(scanNum);
        }

        public override int GetMSLevelFromRawData(int scanNum)
        {
            try
            {
                return RawData.GetMSLevel(scanNum);

            }
            catch (Exception ex)
            {
                if (scanNum > GetMaxPossibleLCScanNum())
                {
                    throw new ArgumentOutOfRangeException(nameof(scanNum), "Failed to get MS level. Input scan was greater than dataset's max scan.");

                }

                throw;
            }
        }

        public override XYData GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            throw new NotImplementedException();
        }
    }
}
#endif
