using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess
{
    [Flags]
    public enum DrawerReportMode : byte
    {
        None = 0,
        X_Print, Z_Print, D_Print
    }
    [Flags]
    public enum DrawerOperate : byte
    {
        None = 0,
        Opened, Closeed, Signed_In, Signed_Out, SetStartTerm
    }
    [Flags]
    public enum DrawerFlag : byte
    {
        None = 0,
        DineIn_Drawer, TakeOut_Drawer, Delivery_Drawer
    }
    [Flags]
    public enum RawCountIndicatorFlag : byte
    {
        None = 0,
        Shortage, Growing
    }
    [Flags]
    public enum RawCountFlag : byte
    {
        Daily = 0,
        Weekly, Monthly
    }
    [Flags]
    public enum ProductFlag : byte
    {
        None = 0,
        Main_Course, Side_Item, Service
    }
    [Flags]
    public enum DepartmentReportFlag : byte
    {
        None = 0,
        Screen, Printer
    }
    [Flags]
    public enum DepartmentFlag : byte
    {
        None = 0,
        Cash_Section, Pack_Section, MainCourse_Section, SideItem_Section, Delivery_Section
    }
    [Flags]
    public enum PrintableShiftFlags : byte
    {
        None = 0,
        CheckIn, CheckOut
    }
    [Flags]
    public enum ShiftFlags : byte
    {
        None = 0,
        Presence, Absence, Vacation
    }
    [Flags]
    public enum Units : byte
    {
        None = 0,
        Gram, Kilogram, Millilitre, Litre, Piece, Ounze, Packet, Cartoon, Flask
    }
    [Flags]
    public enum MoneyUnits : byte
    {
        None = 0,
        _25_Piasters, _50_Piasters, _1_Pound, _5_Pounds, _10_Pounds, _20_Pounds, _50_Pounds, _100_Pounds, _200_Pounds
    }
    [Flags]
    public enum UserFlags : byte
    {
        CrewMember, Cashier, CrewTrainer, SuperVisor, AssistantManager, GeneralManager,
        AreaManager, ChainManager, ITMember, Tester, Developer
    }
    [Flags]
    public enum OrderType : byte
    {
        None = 0,
        DineIn, TakeOut, Delivery
    }
    [Flags]
    public enum ChequeStatus : byte
    {
        None = 0,
        Ordering, Confirmed, Cashing, Delivering, Done
    }
    [Flags]
    public enum ChequeCashOperator : byte
    {
        StaffMeal, CashIn, Refund
    }
    [Flags]
    public enum InventoryTransferenceFlag : byte
    {
        None = 0, TransferenceShortage, TransferenceGrowing
    }
}
