using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class UserPayment
    {
        public int Id { get; set; }
        public double AcctNumber { get; set; }
        public bool Active { get; set; }
        public int CustomerId { get; set; }
        public int PaymentTypeId { get; set; }
    }
}
