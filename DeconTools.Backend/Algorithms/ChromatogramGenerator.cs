﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;
using PNNLOmics.Generic;

namespace DeconTools.Backend.Algorithms
{
    public class ChromatogramGenerator
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        
      

  

        /// <summary>
        /// Generates chromatogram based on a single m/z value and a given tolerance for a range of scans. 
        /// </summary>
        /// <param name="msPeakList"></param>
        /// <param name="minScan"></param>
        /// <param name="maxScan"></param>
        /// <param name="targetMZ"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public XYData GenerateChromatogram(List<MSPeakResult> msPeakList, int minScan, int maxScan, double targetMZ, double tolerance, Globals.ToleranceUnit toleranceUnit = Globals.ToleranceUnit.PPM)
        {
            List<double> targetMZList = new List<double>();
            targetMZList.Add(targetMZ);

            return GenerateChromatogram(msPeakList, minScan, maxScan, targetMZList, tolerance, toleranceUnit);
        }

        public XYData GenerateChromatogram(List<MSPeakResult> msPeakList, int minScan, int maxScan, double targetMZ, double tolerance, int chromIDToAssign, Globals.ToleranceUnit toleranceUnit = Globals.ToleranceUnit.PPM)
        {
            List<double> targetMZList = new List<double>();
            targetMZList.Add(targetMZ);

            return GenerateChromatogram(msPeakList, minScan, maxScan, targetMZList, tolerance, chromIDToAssign, toleranceUnit);
        }

        /// <summary>
        /// Will generate a chromatogram that is in fact a combination of chromatograms based on user-supplied target m/z values. 
        /// This is geared for producing a chromatogram for an isotopic profile, but only using narrow mass ranges
        /// that encompass individual peaks of an isotopic profile. 
        /// </summary>
        /// <param name="msPeakList"></param>
        /// <param name="minScan"></param>
        /// <param name="maxScan"></param>
        /// <param name="targetMZList"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public XYData GenerateChromatogram(List<MSPeakResult> msPeakList, int minScan, int maxScan, List<double> targetMZList, double tolerance, Globals.ToleranceUnit toleranceUnit = Globals.ToleranceUnit.PPM)
        {
            int defaultChromID = 0;

            return GenerateChromatogram(msPeakList, minScan, maxScan, targetMZList, tolerance, defaultChromID, toleranceUnit);
        }

        //TODO:  make a ChromatogramObject that will help handle my MSPeakResults, etc.

        public XYData GenerateChromatogram(List<MSPeakResult> msPeakList, int minScan, int maxScan, List<double> targetMZList, double tolerance, int chromIDToAssign, Globals.ToleranceUnit toleranceUnit = Globals.ToleranceUnit.PPM)
        {
            Check.Require(msPeakList != null && msPeakList.Count > 0, "Cannot generate chromatogram. Source msPeakList is empty or hasn't been defined.");

            int scanTolerance = 5;     // TODO:   keep an eye on this

            int indexOfLowerScan = getIndexOfClosestScanValue(msPeakList, minScan, 0, msPeakList.Count - 1, scanTolerance);
            int indexOfUpperScan = getIndexOfClosestScanValue(msPeakList, maxScan, 0, msPeakList.Count - 1, scanTolerance);

            XYData chromData = null;

            foreach (var targetMZ in targetMZList)
            {
                double lowerMZ;
                double upperMZ;

                if (toleranceUnit==Globals.ToleranceUnit.PPM)
                {
                    lowerMZ = targetMZ - tolerance * targetMZ / 1e6;
                    upperMZ = targetMZ + tolerance * targetMZ / 1e6;                    
                }
                else if (toleranceUnit == Globals.ToleranceUnit.MZ)
                {
                    lowerMZ = targetMZ - tolerance;
                    upperMZ = targetMZ + tolerance;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Trying to create chromatogram, but the "  + toleranceUnit +" unit isn't supported");
                }

				List<MSPeakResult> tempPeakList = new List<MSPeakResult>();

				for (int i = indexOfLowerScan; i <= indexOfUpperScan; i++)
				{
					MSPeakResult msPeakResult = msPeakList[i];
					double xValue = msPeakResult.MSPeak.XValue;

					if (xValue >= lowerMZ && xValue <= upperMZ)
					{
						tempPeakList.Add(msPeakResult);
					}
				}

            	if (!tempPeakList.Any())
                {
                    //TODO: we want to return 0 intensity values. But need to make sure there are no downstream problems with this change. 
                }
                else
                {
                    XYData currentChromdata = getChromDataAndFillInZerosAndAssignChromID(tempPeakList, chromIDToAssign);
                    chromData = AddCurrentXYDataToBaseXYData(chromData, currentChromdata);
                }
            }

            return chromData;
        }


