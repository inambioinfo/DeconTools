﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.Backend.Utilities
{

	/// <summary>
	/// Used to dump data in a format where sipper can read it.
	/// </summary>
	public static class SipperDataDump
	{
		private static bool outputResults = false;
		public static bool OutputResults
		{
			get { return outputResults; }
			set { outputResults = value; }
		}

		public static string Outfile { get; set; }

		public static void DataDumpSetup(string filename)
		{
			OutputResults = true;
			Outfile = filename;
			if (File.Exists(Outfile))
			{
				File.Delete(Outfile);
			}

			using (StreamWriter sipper = File.AppendText(Outfile))
			{
				sipper.WriteLine("TargetID" + "\t" + "PRSM_ID" + "\t" + "ChargeState" + "\t" + "Code" + "\t" + "EmpiricalFormula" + "\t" + "MonoMZ" +
				                 "\t" + "MonoisotopicMass" + "\t" + "TargetScan" + "\t" + "ObservedScan" +
				                 "\t" + "Scan" + "\t" + "NETError" + "\t" + "MassError" + "\t" + "FitScore" + "\t" +
				                 "ChromCorrMedian" + "\t" + "ChromCorrAverage" + "\t" + "ChromCorrStdev" + "\t" +
				                 "CorrelationData" + "\t" + "Abundance" + "\t" + "Flagged" + "\t" + "Status");
			}
		}

		/// <summary>
		/// Temporary Data Dumping Point
		/// Will be removed when GUI is developed
		/// </summary>
		/// <param name="input"></param>
		public static void DataDump(IqTarget input, Run Run)
		{
			//Temporary Data Dumping Point
			//Data Dump also in TopDownIqTesting and ChromCorrelationData
			ChromPeakIqTarget target = input as ChromPeakIqTarget;
			string status = "UNK";
			IqResult result = target.GetResult();
			var parent = result.Target.ParentTarget as IqChargeStateTarget;
			if (parent.ObservedScan != -1)
			{
				if (((target.ChromPeak.XValue - target.ChromPeak.Width / 2) <= parent.ObservedScan) &&
					((target.ChromPeak.XValue + target.ChromPeak.Width / 2) >= parent.ObservedScan))
				{
					status = "T-POS";
				}
				else
				{
					status = "T-NEG";
				}
			}


			using (StreamWriter sipper = File.AppendText(Outfile))
			{
				sipper.WriteLine(target.ID + "\t" + parent.AlternateID + "\t" + target.ChargeState + "\t" + target.Code + "\t" + target.EmpiricalFormula + "\t" +
								 target.MZTheor.ToString("0.0000") + "\t" + target.MonoMassTheor + "\t" +
								 Run.GetScanValueForNET((float)target.ElutionTimeTheor) +
								 "\t" + parent.ObservedScan + "\t" + target.ChromPeak.XValue.ToString("0.00") + "\t" +
								 result.NETError.ToString("0.0000") + "\t" + result.MassError.ToString("0.0000") + "\t" +
								 result.FitScore.ToString("0.0000") + "\t" + result.CorrelationData.RSquaredValsMedian + "\t" +
								 result.CorrelationData.RSquaredValsAverage + "\t" + result.CorrelationData.RSquaredValsStDev + "\t" +
								 result.CorrelationData.GetCorrelationData() + "\t" + target.GetResult().Abundance + "\t" + result.IsIsotopicProfileFlagged + "\t" +
								 status);
			}
		}
	}
}