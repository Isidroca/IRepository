using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace EntityRepository.Test {
    [TestClass()]
    public class UnitTest1 {

        string connectionString = @"Server=ICALDERON\SQLEXPRESS;Database=teste;Trusted_Connection=True;";

        [TestMethod()]
        public void InsertTest() {

            EntityDataAccess repo = new EntityDataAccess(connectionString);
            Person p = new Person();

            p.Id = 1;
            p.Name = "Isidro";
            p.lastName = "Calderon Abreu";
            var value = repo.Update(p);

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
