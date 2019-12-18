using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public class Inventory : Dictionary<string, RawItem>
    {
        private int ownerid = 0;

        public int OwnerID { get { return ownerid; } }
        public RawItem[] Items { get { return Values.ToArray(); } }
        public List<Tuple<string, byte, double>> GetReport { get { return Values.Select(p => Tuple.Create(p.Name, (byte)p.Unit, p.Stock)).ToList(); } }

        public Inventory(int _ownerid)
            :base()
        {
            ownerid = _ownerid;
        }

        public void Transference(Transference transference, out List<Exception> thrownexception)
        {
            thrownexception = new List<Exception>();
            switch (transference.Flag)
            {
                case InventoryTransferenceFlag.TransferenceGrowing:
                    {
                        foreach (var item in transference.Values)
                        {
                            if (ContainsKey(item.Name))
                            {
                                double newstock = this[item.Name] < item;
                                this[item.Name].EditCount(newstock);
                            }
                            else
                            {
                                Add(item.Name, item);
                            }
                        }
                        break;
                    }
                case InventoryTransferenceFlag.TransferenceShortage:
                    {
                        foreach (var item in transference.Values)
                        {
                            if (ContainsKey(item.Name))
                            {
                                double newstock = this[item.Name] > item;
                                this[item.Name].EditCount(newstock);
                            }
                            else
                            {
                                thrownexception.Add(new Exception(string.Format("StockItemNotExistedException: Couldn't find item {0}.", item.Name)));
#if ShortageItems
                                double newstock = item.Stock * -1;
                                item.EditCount(newstock);
                                Add(item.Name, item);
#endif
                            }
                        }
                        break;
                    }
                default:
                case InventoryTransferenceFlag.None:
                    break;
            }
        }

        public void CheckOrder(Cheque cheque, out List<Exception> thrownexception)
        {
            thrownexception = new List<Exception>();
            switch (cheque.Status)
            {
                case ChequeStatus.Confirmed:
                    {
                        switch (cheque.Operator)
                        {
                            case ChequeCashOperator.StaffMeal:
                                {
#if UserToken
                                    if (cheque.EmpolyeeID != cheque.ManagerID)
                                    {
                                        thrownexception = new Exception(string.Format("UserTokenDenied: StaffMeal CheckOrder From Employee ID {0}.", cheque.EmpolyeeID));
                                        break;
                                    }
#endif
                                    foreach (var product in cheque)
                                    {
                                        foreach (var item in product.GetRecipe())
                                        {
                                            if (ContainsKey(item.Name))
                                            {
                                                double newstock = this[item.Name] > item;
                                                this[item.Name].EditCount(newstock);
                                            }
                                            else
                                            {
                                                thrownexception.Add(new Exception(string.Format("StockItemNotExistedException: Couldn't find item {0}.", item.Name)));
#if ShortageItems
                                                double newstock = item.Stock * -1;
                                                item.EditCount(newstock);
                                                Add(item.Name, item);
#endif
                                                continue;
                                            }
                                        }
                                    }
                                    break;
                                }
                            case ChequeCashOperator.CashIn:
                                {
                                    foreach (var product in cheque)
                                    {
                                        foreach (var item in product.GetRecipe())
                                        {
                                            if (ContainsKey(item.Name))
                                            {
                                                double newstock = this[item.Name] > item;
                                                this[item.Name].EditCount(newstock);
                                            }
                                            else
                                            {
                                                thrownexception.Add(new Exception(string.Format("StockItemNotExistedException: Couldn't find item {0}.", item.Name)));
#if ShortageItems
                                                double newstock = item.Stock * -1;
                                                item.EditCount(newstock);
                                                Add(item.Name, item);
#endif
                                                continue;
                                            }
                                        }
                                    }
                                    break;
                                }
                            case ChequeCashOperator.Refund:
                                {
#if UserToken
                                    if (cheque.EmpolyeeID != cheque.ManagerID)
                                    {
                                        thrownexception.Add(new Exception(string.Format("UserTokenDenied: StaffMeal CheckOrder From Employee ID {0}.", cheque.EmpolyeeID)));
                                        break;
                                    }
#endif
                                    foreach (var product in cheque)
                                    {
                                        foreach (var item in product.GetRecipe())
                                        {
                                            if (ContainsKey(item.Name))
                                            {
                                                double newstock = this[item.Name] < item;
                                                this[item.Name].EditCount(newstock);
                                            }
                                            else
                                            {
                                                Add(item.Name, item);
                                            }
                                        }
                                    }
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public void ApplyCount(CountReport Report, out List<Exception> thrownexception)
        {
            thrownexception = new List<Exception>();
            foreach (RawItem item in Report.CountResult)
            {
                if (ContainsKey(item.Name))
                {
                    this[item.Name].SetData(item.Unit, item.Stock);
                }
                else
                {
                    thrownexception.Add(new Exception(string.Format("StockItemNotExistedException: Couldn't find item {0}.", item.Name)));
#if ShortageItems
                    Add(item.Name, new RawItem(item.Name, item.Unit, item.Stock, item.CountFlag));
                    this[item.Name].SetData(item.Unit, item.Stock);
#endif
                }
            }
        }
    }
}
