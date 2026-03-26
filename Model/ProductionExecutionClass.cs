namespace TTSteelWebAPI.Model
{
    public class ProductionExecutionClass
    {
        public class ScheduleDto
        {
            public string U_SchNo { get; set; }
            public string U_CustName { get; set; }
            public string U_Grade { get; set; }
            public decimal? U_TIPSQty { get; set; }
            public string U_CoilNo { get; set; }
        }
        public class JobBatchDto
        {
            public string U_IPBatch { get; set; }
            public int? U_IPLn { get; set; }

            public string U_ItemCode { get; set; }
            public int? U_Level { get; set; }

            public decimal? U_SchPcs { get; set; }
            public decimal? U_SchQty { get; set; }

            public decimal? U_Pkts { get; set; }
            public decimal? U_PcPerPkt { get; set; }

            public string U_UOM { get; set; }
            public DateTime? U_RcptDate { get; set; }

            public decimal? U_TrimWid { get; set; }
            public decimal? U_CLen { get; set; }
            public string U_CLenUOM { get; set; }

            public string U_WhseCode { get; set; }
            public string U_MillCode { get; set; }
            public string U_MillName { get; set; }

            public string U_JBCode { get; set; }
            public string U_JBName { get; set; }

            public decimal? U_MaxYield { get; set; }

            public string U_Surface { get; set; }
            public string U_Coating { get; set; }
            public string U_Edge { get; set; }
            public string U_Oiling { get; set; }

            public string U_SchNo { get; set; }

            public string U_CoilNo { get; set; }
            public string U_Grade { get; set; }

            public decimal? U_Thick { get; set; }
            public decimal? U_Width { get; set; }

            public string U_Form { get; set; }
            public string U_Type { get; set; }

            public int? DocNum { get; set; }

            public string BarcdSts { get; set; }
            public string Sts { get; set; }
        }
        public class CountDto
        {
            public int TotalCount { get; set; }
        }
        public class ScheduleSizeDto
        {
            public int? U_IPLn { get; set; }
            public string U_Roll { get; set; }
            public string U_IPItem { get; set; }
            public string U_IPBatch { get; set; }

            public string U_MTS { get; set; }      // Yes/No
            public string U_WIP { get; set; }

            public string U_OPType { get; set; }
            public string U_OPItem { get; set; }

            public string EqSpec { get; set; }

            public string U_Type { get; set; }
            public string U_Grade { get; set; }
            public string U_Form { get; set; }

            public decimal? U_Thick { get; set; }
            public decimal? U_Width { get; set; }

            public decimal? U_Length1 { get; set; }
            public decimal? U_Length2 { get; set; }

            public decimal? U_Pitch { get; set; }

            public decimal? U_Pcs { get; set; }
            public decimal? U_SchPcs { get; set; }

            public decimal? U_Pkts { get; set; }
            public decimal? U_PcPerPkt { get; set; }

            public decimal? U_UnitWt { get; set; }

            public decimal? U_Qty { get; set; }
            public decimal? U_SchQty { get; set; }

            public string U_UOM { get; set; }

            public string U_PassNum { get; set; }   // contains '?'
            public decimal? U_PassLen { get; set; }

            public string U_PassUOM { get; set; }

            public string U_SplInstn { get; set; }

            public decimal? U_MinPkWt { get; set; }
            public decimal? U_MaxPkWt { get; set; }

            public string U_Surface { get; set; }
            public string U_Coating { get; set; }
            public string U_Oiling { get; set; }
            public string U_Edge { get; set; }

            public int? AssgnLn { get; set; }
            public int? AssgnOPLn { get; set; }
            public string AssgnSOLn { get; set; }  // ✅

            public int U_SODE { get; set; }   // "1,726"
            public string U_SODN { get; set; }

            public string U_QDCDE { get; set; }
            public string U_QDCNo { get; set; }
            public string U_QDCObj { get; set; }

            public string U_CustCode { get; set; }
            public string U_CustName { get; set; }

            public string U_WhseCode { get; set; }

            public string U_VPDE { get; set; }
            public string U_VPPrcLn { get; set; }

            public string U_SeqCode { get; set; }

            public string U_Rmks { get; set; }
        }
        public class DocEntryDto
        {
            public int DocEntry { get; set; }
        }
    }
}
