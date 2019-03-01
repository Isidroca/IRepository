using Microsoft.VisualStudio.TestTools.UnitTesting;
using EntityRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
namespace EntityRepository.Test {
    [TestClass()]
    public class UnitTest1 {

        string connectionString = @"Server=tcp:proventa.database.windows.net,1433;Initial Catalog=SFPV;Persist Security Info=False;User ID=prouser;Password=I/*5853090;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        [TestMethod()]
        public void InsertTest() {

            EntityDataAccess repo = new EntityDataAccess(connectionString);
            Person p = new Person();

            p.DateCreated = DateTime.Now;
            p.DateUpdated = DateTime.Now;
            p.FirstName = "Calos";
            p.LastEditedBy = "xcxc";
            p.Status = 1;
            p.CompanyCode = 35;
            p.BranchCode = 26;
            var value = repo.Insert(p, false);

       
        }
    }

    [TableName("Person.Person")]
    public class Person {

        [PrimaryKey(true)]
        public int PersonID { get; set; } //(int, not null)
        [NoUpdate]
        public DateTime DateCreated { get; set; } //(datetime, not null)
        public DateTime DateUpdated { get; set; } //(datetime, not null)
        public int CompanyCode { get; set; } //(int, not null)
        public int BranchCode { get; set; } //(int, not null)
        public byte PersonType { get; set; } //(tinyint, not null)
        public string FirstName { get; set; } //(varchar(50), not null)
        public string LastName { get; set; } //(varchar(50), null)

        [Computed]
        public string SearchName { get; set; } //(varchar(150), null)
        public int? CategoryID { get; set; } //(int, null)
        public DateTime? Birthday { get; set; } //(date, null)
        public string Gender { get; set; } //(char(1), null)
        public string IDNumber { get; set; } //(varchar(20), null)
        public string RNC { get; set; } //(varchar(30), null)
        public string PhoneNumber { get; set; } //(varchar(20), null)
        public string PhoneNumber2 { get; set; } //(varchar(20), null)
        public string CellPhone { get; set; } //(varchar(20), null)      
        public string UserPreferences { get; set; } //(nvarchar(max), null)
        public string CustomFields { get; set; } //(nvarchar(max), null)
        public string Email { get; set; } //(bit, not null)
        public string LastEditedBy { get; set; } //(varchar(450), null)
        public string WebsiteURL { get; set; } //(nvarchar(1000), null)
        public byte Status { get; set; } //(tinyint, not null)
        public string Comments { get; set; } //(varchar(550), null
    }
}
