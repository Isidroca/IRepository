using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace EntityRepository.Test {
    [TestClass()]
    public class UnitTest1 {

        //string connectionString = @"Server=LOCALHOST,1433;Database=INTERSERV;User Id=sa; Password=YourNewStrong!Passw0rd2;";
        string connectionString = @"Server=ICALDERON\SQLEXPRESS;Database=teste;Trusted_Connection=True;";

        [TestMethod()]
        public async Task InsertTest() {

            EntityDataAccess repo = new EntityDataAccess(connectionString);
            User2 p = new User2();
            p.Id = 2;
            p.Email = "Isidro@gmail.com";
            p.RoleId = 1;
            p.Salt = "SSSSSSS";
            p.SaltedPassword = "XXXXXX";
            p.Username = "isidroca";
            p.WDate = DateTime.Now;
            p.Status = (short)1;
            
            var value = (int) await repo.InsertAsync(p);

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

        [TestMethod()]
        public void SelectMultipleParam() {

            EntityDataAccess repo = new EntityDataAccess(connectionString);

            repo.CommandText = "select * from person where id IN (@ids)";
            repo.AddParameter("@ids", new int[] { 1, 3 });

            var value = repo.ExecuteReader<Person>();

            Assert.AreEqual(1, value);
        }

    }
}
