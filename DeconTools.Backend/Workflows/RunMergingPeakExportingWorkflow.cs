﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.Workflows
{
    public class RunMergingPeakExportingWorkflow : ScanBasedWorkflow
    {
      
        private int _datasetCounter;
        private int _peaksProcessedInLastDataset;
        private int _totalPeaksProcessed;

        private const int NumScansBetweenProgress = 1;


        #region Constructors



        public RunMergingPeakExportingWorkflow(OldDecon2LSParameters parameters, IEnumerable<string> datasetFileNameList, string outputFolderPath = null, BackgroundWorker backgroundWorker = null)
            : base(null, null, null, null)
        {
            OldDecon2LsParameters = parameters;
            OldDecon2LsParameters.PeakProcessorParameters.WritePeaksToTextFile = true;

            DatasetFileNameList = datasetFileNameList;

            OutputFolderPath = outputFolderPath;

            BackgroundWorker = backgroundWorker;

        }


        #endregion

        public IEnumerable<string> DatasetFileNameList { get; set; }

        public override void InitializeWorkflow()
        {
            Check.Assert(OldDecon2LsParameters != null, "Cannot initialize workflow. Parameters are null");

         
            ExportData = true;

            InitializeParameters();

      
            if (Run != null)
            {
                CreateOutputFileNames();
                InitializeProcessingTasks();

                PeakListExporter.TriggerToWriteValue = 1;  //put this very low so that peaks are exported after every scan and MSPeakResults are cleared

                ExecutePreprocessHook();
                WriteProcessingInfoToLog();
            }
        }


        public override void Execute()
        {
            InitializeWorkflow();

            IterateOverDatasets();
        }

        private void IterateOverDatasets()
        {
            _datasetCounter = 0;
            _peaksProcessedInLastDataset = 0;
            _totalPeaksProcessed = 0;

            WorkflowStats = new WorkflowStats();
            WorkflowStats.TimeStarted = DateTime.Now;

            foreach (var datasetFileName in DatasetFileNameList)
            {
                _peaksProcessedInLastDataset = 0;

                Run = new RunFactory().CreateRun(datasetFileName);

                Check.Require(Run!=null, "ERROR: tried to initialize Run but failed.");

                if (_datasetCounter == 0)
                {
                    InitializeWorkflow();
                }

                CreateTargetMassSpectra();

                IterateOverScans();

                _totalPeaksProcessed += _peaksProcessedInLastDataset;
                

                _datasetCounter++;

            }

            WorkflowStats.TimeFinished = DateTime.Now;
            WorkflowStats.NumFeatures = _totalPeaksProcessed;

            WriteOutSummaryToLogfile();

        }



        protected override void WriteOutSummaryToLogfile()
        {
            Logger.Instance.AddEntry("Finished file processing", Logger.Instance.OutputFilename);

            string formattedOverallprocessingTime = string.Format("{0:00}:{1:00}:{2:00}",
                WorkflowStats.ElapsedTime.Hours, WorkflowStats.ElapsedTime.Minutes, WorkflowStats.ElapsedTime.Seconds);

            Logger.Instance.AddEntry("total processing time = " + formattedOverallprocessingTime);
            Logger.Instance.AddEntry("total features = " + WorkflowStats.NumFeatures);
            Logger.Instance.AddEntry("Peak data written to: " + PeakListOutputFileName);
            Logger.Instance.WriteToFile(Logger.Instance.OutputFilename);
            Logger.Instance.Close();
        }


        protected override void IterateOverScans()
        {
            foreach (ScanSet scanset in Run.ScanSetCollection.ScanSetList)
            {
                Run.ResultCollection.MSPeakResultList.Clear();

                Run.CurrentScanSet = scanset;


                ExecuteProcessingTasks();



                if (BackgroundWorker != null)
                {
                    if (BackgroundWorker.CancellationPending)
                    {
                        return;
                    }

                }
                ReportProgress();


            }
        }


        protected override void CreateOutputFileNames()
        {
            base.CreateOutputFileNames();

            PeakListOutputFileName = PeakListOutputFileName.Replace("_peaks.txt", "_merged_peaks.txt");

        }


        protected override void ExecuteProcessingTasks()
        {
           

            
            ExecuteTask(MSGenerator);

            if (OldDecon2LsParameters.HornTransformParameters.ZeroFill)
            {
                ExecuteTask(ZeroFiller);
            }

            if (OldDecon2LsParameters.HornTransformParameters.UseSavitzkyGolaySmooth)
            {
                ExecuteTask(Smoother);
            }

            ExecuteTask(PeakDetector);

            foreach (var peak in Run.ResultCollection.MSPeakResultList)
            {
                peak.Scan_num = peak.Scan_num + Run.ScanSetCollection.ScanSetList.Count * _datasetCounter;
            }

            _peaksProcessedInLastDataset += Run.ResultCollection.MSPeakResultList.Count;

            PeakListExporter.Execute(Run.ResultCollection);
        }



      

        public override void ReportProgress()
        {
            if (Run.ScanSetCollection == null || Run.ScanSetCollection.ScanSetList.Count == 0) return;

            ScanBasedProgressInfo userstate = new ScanBasedProgressInfo(Run, Run.CurrentScanSet, null);

            float percentDone = (_datasetCounter+1) / (float)(DatasetFileNameList.Count()) * 100;
            userstate.PercentDone = percentDone;

            string logText = "Dataset= \t" + Run.DatasetName + "; PercentComplete= \t" + percentDone.ToString("0.0") + "; Total peaks= \t" + _peaksProcessedInLastDataset;

            if (BackgroundWorker != null)
            {
                BackgroundWorker.ReportProgress((int)percentDone, userstate);
            }

            if (_datasetCounter % NumScansBetweenProgress == 0)
            {
                Logger.Instance.AddEntry(logText, Logger.Instance.OutputFilename);

                if (BackgroundWorker == null)
                {
                    Console.WriteLine(DateTime.Now + "\t" + logText);
                }

            }
        }
    }
}
