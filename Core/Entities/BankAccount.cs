using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities;
    [Table("BankAccounts")]
    public class BankAccount : IBankAccount
    {
        public int NationalId { get; set; }
        public Guid UserId { get; set; }
        public string AccountNumber { get; set; }
        public DateTime CreationDate { get; set; }
        public decimal Balance { get; set; }
        public List<Operation>? Operations { get; set; }
        public User User { get; set; }
        public List<Card> BankCards { get; set; }
    }
