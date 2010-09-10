﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.UnitTesting2
{
    public class FileRefs
    {
        public static string RawDataBasePath = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\";
        public static string TestFileBasePath = @"..\\..\\..\\TestFiles";
        public static string OutputFolderPath = @"..\\..\\..\\TestFiles\OutputtedData\";

        public class RawDataMSFiles
        {
            public static string RawDataBasePath = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\";
            public static string OrbitrapStdFile1 = FileRefs.RawDataBasePath + "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            public static string UIMFStdFile1 = FileRefs.RawDataBasePath + "35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";

            public static string TestFileBasePath = @"..\\..\\..\\TestFiles";
            public static string Bruker9TStandardFile1 = FileRefs.RawDataBasePath + "SWT_9t_TestDS216_Small";

            public static string YAFMSStandardFile1 = FileRefs.RawDataBasePath + "QC_Shew_09_01_pt5_a_20Mar09_Earth_09-01-01.yafms";
            public static string YAFMSStandardFile2 = FileRefs.RawDataBasePath + "metabolite_eqd.yafms";
        }

        public class PeakDataFiles
        {
            public static string OrbitrapOldDecon2LSPeakFile = FileRefs.RawDataBasePath + "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_peaks.dat";
            public static string OrbitrapPeakFile1 = FileRefs.RawDataBasePath + "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_peaks.txt";


        }





        public class ParameterFiles
        {
            public static string YAFMSParameterFileScans4000_4050 = FileRefs.RawDataBasePath + "LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_Thrash_scans4000-4050.xml";
        }



    }
}