        public XYData GenerateChromatogramFromRawData(Run run, int minScan, int maxScan, double targetMZ, double toleranceInPPM)
        {
            XYData xydata = new XYData();

            List<double> xvals = new List<double>();
            List<double> yvals = new List<double>();

            for (int scan = minScan; scan <= maxScan; scan++)
            {
                bool scanIsGoodToGet = true;

                bool currentScanContainsMSMS = (run.ContainsMSMSData && run.GetMSLevel(scan) > 1);
                if (currentScanContainsMSMS)
                {
                    scanIsGoodToGet = false;
                }

                if (!scanIsGoodToGet)
                {
                    continue;
                }

                ScanSet scanset = new ScanSet(scan);
                run.GetMassSpectrum(scanset);

                double chromDataPointIntensity = getChromDataPoint(run.XYData, targetMZ, toleranceInPPM);

                xvals.Add(scan);
                yvals.Add(chromDataPointIntensity);
            }

            xydata.Xvalues = xvals.ToArray();
            xydata.Yvalues = yvals.ToArray();

            return xydata;
        }

        private double getChromDataPoint(XYData xydata, double targetMZ, double toleranceInPPM)
        {
            bool dataIsEmpty = (xydata == null || xydata.Xvalues.Length == 0);
            if (dataIsEmpty)
            {
                return 0;
            }

            double toleranceInMZ = toleranceInPPM * targetMZ / 1e6;

            double lowerMZ = targetMZ - toleranceInMZ;
            double upperMZ = targetMZ + toleranceInMZ;

            double startingPointMZ = lowerMZ - 2;

            int indexOfGoodStartingPoint = MathUtils.BinarySearchWithTolerance(xydata.Xvalues, startingPointMZ, 0, xydata.Xvalues.Length - 1, 1.9);

            if (indexOfGoodStartingPoint == -1)
            {
                indexOfGoodStartingPoint = 0;
            }

            double intensitySum = 0;

            for (int i = indexOfGoodStartingPoint; i < xydata.Xvalues.Length; i++)
            {
                if (xydata.Xvalues[i] >= lowerMZ)
                {

                    if (xydata.Xvalues[i] > upperMZ)
                    {
                        break;
                    }
                    else
                    {
                        intensitySum = +xydata.Yvalues[i];
                    }
                }
            }

            return intensitySum;
        }



        [Obsolete("Use the other GeneratePeakChromatogram!")]
        public List<MSPeakResult> GeneratePeakChromatogram(List<MSPeakResult> msPeakList, int minScan, int maxScan, List<double> targetMZList, double toleranceInPPM)
        {
            int scanTolerance = 5;     // TODO:   keep an eye on this

            int indexOfLowerScan = getIndexOfClosestScanValue(msPeakList, minScan, 0, msPeakList.Count - 1, scanTolerance);
            int indexOfUpperScan = getIndexOfClosestScanValue(msPeakList, maxScan, 0, msPeakList.Count - 1, scanTolerance);

            int currentIndex = indexOfLowerScan;
            List<MSPeakResult> filteredPeakList = new List<MSPeakResult>();
            while (currentIndex <= indexOfUpperScan)
            {
                filteredPeakList.Add(msPeakList[currentIndex]);
                currentIndex++;
            }

            List<MSPeakResult> compiledChromPeakList = new List<MSPeakResult>();

            int counter = 0;
            foreach (var targetMZ in targetMZList)
            {
                counter++;
                double lowerMZ = targetMZ - toleranceInPPM * targetMZ / 1e6;
                double upperMZ = targetMZ + toleranceInPPM * targetMZ / 1e6;

                List<MSPeakResult> tempPeakList = filteredPeakList.Where(p => p.MSPeak.XValue >= lowerMZ && p.MSPeak.XValue <= upperMZ).ToList();

                compiledChromPeakList.AddRange(tempPeakList);
            }

            if (counter > 1) // if the list contains multiple peak chromatograms, then need to sort.  Otherwise, don't need to sort.
            {
                compiledChromPeakList.Sort(delegate(MSPeakResult peak1, MSPeakResult peak2)
                {
                    return peak2.Scan_num.CompareTo(peak1.Scan_num);
                });
            }

            return compiledChromPeakList;
        }

        [Obsolete("Use the other GeneratePeakChromatogram!")]
        public List<MSPeakResult> GeneratePeakChromatogram(List<MSPeakResult> msPeakList, int minScan, int maxScan, double targetMZ, double toleranceInPPM)
        {
            List<double> targetMZList = new List<double>();
            targetMZList.Add(targetMZ);

            return GeneratePeakChromatogram(msPeakList, minScan, maxScan, targetMZList, toleranceInPPM);
        }

