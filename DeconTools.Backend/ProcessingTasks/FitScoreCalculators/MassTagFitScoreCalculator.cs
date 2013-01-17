﻿using DeconTools.Backend.Core;
using DeconTools.Backend;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.FitScoreCalculators
{
    public class MassTagFitScoreCalculator:Task
    {
        #region properties
        /// <summary>
        /// score experimental profile to labeled or unlabeled theoretical profile
        /// </summary>
        public DeconTools.Backend.Globals.IsotopicProfileType IsotopicProfileType { get; set; }

        public MassTagFitScoreCalculator()
        {
            IsotopicProfileType = DeconTools.Backend.Globals.IsotopicProfileType.UNLABELLED;
        }

        public MassTagFitScoreCalculator(DeconTools.Backend.Globals.IsotopicProfileType lableType)
        {
            IsotopicProfileType = lableType;
        }
        #endregion

        #region Public Methods
        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.Run.CurrentMassTag != null, this.Name + " failed; CurrentMassTag is empty");
            Check.Require(resultList.Run.XYData != null && resultList.Run.XYData.Xvalues != null && resultList.Run.XYData.Xvalues.Length > 0, this.Name + " failed; Run's XY data is empty. Need to Run an MSGenerator");

            IsotopicProfile theorProfile = new IsotopicProfile();
            switch (IsotopicProfileType)
            {
                case DeconTools.Backend.Globals.IsotopicProfileType.UNLABELLED:
                    Check.Require(resultList.Run.CurrentMassTag.IsotopicProfile != null, "Target's theoretical isotopic profile has not been established");
                    theorProfile = resultList.Run.CurrentMassTag.IsotopicProfile;
                    break;
                case DeconTools.Backend.Globals.IsotopicProfileType.LABELLED:
                    //Check.Require(resultList.Run.CurrentMassTag.IsotopicProfileLabelled != null, this.Name + " failed; Theor isotopic profile is empty. Run a TheorFeatureGenerator");
                    Check.Require(resultList.Run.CurrentMassTag.IsotopicProfileLabelled != null, "Target's labelled theoretical isotopic profile has not been established");
                    theorProfile = resultList.Run.CurrentMassTag.IsotopicProfileLabelled;
                    break;
                default:
                    theorProfile = resultList.Run.CurrentMassTag.IsotopicProfile;
                    break;
            }

            TargetedResultBase result = resultList.CurrentTargetedResult;
            Check.Require(result != null, "No MassTagResult has been generated for CurrentMassTag");

            if (result.IsotopicProfile == null || result.IsotopicProfile.Peaklist == null || result.IsotopicProfile.Peaklist.Count == 0)
            {
                result.Score = 1;   // this is the worst possible fit score. ( 0.000 is the best possible fit score);  Maybe we want to return a '-1' to indicate a failure...              
                return;
            }

            
            int indexOfMostAbundantTheorPeak = theorProfile.GetIndexOfMostIntensePeak();
            int indexOfCorrespondingObservedPeak = PeakUtilities.getIndexOfClosestValue(result.IsotopicProfile.Peaklist,
                theorProfile.getMostIntensePeak().XValue, 0, result.IsotopicProfile.Peaklist.Count - 1, 0.1);


            if (indexOfCorrespondingObservedPeak<0)      // most abundant peak isn't present in the actual theoretical profile... problem!
            {
                result.Score = 1;
                return;
            }

            double mzOffset = result.IsotopicProfile.Peaklist[indexOfCorrespondingObservedPeak].XValue - theorProfile.Peaklist[indexOfMostAbundantTheorPeak].XValue;

            XYData theorXYData = theorProfile.GetTheoreticalIsotopicProfileXYData(result.IsotopicProfile.GetFWHM());
            //theorXYData.Display();

            theorXYData.OffSetXValues(mzOffset);     //May want to avoid this offset if the masses have been aligned using LCMS Warp

            //theorXYData.Display();

            AreaFitter areafitter = new AreaFitter();
            double fitval = areafitter.GetFit(theorXYData, resultList.Run.XYData, 0.1);

            if (fitval == double.NaN || fitval > 1) fitval = 1;
            result.Score = fitval;
        }
        #endregion
    }
}
