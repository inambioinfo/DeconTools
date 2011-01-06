﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ResultValidators
{
    public class InterferenceScorer
    {

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xydata">the raw data including noise and non-noise</param>
        /// <param name="peakList">the peak list representing the non-noise</param>
        /// <param name="leftBoundary">the left most xvalue to be considered</param>
        /// <param name="rightBoundary">the right most xvalue to be considered</param>
        /// <param name="startIndex">the index of the xydata from which to begin searching - for improving performance. Default is '0'</param>
        /// <returns></returns>
        public double GetInterferenceScore(XYData xydata, List<MSPeak> peakList, double leftBoundary, double rightBoundary, int startIndex = 0)
        {
            
            int currentIndex = startIndex;
            if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            double sumIntensities = 0;
            double sumPeakIntensities = 0;
            int currentPeakIndex = 0;

            while (xydata.Xvalues[currentIndex] < rightBoundary && currentPeakIndex < peakList.Count)
            {

                bool isWithinRange = (!(xydata.Xvalues[currentIndex] < leftBoundary));

                if (isWithinRange)
                {
                    sumIntensities += xydata.Yvalues[currentIndex];

                    double sigma = peakList[currentPeakIndex].Width / 2.35;
                    double threeSigma = sigma * 3;

                    double leftPeakValue = peakList[currentPeakIndex].XValue - threeSigma;
                    double rightPeakValue = peakList[currentPeakIndex].XValue + threeSigma;

                    if (xydata.Xvalues[currentIndex] > leftPeakValue)
                    {

                        bool wentPastPeak = (xydata.Xvalues[currentIndex] > rightPeakValue);
                        if (wentPastPeak)
                        {
                            currentPeakIndex++;
                        }
                        else
                        {
                            sumPeakIntensities += xydata.Yvalues[currentIndex];
                        }

                    }
                    
                }
              

                currentIndex++;
                if (currentIndex >= xydata.Xvalues.Length) break;
            }

            double interferenceScore = 1 - (sumPeakIntensities / sumIntensities);
            return interferenceScore;
        }

        /// <summary>
        /// This calculates a score:  1- (I1/I2) where:
        /// I1= sum of intensities of non-noise peaks.
        /// I2 = sum of intensities of all peak.
        /// </summary>
        /// <param name="allPeaks">all peaks, including noise and non-noise</param>
        /// <param name="nonNoisePeaks">non-noise peaks. </param>
        /// <param name="leftBoundary"></param>
        /// <param name="rightBoundary"></param>
        /// <returns></returns>
        public double GetInterferenceScore(List<MSPeak> allPeaks, List<MSPeak> nonNoisePeaks, double leftBoundary, double rightBoundary)
        {
            return 0;
        }

        #endregion

        #region Private Methods

        #endregion


    }
}
