﻿using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class ResultCollection
    {
        #region Constructors
        public ResultCollection(Run run)
        {
            this.run = run;
            ResultList = new List<IsosResult>();
            MassTagResultList = new Dictionary<TargetBase, TargetedResultBase>();
            scanResultList = new List<ScanResult>();
            msPeakResultsGroupedAndMzOrdered = new Dictionary<int, List<MSPeakResult>>();
            MSPeakResultList = new List<MSPeakResult>();
            m_IsosResultBin = new List<IsosResult>(10);
            logMessageList = new List<string>();
            ElutingPeakCollection = new List<ElutingPeak>();
        }

        #endregion

        #region Properties


        public Dictionary<TargetBase, TargetedResultBase> MassTagResultList { get; set; }


        private List<MSPeakResult> mSPeakResultList;
        public List<MSPeakResult> MSPeakResultList
        {
            get => mSPeakResultList;
            set
            {
                mSPeakResultList = value;
                msPeakResultsGroupedAndMzOrdered.Clear();
            }
        }

        private IList<IsosResult> m_IsosResultBin;
        public IList<IsosResult> IsosResultBin
        {
            get => m_IsosResultBin;
            set => m_IsosResultBin = value;
        }


        private List<ScanResult> scanResultList;
        public List<ScanResult> ScanResultList
        {
            get => scanResultList;
            set => scanResultList = value;
        }

        public List<IsosResult> ResultList { get; set; }


        private Run run;
        public Run Run
        {
            get => run;
            set => run = value;
        }

        private List<string> logMessageList;
        public List<string> LogMessageList
        {
            get => logMessageList;
            set => logMessageList = value;
        }

        private List<ElutingPeak> m_ElutingPeakCollection;
        public List<ElutingPeak> ElutingPeakCollection
        {
            get => m_ElutingPeakCollection;
            set => m_ElutingPeakCollection = value;
        }

        public int MSFeatureCounter { get; set; }

        //public Globals.IsosResultType ResultType { get; set; }

        public Globals.ResultType ResultType { get; set; }

        public int PeakCounter { get; set; }

        private Dictionary<int, List<MSPeakResult>> msPeakResultsGroupedAndMzOrdered;
        #endregion

        #region Public Methods
        public ScanResult GetCurrentScanResult()
        {
            if (scanResultList == null || scanResultList.Count == 0) return null;
            return scanResultList[scanResultList.Count - 1];
        }

        public Dictionary<int, List<MSPeakResult>> GetMsPeakResultsGroupedAndMzOrdered()
        {
            if(msPeakResultsGroupedAndMzOrdered == null || !msPeakResultsGroupedAndMzOrdered.Any())
            {
                msPeakResultsGroupedAndMzOrdered = new Dictionary<int, List<MSPeakResult>>();

                if(mSPeakResultList != null)
                {
                    // Group by scan number
                    foreach (var grouping in mSPeakResultList.GroupBy(x => x.Scan_num))
                    {
                        // Order by m/z
                        var msPeakResultList = grouping.OrderBy(x => x.MSPeak.XValue).ToList();
                        msPeakResultsGroupedAndMzOrdered.Add(grouping.Key, msPeakResultList);
                    }
                }
            }

            return msPeakResultsGroupedAndMzOrdered;
        }

        public int getTotalIsotopicProfiles()
        {
            if (ScanResultList == null) return 0;

            var totIsotopicProfiles = 0;
            foreach (var scanResult in ScanResultList)
            {
                totIsotopicProfiles += scanResult.NumIsotopicProfiles;

            }
            return totIsotopicProfiles;
        }


        public TargetedResultBase CurrentTargetedResult { get; set; }



        public TargetedResultBase GetTargetedResult(TargetBase target)
        {
            if (target==null)
            {
                throw new NullReferenceException("Tried to get the TargetResult, but Target is null");
            }

            if (MassTagResultList.ContainsKey(target))
            {
                CurrentTargetedResult = MassTagResultList[target];
                return CurrentTargetedResult;
            }

            var result = CreateMassTagResult(target);   // this creates the appropriate type and adds it to the MassTagResultList and increments the MSFeatureID number
            CurrentTargetedResult = result;
            return result;
        }

        //This is the primary way to add an IsosResult
        public void AddIsosResult(IsosResult addedResult)
        {
            addedResult.MSFeatureID = MSFeatureCounter;
            MSFeatureCounter++;    // by placing it here, we make the MSFeatureID a zero-based ID, as Kevin requested in an email (Jan 20/2010)
            IsosResultBin.Add(addedResult);
        }


        public void ClearAllResults()
        {
            IsosResultBin.Clear();
            MSPeakResultList.Clear();
            ResultList.Clear();
            ScanResultList.Clear();
            MassTagResultList.Clear();
            msPeakResultsGroupedAndMzOrdered.Clear();
        }

        public TargetedResultBase CreateMassTagResult(TargetBase massTag)
        {
            TargetedResultBase result;

            switch (ResultType)
            {
                case Globals.ResultType.BASIC_TARGETED_RESULT:
                    result = new MassTagResult(massTag);
                    break;
                case Globals.ResultType.N14N15_TARGETED_RESULT:
                    result = new N14N15_TResult(massTag);
                    break;
                case Globals.ResultType.O16O18_TARGETED_RESULT:
                    result = new O16O18TargetedResultObject(massTag);
                    break;
                case Globals.ResultType.SIPPER_TARGETED_RESULT:
                    result = new SipperLcmsTargetedResult(massTag);
                    break;
                case Globals.ResultType.TOPDOWN_TARGETED_RESULT:
                    result = new TopDownTargetedResult(massTag);
                    break;
                case Globals.ResultType.DEUTERATED_TARGETED_RESULT:
                    result = new DeuteratedTargetedResultObject(massTag);
                    break;
                default:
                    result = new MassTagResult(massTag);
                    break;
            }

            MassTagResultList.Add(massTag, result);
            result.MSFeatureID = MSFeatureCounter;
            result.Score = 1;
            result.Run = Run;

            MSFeatureCounter++;
            return result;
        }

        //TODO: delete this if sure not used
        //public MassTagResultBase AddMassTagResult(Globals.ResultType ResultType)
        //{
        //    switch (ResultType)
        //    {
        //        case Globals.ResultType.BASIC_TARGETED_RESULT:
        //            return new DeconTools.Backend.Core.MassTagResult();
        //        case Globals.ResultType.N14N15_TARGETED_RESULT:
        //            return new N14N15_TResult();
        //        default:
        //            return new DeconTools.Backend.Core.MassTagResult();
        //    }


        //}

        public List<TargetedResultBase> GetMassTagResults()
        {
            return MassTagResultList.Values.ToList();
        }


        public List<TargetedResultBase> GetSuccessfulMassTagResults()
        {

            //first collect all massTagIDs   (there are more than one massTag having the same ID - because there are multiple charge states for each ID

            var resultList = MassTagResultList.Values.ToList();
            var massTagIDs = new HashSet<int>();
            foreach (var result in resultList)
            {
                massTagIDs.Add(result.Target.ID);
            }
            var filteredResults = new List<TargetedResultBase>(massTagIDs.Count);

            foreach (var mtID in massTagIDs)
            {
                var tempResults = resultList.Where(p => p.Score < 0.15 && p.Target.ID == mtID).ToList();
                if (tempResults.Count > 0)
                {
                    filteredResults.Add(tempResults.OrderByDescending(p => p.Score).First());
                }
            }

            return filteredResults;




        }

        public void FillMSPeakResults()
        {
            if (Run is UIMFRun uimfRun)
            {

                foreach (var item in Run.PeakList)
                {
                    var peak = (MSPeak)item;
                    PeakCounter++;
                    var peakResult = new MSPeakResult(PeakCounter, uimfRun.CurrentScanSet.PrimaryScanNumber,
                                                      uimfRun.CurrentIMSScanSet.PrimaryScanNumber, peak);
                    MSPeakResultList.Add(peakResult);
                }
            }
            else
            {
                foreach (var item in Run.PeakList)
                {
                    var peak = (MSPeak)item;
                    PeakCounter++;
                    var peakResult = new MSPeakResult(PeakCounter, Run.CurrentScanSet.PrimaryScanNumber, peak);
                    MSPeakResultList.Add(peakResult);
                }
            }

        }


        #endregion

        #region Internal Methods
        internal IsosResult CreateIsosResult()
        {
            IsosResult result;

            switch (ResultType)
            {
                case Globals.ResultType.BASIC_TRADITIONAL_RESULT:
                    result = new StandardIsosResult(Run, Run.CurrentScanSet);
                    break;
                case Globals.ResultType.UIMF_TRADITIONAL_RESULT:
                    Check.Require(Run is UIMFRun, "Tried to create an IMS_TRADITIONAL_RESULT but the Dataset is not a UIMF file.");
                    var uimfRun = (UIMFRun)run;
                    result = new UIMFIsosResult(Run, uimfRun.CurrentScanSet, uimfRun.CurrentIMSScanSet);
                    break;
                case Globals.ResultType.O16O18_TRADITIONAL_RESULT:
                    result = new O16O18IsosResult(Run, Run.CurrentScanSet);
                    break;
                case Globals.ResultType.IMS_TRADITIONAL_RESULT:
                    result = new StandardIsosResult(Run, Run.CurrentScanSet);
                    break;
                case Globals.ResultType.BASIC_TARGETED_RESULT:
                    throw new ApplicationException("ResultType is a Targeted type but currently we are trying to create a Traditional result");
                case Globals.ResultType.O16O18_TARGETED_RESULT:
                    throw new ApplicationException("ResultType is a Targeted type but currently we are trying to create a Traditional result");
                case Globals.ResultType.N14N15_TARGETED_RESULT:
                    throw new ApplicationException("ResultType is a Targeted type but currently we are trying to create a Traditional result");
                case Globals.ResultType.DEUTERATED_TARGETED_RESULT:
                    throw new ApplicationException("ResultType is a Targeted type but currently we are trying to create a Traditional result");
                default:
                    throw new ApplicationException("ResultType is not of a know type: " + ResultType);

            }


            return result;
        }



        internal void AddLog(string logMessage)
        {
            logMessageList.Add(logMessage);
        }


        internal static List<IsosResult> getIsosResultsForCurrentScanSet(ResultCollection results)
        {
            Check.Require(results != null, "Can't retrieve IsosResults. Input list is null");

            if (results == null)
                return null;

            var queryList = from n in results.ResultList
                            where n.ScanSet == results.Run.CurrentScanSet
                            select n;


            return queryList.ToList();
        }


        #endregion

    }
}
