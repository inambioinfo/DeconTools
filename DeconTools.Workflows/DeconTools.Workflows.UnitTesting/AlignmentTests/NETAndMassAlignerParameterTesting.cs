﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Runs;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting
{
    [TestFixture]
    public class NETAndMassAlignerParameterTesting
    {
        [Test]
        public void AlignmentParameterTesting1()
        {
            Run run = new RunFactory().CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            string alignmentFeaturesFile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_READONLY_alignedFeatures.txt";

            UnlabelledTargetedResultFromTextImporter importer = new UnlabelledTargetedResultFromTextImporter(alignmentFeaturesFile);
            TargetedResultRepository repo = importer.Import();

            string massTagFile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\QCShew_Formic_MassTags_Bin10_all.txt";

            MassTagCollection mtc = new MassTagCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();


            NETAndMassAlignerParameters parameters = new NETAndMassAlignerParameters();
            NETAndMassAligner aligner = new NETAndMassAligner();

            aligner.SetFeaturesToBeAligned(repo.Results);
            aligner.SetReferenceMassTags(mtc.MassTagList);

       


            StringBuilder sb = new StringBuilder();
            sb.Append("theorMZ\tobsMZ\talignedMZ\tppmErrorBefore\tppmErrorAfter\n");


            parameters.MassCalibrationWindow = 50;
            parameters.MassCalibrationNumMassDeltaBins = 100;

            int[]massCalXSliceValues = {3,6,9,12,15,20,30,50};

            foreach (var xsliceVal in massCalXSliceValues)
            {
                List<double> ppmErrorsBefore = new List<double>();
                List<double> ppmErrorsAfter = new List<double>();

                parameters.MassCalibrationNumXSlices =(short)xsliceVal;
                aligner.AlignerParameters = parameters;
                aligner.Execute(run);

                foreach (var result in repo.Results)
                {
                    MassTag mt = mtc.MassTagList.Where(p => p.ID == result.MassTagID).Where(p => p.ChargeState == result.ChargeState).First();
                    double theorMZ = mt.MZ;
                    double obsMZ = result.MonoMZ;
                    double scan = result.ScanLC;
                    double alignedMZ = run.GetAlignedMZ(obsMZ, scan);
                    double ppmErrorBefore = (theorMZ - obsMZ) / theorMZ * 1e6;
                    double ppmErrorAfter = (theorMZ - alignedMZ) / theorMZ * 1e6;


                    sb.Append(result.MassTagID + "\t" + result.ScanLC + "\t" + theorMZ.ToString("0.00000") + "\t" + obsMZ.ToString("0.00000") + "\t" + alignedMZ.ToString("0.00000") + "\t" + ppmErrorBefore.ToString("0.0") + "\t" + ppmErrorAfter.ToString("0.0"));
                    sb.Append(Environment.NewLine);
                    

                    ppmErrorsAfter.Add(ppmErrorAfter);
                    ppmErrorsBefore.Add(ppmErrorBefore);
                }

                Console.WriteLine(xsliceVal + "\t" + ppmErrorsBefore.Average().ToString("0.00") + "\t"+ ppmErrorsAfter.Average().ToString("0.00"));

                //Console.WriteLine(sb.ToString());
                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine("Average ppm error before alignment = " + ppmErrorsBefore.Average().ToString("0.00"));
                //Console.WriteLine("Average ppm error after alignment = " + ppmErrorsAfter.Average().ToString("0.00"));

           
            }

      


        }


        [Test]
        public void AlignmentParameterTesting2()
        {


            Run run = new RunFactory().CreateRun(@"D:\Data\Orbitrap\Subissue01\QC_Shew_10_01-pt5-1_8Feb10_Doc_09-12-24.RAW");

            string alignmentFeaturesFile = @"\\protoapps\UserData\Slysz\Data\QCShew_MassiveTargeted\AlignmentInfo\QC_Shew_10_01-pt5-1_8Feb10_Doc_09-12-24_alignedFeatures.txt";

            UnlabelledTargetedResultFromTextImporter importer = new UnlabelledTargetedResultFromTextImporter(alignmentFeaturesFile);
            TargetedResultRepository repo = importer.Import();

            string massTagFile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\QCShew_Formic_MassTags_Bin10_all.txt";

            MassTagCollection mtc = new MassTagCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();


            NETAndMassAlignerParameters parameters = new NETAndMassAlignerParameters();
            NETAndMassAligner aligner = new NETAndMassAligner();

            aligner.SetFeaturesToBeAligned(repo.Results);
            aligner.SetReferenceMassTags(mtc.MassTagList);




            StringBuilder sb = new StringBuilder();
            sb.Append("mtid\tscanLC\ttheorMZ\tobsMZ\talignedMZ\tppmErrorBefore\tppmErrorAfter\n");


            parameters.MassCalibrationWindow = 20;
            parameters.MassCalibrationNumMassDeltaBins = 100;

           // int[] massCalXSliceValues = { 3, 6, 9, 12, 15 };

            int[] massCalXSliceValues = { 15 };



            foreach (var xsliceVal in massCalXSliceValues)
            {
                List<double> ppmErrorsBefore = new List<double>();
                List<double> ppmErrorsAfter = new List<double>();

                parameters.MassCalibrationNumXSlices = (short)xsliceVal;
                aligner.AlignerParameters = parameters;
                aligner.Execute(run);

                foreach (var result in repo.Results)
                {
                    MassTag mt = mtc.MassTagList.Where(p => p.ID == result.MassTagID).Where(p => p.ChargeState == result.ChargeState).First();
                    double theorMZ = mt.MZ;
                    double obsMZ = result.MonoMZ;
                    double scan = result.ScanLC;
                    double alignedMZ = run.GetAlignedMZ(obsMZ, scan);
                    double ppmErrorBefore = (theorMZ - obsMZ) / theorMZ * 1e6;
                    double ppmErrorAfter = (theorMZ - alignedMZ) / theorMZ * 1e6;


                    sb.Append(result.MassTagID + "\t" + result.ScanLC + "\t" + theorMZ.ToString("0.00000") + "\t" + obsMZ.ToString("0.00000") + "\t" + alignedMZ.ToString("0.00000") + "\t" + ppmErrorBefore.ToString("0.0") + "\t" + ppmErrorAfter.ToString("0.0"));
                    sb.Append(Environment.NewLine);
                    

                    ppmErrorsAfter.Add(ppmErrorAfter);
                    ppmErrorsBefore.Add(ppmErrorBefore);
                }

                Console.WriteLine(sb.ToString());
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Average ppm error before alignment = " + ppmErrorsBefore.Average().ToString("0.00"));
                Console.WriteLine("Average ppm error after alignment = " + ppmErrorsAfter.Average().ToString("0.00"));
                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine(xsliceVal + "\t" + ppmErrorsBefore.Average().ToString("0.00") + "\t" + ppmErrorsAfter.Average().ToString("0.00"));

              


            }




        }



        [Test]
        public void AlignmentParameterTesting3()
        {


            Run run = new RunFactory().CreateRun(@"D:\Data\Orbitrap\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW");

            string alignmentFeaturesFile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_READONLY_alignedFeatures.txt";
            UnlabelledTargetedResultFromTextImporter importer = new UnlabelledTargetedResultFromTextImporter(alignmentFeaturesFile);
            TargetedResultRepository repo = importer.Import();

            string massTagFile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\QCShew_Formic_MassTags_Bin10_all.txt";

            MassTagCollection mtc = new MassTagCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();


            NETAndMassAlignerParameters parameters = new NETAndMassAlignerParameters();
            NETAndMassAligner aligner = new NETAndMassAligner();

            aligner.SetFeaturesToBeAligned(repo.Results);
            aligner.SetReferenceMassTags(mtc.MassTagList);




            StringBuilder sb = new StringBuilder();
            sb.Append("mtid\tscanLC\ttheorMZ\tobsMZ\talignedMZ\tppmErrorBefore\tppmErrorAfter\n");


            parameters.MassCalibrationWindow = 20;
            parameters.MassCalibrationNumMassDeltaBins = 100;

            // int[] massCalXSliceValues = { 3, 6, 9, 12, 15 };

            int[] massCalXSliceValues = { 9 };



            foreach (var xsliceVal in massCalXSliceValues)
            {
                List<double> ppmErrorsBefore = new List<double>();
                List<double> ppmErrorsAfter = new List<double>();

                parameters.MassCalibrationNumXSlices = (short)xsliceVal;
                aligner.AlignerParameters = parameters;
                aligner.Execute(run);

                foreach (var result in repo.Results)
                {
                    MassTag mt = mtc.MassTagList.Where(p => p.ID == result.MassTagID).Where(p => p.ChargeState == result.ChargeState).First();
                    double theorMZ = mt.MZ;
                    double obsMZ = result.MonoMZ;
                    double scan = result.ScanLC;
                    double alignedMZ = run.GetAlignedMZ(obsMZ, scan);
                    double ppmErrorBefore = (theorMZ - obsMZ) / theorMZ * 1e6;
                    double ppmErrorAfter = (theorMZ - alignedMZ) / theorMZ * 1e6;


                    sb.Append(result.MassTagID + "\t" + result.ScanLC + "\t" + theorMZ.ToString("0.00000") + "\t" + obsMZ.ToString("0.00000") + "\t" + alignedMZ.ToString("0.00000") + "\t" + ppmErrorBefore.ToString("0.0") + "\t" + ppmErrorAfter.ToString("0.0"));
                    sb.Append(Environment.NewLine);


                    ppmErrorsAfter.Add(ppmErrorAfter);
                    ppmErrorsBefore.Add(ppmErrorBefore);
                }

                Console.WriteLine(sb.ToString());
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Average ppm error before alignment = " + ppmErrorsBefore.Average().ToString("0.00"));
                Console.WriteLine("Average ppm error after alignment = " + ppmErrorsAfter.Average().ToString("0.00"));
                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine(xsliceVal + "\t" + ppmErrorsBefore.Average().ToString("0.00") + "\t" + ppmErrorsAfter.Average().ToString("0.00"));




            }




        }

       

    }
}