        private XYData getChromDataAndFillInZerosAndAssignChromID(List<MSPeakResult> filteredPeakList, int chromID)
        {
			int filteredPeakListCount = filteredPeakList.Count;

            int leftZeroPadding = 200;   //number of scans to the left of the minscan for which zeros will be added
            int rightZeroPadding = 200;   //number of scans to the left of the minscan for which zeros will be added

            int peakListMinScan = filteredPeakList[0].Scan_num;
            int peakListMaxScan = filteredPeakList[filteredPeakList.Count - 1].Scan_num;

            //will pad min and max scans with zeros, and add zeros in between. This allows smoothing to execute properly

            peakListMinScan = peakListMinScan - leftZeroPadding;
            peakListMaxScan = peakListMaxScan + rightZeroPadding;

            if (peakListMinScan < 0) peakListMinScan = 0;

            //populate array with zero intensities.
            SortedDictionary<int, double> xyValues = new SortedDictionary<int, double>();
            for (int i = peakListMinScan; i <= peakListMaxScan; i++)
            {
            	xyValues[i] = 0;
            }

            //iterate over the peaklist, assign chromID,  and extract intensity values
			for (int i = 0; i < filteredPeakListCount; i++)
            {
                MSPeakResult peakResult = filteredPeakList[i];

                //NOTE:   we assign the chromID here. 
                peakResult.ChromID = chromID;

                double intensity = peakResult.MSPeak.Height;
            	int scanNumber = peakResult.Scan_num;

                //because we have tolerances to filter the peaks, more than one m/z peak may occur for a given scan. So will take the most abundant...
                
                if (!xyValues.ContainsKey(scanNumber))
                {
					string errorString = "Unexpected error in chromatogram generator!! Scan= " + scanNumber +
										 "; num filtered peaks = " + filteredPeakListCount;

                    Console.WriteLine(errorString);
                    
                    throw new InvalidProgramException(errorString);
                }

				if (intensity > xyValues[scanNumber])
                {
					xyValues[scanNumber] = intensity;
                }
            }

            XYData outputXYData = new XYData();

            outputXYData.Xvalues = XYData.ConvertIntsToDouble(xyValues.Keys.ToArray());
            outputXYData.Yvalues = xyValues.Values.ToArray();

            return outputXYData;
        }

        private XYData AddCurrentXYDataToBaseXYData(XYData baseData, XYData newdata)
        {
            XYData returnedData = new XYData();

            if (baseData == null)
            {
                returnedData = newdata;
            }
            else
            {
                //this might need to be cleaned up   :)

                //first add the base data
                SortedDictionary<int, double> baseValues = new SortedDictionary<int, double>();
                for (int i = 0; i < baseData.Xvalues.Length; i++)
                {
                    baseValues.Add((int)baseData.Xvalues[i], baseData.Yvalues[i]);
                }

                //now combine base data with the new
                for (int i = 0; i < newdata.Xvalues.Length; i++)
                {
                    int scanToBeInserted = (int)newdata.Xvalues[i];
                    double intensityToBeInserted = newdata.Yvalues[i];

                    if (baseValues.ContainsKey(scanToBeInserted))
                    {
                        baseValues[scanToBeInserted] += intensityToBeInserted;
                    }
                    else
                    {
                        baseValues.Add(scanToBeInserted, intensityToBeInserted);
                    }

                }

                returnedData.Xvalues = XYData.ConvertIntsToDouble(baseValues.Keys.ToArray());
                returnedData.Yvalues = baseValues.Values.ToArray();

            }

            return returnedData;
        }

        #endregion

        #region Private Methods
        private int getIndexOfClosestScanValue(List<MSPeakResult> peakList, int targetScan, int leftIndex, int rightIndex, int scanTolerance)
        {
            if (leftIndex < rightIndex)
            {
                int middle = (leftIndex + rightIndex) / 2;

                if (Math.Abs(targetScan - peakList[middle].Scan_num) <= scanTolerance)
                {
                    return middle;
                }
                else if (targetScan < peakList[middle].Scan_num)
                {
                    return getIndexOfClosestScanValue(peakList, targetScan, leftIndex, middle - 1, scanTolerance);
                }
                else
                {
                    return getIndexOfClosestScanValue(peakList, targetScan, middle + 1, rightIndex, scanTolerance);
                }
            }
            else if (leftIndex == rightIndex)
            {
                {
                    return leftIndex;
                }
            }
            return leftIndex;    // if fails to find...  will return the inputted left-most scan
        }

        #endregion
    }
}
