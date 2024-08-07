using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities;
    public class BankAccount : IBankAccount
    {
       
        public int NationalId { get; set; }
        public string AccountNumber { get; set; }
        public DateTime CreationDate { get; set; }
        public decimal Balance { get; set; }
        private List<Operation> Transactions { get; set; }
    }
