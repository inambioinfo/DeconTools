﻿using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public class UIMFScanResult_TextFileExporter : ScanResult_TextFileExporter
    {
   
        #region Constructors
        public UIMFScanResult_TextFileExporter(string fileName) : base(fileName) { }
        #endregion




        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        protected override string buildScansResultOutput(ScanResult result)
        {
            StringBuilder sb = new StringBuilder();

            UimfScanResult uimfScanResult = (UimfScanResult)result;


            //sb.Append(uimfScanResult.Frameset.PrimaryFrame);

            //we want to report the unique 'FrameNum', not the non-unique 'Frame_index');
            sb.Append(uimfScanResult.LCScanNum);
            sb.Append(Delimiter);
            sb.Append(uimfScanResult.ScanTime.ToString("0.###"));
            sb.Append(Delimiter);
            sb.Append(result.SpectrumType);
            sb.Append(Delimiter);
            sb.Append(uimfScanResult.BasePeak.Height);
            sb.Append(Delimiter);
            sb.Append(uimfScanResult.BasePeak.XValue.ToString("0.#####"));
            sb.Append(Delimiter);
            sb.Append(uimfScanResult.TICValue);
            sb.Append(Delimiter);
            sb.Append(uimfScanResult.NumPeaks);
            sb.Append(Delimiter);
            sb.Append(uimfScanResult.NumIsotopicProfiles);
            sb.Append(Delimiter);
            sb.Append(uimfScanResult.FramePressureUnsmoothed.ToString("0.#####"));
            sb.Append(Delimiter);
            sb.Append(uimfScanResult.FramePressureSmoothed.ToString("0.#####"));

            return sb.ToString();


        }

        protected override string buildHeaderLine()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("frame_num");
            sb.Append(Delimiter);
            sb.Append("frame_time");
            sb.Append(Delimiter);
            sb.Append("type");
            sb.Append(Delimiter);
            sb.Append("bpi");
            sb.Append(Delimiter);
            sb.Append("bpi_mz");
            sb.Append(Delimiter);
            sb.Append("tic");
            sb.Append(Delimiter);
            sb.Append("num_peaks");
            sb.Append(Delimiter);
            sb.Append("num_deisotoped");
            sb.Append(Delimiter);
            sb.Append("FramePressureUnsmoothed");
            sb.Append(Delimiter);
            sb.Append("FramePressureSmoothed");
           

            return sb.ToString();
        }


     
    }
}
