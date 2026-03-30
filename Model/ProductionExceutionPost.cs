using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTSteelWebAPI.Model
{
    public class ProductionExceutionPost
    {
        public class CCO_TRNS_PRDEXE_HD
        {
            public int DocEntry { get; set; }
            public int? DocNum { get; set; }
           
            public int? Series { get; set; }
        
            public string Object { get; set; }

            public string Status { get; set; } = "O";
        
            public DateTime? CreateDate { get; set; }
            public short? CreateTime { get; set; }
            public DateTime? UpdateDate { get; set; }
            public short? UpdateTime { get; set; }

            public string? U_DocEntry { get; set; }
            public string? U_Manual { get; set; }
            public string? U_ExecRmks { get; set; }
            public string? U_Source { get; set; }
            public string? U_WhsCode { get; set; }
            public string? U_FGWhs { get; set; }
            public string? U_SchId { get; set; }
            public string? U_SchNo { get; set; }
            public DateTime? U_SchDt { get; set; }
            public string? U_ProdType { get; set; }
            public string? U_UnitCode { get; set; }
            public DateTime? U_ProdDt { get; set; }
            public string? U_SchSts { get; set; }
            public string? U_ProcCode { get; set; }
            public string? U_OpID { get; set; }
            public string? U_Operator { get; set; }
            public short? U_StartHrs { get; set; }
            public short? U_EndHrs { get; set; }

            public string? U_PSchNo { get; set; }
            public string  ? U_PSchSts { get; set; }

            public string? U_TIPSUOM { get; set; }
            public string? U_TOPSUOM { get; set; }
            public string? U_TROUOM { get; set; }
            public string? U_TSCRUOM { get; set; }
            public string? U_TBalUom { get; set; }

            public decimal? U_TIPSQty { get; set; }
            public decimal? U_TRBQty { get; set; }
            public decimal? U_TOPSQty { get; set; }
            public decimal? U_TScrQty { get; set; }
            public decimal? U_YieldPer { get; set; }
            public decimal? U_TBalQty { get; set; }

            public string? U_SelectIp { get; set; }
            public DateTime? U_StartDate { get; set; }
            public DateTime? U_EndDate { get; set; }
            public short? U_StartTime { get; set; }
            public short? U_EndTime { get; set; }

            public DateTime? U_RlseDt { get; set; }
            public string? U_DocSts { get; set; }
            public string? U_PROERP { get; set; }
            public string? U_StsOnly { get; set; }
            public string? U_PrcMigo { get; set; }
            public string? U_BillNo { get; set; }

            public string? U_InterCom { get; set; }
            public string? U_WOEntry { get; set; }
            public string? U_WIPCMB { get; set; }
            public string? U_Merge { get; set; }
            public string? U_CoilNo { get; set; }

            public string? U_ShiftA { get; set; }
            public string? U_ShiftB { get; set; }
            public string? U_ShiftC { get; set; }
            public string? U_Bselect { get; set; }

            public string? U_GIQTY { get; set; }
            public string? U_GRQTY { get; set; }
            public string? U_TFlag { get; set; }
            public List<CCO_TRNS_PRDEXE_C1>? CCO_TRNS_PRDEXE_C1 { get; set; }
            public List<CCO_TRNS_PRDEXE_C2>? CCO_TRNS_PRDEXE_C2 { get; set; }
        }
        public class CCO_TRNS_PRDEXE_C1
        {
            public int DocEntry { get; set; }
            public int LineId { get; set; }

            public string Object { get; set; }
        

            public string? U_Select { get; set; }
            public string? U_WONo { get; set; }
            public string? U_WOId { get; set; }
            public string? U_ItemCode { get; set; }
            public string? U_IPLn { get; set; }
            public string? U_IPItem { get; set; }
            public string? U_IPBatch { get; set; }
            public string? U_BarCdSts { get; set; }
            public string? U_IPLevel { get; set; }

            public short? U_SchPcs { get; set; }
            public short? U_Pkts { get; set; }
            public short? U_PcPerPkt { get; set; }

            public decimal? U_SchQty { get; set; }
            public string U_UOM { get; set; }

            public decimal? U_TrimWid { get; set; }
            public decimal? U_CLen { get; set; }
            public string? U_CLenUOM { get; set; }

            public string? U_WhseCode { get; set; }
            public string? U_MillCode { get; set; }
            public string? U_MillName { get; set; }
            public string? U_JBCode { get; set; }
            public string? U_JBName { get; set; }

            public string? U_MaxYield { get; set; }
            public string? U_VIPBatch { get; set; }

            public string? U_Surface { get; set; }
            public string? U_Coating { get; set; }
            public string? U_Oiling { get; set; }
            public string? U_Edge { get; set; }

            public decimal? U_TPrmQty { get; set; }
            public decimal? U_TPrmVal { get; set; }
            public decimal? U_TScrVal { get; set; }
            public decimal? U_UnitPrc { get; set; }

            public string? U_BarScn { get; set; }
            public string? U_EqSpec { get; set; }
            public string? U_Status { get; set; }

            public decimal? U_ActlQty { get; set; }
            public decimal? U_BalQty { get; set; }
            public decimal? U_RolBkQty { get; set; }

            public string? U_RolBak { get; set; }  // default 'N'

            public DateTime? U_RcptDate { get; set; }
            public string? U_Remarks { get; set; }

            public string? U_Challan { get; set; }
            public string? U_CoilNo { get; set; }

            public short? U_StartTime { get; set; }
            public short? U_EndTime { get; set; }

            public decimal? U_Thick { get; set; }
            public decimal? U_Width { get; set; }
            public decimal? U_Length { get; set; }

            public string? U_Form { get; set; }
            public string? U_Type { get; set; }
            public string? U_Grade { get; set; }
        }
        public class CCO_TRNS_PRDEXE_C2
        {
            public int DocEntry { get; set; }
            public int LineId { get; set; }
            public int? VisOrder { get; set; }
            public string Object { get; set; }

            public string? U_Select { get; set; }
            public string? U_WONo { get; set; }
            public string? U_WOId { get; set; }
            public string? U_Roll { get; set; }

            public string? U_IPLn { get; set; }
            public string? U_IPBatch { get; set; }
            public string? U_IPItem { get; set; }

            public string? U_MTS { get; set; }
            public string? U_WIP { get; set; }

            public string? U_VIPBatch { get; set; }
            public string? U_VOPBatch { get; set; }

            public string? U_OPLn { get; set; }
            public string? U_OPType { get; set; }
            public string? U_OPItem { get; set; }

            public string? U_CustCode { get; set; }
            public string? U_CustName { get; set; }

            public string? U_SODN { get; set; }
            public short? U_SODE { get; set; }
            public string? U_SOLn { get; set; }

            public string? U_QDCNo { get; set; }
            public string? U_QDCDE { get; set; }
            public string? U_QDCObj { get; set; }

            public string? U_OPLevel { get; set; }
            public string? U_Plan { get; set; }

            public string? U_Type { get; set; }
            public string? U_Form { get; set; }
            public string? U_Grade { get; set; }

            public string? U_FGWhs { get; set; }

            public decimal? U_SOSchQty { get; set; }

            public decimal? U_Thick { get; set; }
            public decimal? U_Width { get; set; }
            public decimal? U_Length1 { get; set; }
            public decimal? U_Length2 { get; set; }
            public decimal? U_Pitch { get; set; }
            public decimal? U_UnitWt { get; set; }

            public decimal? U_SchQty { get; set; }

            public short? U_Pcs { get; set; }
            public short? U_SchPcs { get; set; }
            public short? U_Pkts { get; set; }
            public short? U_PcPerPkt { get; set; }

            public decimal? U_Qty { get; set; }
            public string? U_UOM { get; set; }

            public string? U_PassNum { get; set; }
            public decimal? U_PassLen { get; set; }
            public string? U_PassUOM { get; set; }

            public decimal? U_LastUnit { get; set; }
            public decimal? U_NextUnit { get; set; }

            public string? U_SplInstn { get; set; }
            public string? U_NextProc { get; set; }

            public string? U_PackID { get; set; }
            public string? U_GrpNo { get; set; }

            public decimal? U_MinPkWt { get; set; }
            public decimal? U_MaxPkWt { get; set; }

            public string? U_Surface { get; set; }
            public string? U_Coating { get; set; }
            public string? U_Oiling { get; set; }
            public string? U_Edge { get; set; }

            public string? U_CPartNo { get; set; }
            public string? U_EqSpec { get; set; }

            public decimal? U_PlanOP { get; set; }
            public decimal? U_Produced { get; set; }
            public decimal? U_OpenQty { get; set; }

            public string? U_Status { get; set; }

            public string? U_CustSpec { get; set; }
            public string? U_Sequence { get; set; }

            public short? U_ActPkt { get; set; }

            public string? U_VPDE { get; set; }
            public string? U_VPPrcLn { get; set; }
            public string? U_VPRmks { get; set; }

            public string? U_MachCode { get; set; }
            public string? U_VPUser { get; set; }

            public string? U_Edgebur { get; set; }
            public string? U_OilStain { get; set; }
            public string? U_Coilset { get; set; }
            public string? U_Telescop { get; set; }
            public string? U_Scalmark { get; set; }
            public string? U_surfscrh { get; set; }
            public string? U_RustOxd { get; set; }
            public string? U_crosbow { get; set; }
            public string? U_Pinhole { get; set; }
            public string? U_dentgoug { get; set; }

            public string? U_Remark { get; set; }
        }
        public class CCO_TRNS_PRDEXE_C3

        {

            public int DocEntry { get; set; }

            public int LineId { get; set; }

            public string Object { get; set; }

            public string? U_Select { get; set; }

            public string? U_AddRow { get; set; }

            public string? U_ItemCode { get; set; }

            public string? U_AssignLn { get; set; }

            public short? U_IPPcs { get; set; }

            public decimal? U_IPQty { get; set; }

            public string? U_IPItem { get; set; }

            public string? U_OPItem { get; set; }

            public string? U_IPLn { get; set; }

            public string? U_INLn { get; set; }

            public string? U_OutLn { get; set; }

            public string? U_IPBatch { get; set; }

            public string? U_IPLevel { get; set; }

            public short? U_IPSchPcs { get; set; }

            public string? U_IPUOM { get; set; }

            public string? U_CustCode { get; set; }

            public string? U_CustName { get; set; }

            public string? U_MTS { get; set; }

            public string? U_OPLn { get; set; }

            public string? U_OPType { get; set; }

            public short? U_RBackNo { get; set; }

            public string? U_OPBatch { get; set; }

            public string? U_OPForm { get; set; }

            public string? U_OPGrade { get; set; }

            public decimal? U_OPThick { get; set; }

            public decimal? U_OPWidth { get; set; }

            public string? U_Location { get; set; }

            public decimal? U_Length1 { get; set; }

            public decimal? U_Length2 { get; set; }

            public decimal? U_Pitch { get; set; }

            public short? U_SOPcs { get; set; }

            public short? U_OPSchPcs { get; set; }

            public short? U_Pkts { get; set; }

            public short? U_PcPerPkt { get; set; }

            public decimal? U_UnitWt { get; set; }

            public decimal? U_GrossWt { get; set; }

            public decimal? U_PckgWt { get; set; }

            public decimal? U_NetWt { get; set; }

            public decimal? U_TheoWt { get; set; }

            public string? U_UOM { get; set; }

            public string? U_Whse { get; set; }

            public string? U_FGWhs { get; set; }

            public string? U_SODN { get; set; }

            public short? U_SODE { get; set; }

            public string U_SOLn { get; set; }

            public string? U_PackID { get; set; }

            public string? U_GrpNo { get; set; }

            public string? U_QDCNo { get; set; }

            public string? U_QDCDE { get; set; }

            public string? U_QDCObj { get; set; }

            public string? U_WIP { get; set; }

            public string? U_GIDN { get; set; }

            public string? U_GIDE { get; set; }

            public string? U_GRDN { get; set; }

            public string? U_GRDE { get; set; }

            public string? U_QCSts { get; set; }

            public string? U_ExeSts { get; set; }

            public decimal? U_MinPkWt { get; set; }

            public decimal? U_MaxPkWt { get; set; }

            public string? U_Surface { get; set; }

            public string? U_Coating { get; set; }

            public string? U_Oiling { get; set; }

            public string? U_Edge { get; set; }

            public string? U_CPartNo { get; set; }

            public decimal? U_PurPrc { get; set; }

            public decimal? U_PrmVal { get; set; }

            public decimal? U_ScrPrc { get; set; }

            public decimal? U_ScrVal { get; set; }

            public decimal? U_UnitPrc { get; set; }

            public string? U_selctlbl { get; set; }

            public string? U_TrnsId { get; set; }

            public string? U_EqSpec { get; set; }

            public string? U_Status { get; set; }

            public string? U_SubGR { get; set; }

            public string? U_ConSGI { get; set; }

            public string? U_CustSpec { get; set; }

            public string? U_PartNo { get; set; }

            public string? U_Seq { get; set; }

            public decimal? U_OBThick1 { get; set; }

            public decimal? U_OBThick2 { get; set; }

            public decimal? U_OBWidth1 { get; set; }

            public decimal? U_OBWidth2 { get; set; }

            public decimal? U_OBLen1 { get; set; }

            public decimal? U_OBLen2 { get; set; }

            public decimal? U_OBDig1 { get; set; }

            public decimal? U_OBDig2 { get; set; }

            public string? U_OPLevel { get; set; }

            public string? U_Type { get; set; }

            public string? U_GRRevNo { get; set; }

            public string? U_GIRevNo { get; set; }

            public string? U_DwnGrd { get; set; }

            public string? U_DwnItem { get; set; }

            public string? U_TGTSCHNO { get; set; }

            public string? U_TGTSCHDE { get; set; }

            public string? U_TGTWO { get; set; }

            public string? U_VPDE { get; set; }

            public string? U_VPPrcLn { get; set; }

            public string? U_MItem { get; set; }

            public string? U_MBatch { get; set; }

            public string? U_MCHallan { get; set; }

            public string? U_BC { get; set; }

            public string? U_MinWidth { get; set; }

            public string? U_MaxWidth { get; set; }

            public string? U_MinLen { get; set; }

            public string? U_MaxLen { get; set; }

            public string? U_MinThick { get; set; }

            public string? U_MaxThick { get; set; }

            public string? U_QRmks1 { get; set; }

            public string? U_QRmks2 { get; set; }

            public string? U_Attach { get; set; }

            public short? U_OD { get; set; }

            public string? U_Bundling { get; set; }  // default 'N'

            public short? U_ID { get; set; }

            public DateTime? U_StartDate { get; set; }

            public DateTime? U_EndDate { get; set; }

            public short? U_StartTime { get; set; }

            public short? U_EndTime { get; set; }

            public string? U_QADE { get; set; }

            public string? U_Shift { get; set; }

            public string? U_HeatNo { get; set; }

            public decimal? U_OrgNetWt { get; set; }

            public decimal? U_OrgGrWt { get; set; }

            public decimal? U_OrgPkgWt { get; set; }

            public string? U_MachCode { get; set; }

            public short? U_Split { get; set; }

            public string? U_TrgtRlDE { get; set; }

            public string? U_Select1 { get; set; }

            public string? U_OPType1 { get; set; }

            public string? U_OPBatch1 { get; set; }

            public string? U_GINo1 { get; set; }

            public string? U_GRNo1 { get; set; }

            public decimal? U_NetWt1 { get; set; }

            public string? U_ScpMach { get; set; }

            public string? U_OPPartNoRef { get; set; }

            public string? U_FGRef { get; set; }

            public string? U_PCount { get; set; } // default '1'

            public string? U_Operator { get; set; }

            public decimal? U_PWt { get; set; }

            public string? U_VPPCust { get; set; }

            public string? U_VPUser { get; set; }

            public string? U_Edgebur { get; set; }

            public string? U_OilStain { get; set; }

            public string? U_Coilset { get; set; }

            public string? U_Telescop { get; set; }

            public string? U_Scalmark { get; set; }

            public string? U_surfscrh { get; set; }

            public string? U_RustOxd { get; set; }

            public string? U_crosbow { get; set; }

            public string? U_Pinhole { get; set; }

            public string? U_dentgoug { get; set; }

            public string? U_Remark { get; set; }

        }

        public class ProductionExecutionC3Post

        {

            public int DocEntry { get; set; }

            public DateTime PostDate { get; set; }

            public string MachineCode { get; set; }

            public List<CCO_TRNS_PRDEXE_C3> C3List { get; set; }

        }

        public class SapReceiptOIGN
        {
            public DateTime DocDate { get; set; }
            public string? Comments { get; set; }

            public string? U_DocType { get; set; }   // U_Source
            public string? U_SchNo { get; set; }
            public string? U_SrcObj { get; set; }

            public List<SapReceiptLine> DocumentLines { get; set; } = new();
        }

        public class SapReceiptLine
        {
            public string ItemCode { get; set; }
            public string WarehouseCode { get; set; }
            public decimal Quantity { get; set; }

            public List<SapBatch> BatchNumbers { get; set; } = new();
        }

        public class SapBatch
        {
            public string BatchNumber { get; set; }
            public decimal Quantity { get; set; }
        }
        public class WorkOrderLabelDto
        {
            public decimal U_SchQty { get; set; }
            public string? U_CoilNo { get; set; }
            public string? U_JBName { get; set; }
            public string? U_SchNo { get; set; }

            public string? U_Edgebur { get; set; }
            public string? U_OilStain { get; set; }
            public string? U_Coilset { get; set; }
            public string? U_Telescop { get; set; }
            public string? U_Scalmark { get; set; }
            public string? U_surfscrh { get; set; }
            public string? U_RustOxd { get; set; }
            public string? U_crosbow { get; set; }
            public string? U_Pinhole { get; set; }
            public string? U_dentgoug { get; set; }
            public string? U_OPBatch { get; set; }

            public decimal? U_OPThick { get; set; }
            public decimal? U_NetWt { get; set; }
            public decimal? U_OPWidth { get; set; }
            public string? U_OPGrade { get; set; }
            public int? U_SOPcs { get; set; }
            public int? U_Pkts { get; set; }
        }

    }
}
