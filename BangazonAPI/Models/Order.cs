﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int UserPaymentTypeId { get; set; }
        //public List<Order> Orders { get; set; }
        public List<Product> Products { get; set; }
    }
}
