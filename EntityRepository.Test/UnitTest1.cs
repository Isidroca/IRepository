using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EntityRepository.Test {
    [TestClass()]
    public class UnitTest1 {

        string ProconnectionString = @"";
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

            var value = (int)await repo.InsertAsync(p);

            Assert.AreEqual(1, value);
        }

        [TestMethod()]
        public void SelectFTest() {

            EntityDataAccess repo = new EntityDataAccess(connectionString);

            repo.CommandText = "select * from person";
            var value = repo.FirstOrDefault<Person>();

            Assert.AreEqual(1, value);
        }

        [TestMethod()]
        public void SelectMultipleParam() {

            EntityDataAccess repo = new EntityDataAccess(connectionString);

            repo.CommandText = "select * from person where id IN (@ids)";

            var list = new List<int>();
            list.Add(1);
            list.Add(3);

             repo.AddParameter("@ids", list);

            var value = repo.ExecuteReader<Person>();

            Assert.AreEqual(1, value);
        }

        [TestMethod()]
        public async Task ChecProAsync() {

            var repo = new EntityDataAccess(ProconnectionString);
            var productsViewParams = new ProductsViewParams();

            productsViewParams.CompanyCode = 35;
            //productsViewParams.Status = 1;
            productsViewParams.Top = 100;
            //productsViewParams.KeySearch = "Pixel 2 XL";

            repo.CommandText = "dbo.GetProducts";
            repo.AddParameter("@CompanyCode", productsViewParams.CompanyCode);
            repo.AddParameter("@BranchCode", productsViewParams.BranchCode);
            repo.AddParameter("@Top", productsViewParams.Top);
            repo.AddParameter("@KeySearch", productsViewParams.KeySearch);
            repo.AddParameter("@CategoryID", productsViewParams.CategoryID);
            repo.AddParameter("@PType", productsViewParams.PType);
            repo.AddParameter("@Status", productsViewParams.Status);
            repo.CommandType = EntityDataAccess.ICommandType.StorePrecedure;
            var ddd = await repo.ExecuteReaderAsync<ProductsListView>();

            Assert.AreEqual(1, ddd);
        }

    }
}
