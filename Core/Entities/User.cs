using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Enums;

namespace Core.Entities
{
    public class User : IUser
    {
        public int NationalId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public UserContactInfo UserContactInfo { get; set; }
        public BankAccount? Account { get; set; }
    
        public override string ToString()
        {
            return $"NationalId: {NationalId}, " +
                   $"FirstName: {FirstName}, " +
                   $"LastName: {LastName}, " +
                   $"DateOfBirth: {DateOfBirth.ToString("yyyy-MM-dd")}, " +
                   $"UserContactInfo: {UserContactInfo}, " +
                   $"Account: {(Account != null ? Account.ToString() : "None")}";
        }
    }
}
