using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace Core.Entities
{
    public class User : IdentityUser<Guid>,IUser
    {
        public int NationalId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool Notifications { get; set; } = true;
        public BankAccount? Account { get; set; }
    }
}
