using System;
using System.Collections.Generic;

namespace MVS_Stor.Areas.Admin.Models.Shop
{
    public class OrdersForAdminVM
    {
        public int OrderNumber { get; set; }
        public string UserName { get; set; }

        public decimal Total { get; set; }

        public Dictionary<string, int> ProductsAndQty { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}