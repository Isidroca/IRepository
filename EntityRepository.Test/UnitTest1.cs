using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EntityRepository.Test {
    [TestClass()]
    public class UnitTest1 {

        string connectionString = @"Server=LOCALHOST,1433;Database=INTERSERV;User Id=sa; Password=YourNewStrong!Passw0rd2;";

        [TestMethod()]
        public void InsertTest() {

            EntityDataAccess repo = new EntityDataAccess(connectionString);
            User2 p = new User2();

            p.Email = "Isidro@gmail.com";
            p.RoleId = 1;
            p.Salt = "SSSSSSS";
            p.SaltedPassword = "XXXXXX";
            p.Username = "isidroca";
            p.WDate = DateTime.Now;
            p.Status = (short)1;
          
            short value = (short)repo.Insert(p);

            Assert.AreEqual(1, value);
        }

        [TestMethod()]
        public void SelectFTest()
        {

            EntityDataAccess repo = new EntityDataAccess(connectionString);
     
            repo.CommandText = "select * from person";
            var value = repo.FirstOrDefault<Person>();

            Assert.AreEqual(1, value);
        }
    }
}
