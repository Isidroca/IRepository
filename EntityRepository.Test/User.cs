using System;
using System.Collections.Generic;
using System.Text;

namespace EntityRepository.Test {
   public class User2 {
        [PrimaryKey(true)]
        public long Id { get; set; } //(short, not null)
        public byte RoleId { get; set; } //(tinyint, not null)
        public string Username { get; set; } //(varchar(50), not null)
        public string Email { get; set; } //(varchar(100), null)
        public string SaltedPassword { get; set; } //(nvarchar(450), not null)
        public string Salt { get; set; } //(nvarchar(450), not null)
        public short Status { get; set; } //(short, not null)
        public DateTime WDate { get; set; } //(datetime, not null)
    }

    public class ProductsViewParams {
        public int? CompanyCode { get; set; }
        public int? BranchCode { get; set; }
        public int? Top { get; set; }
        public string KeySearch { get; set; }
        public int? CategoryID { get; set; }
        public byte? PType { get; set; }
        public short? Status { get; set; }
    }

    public class ProductsListView {

        public int ProductID { get; set; } //(int, not null)
        public DateTime DateCreated { get; set; } //(datetime, not null)
        public DateTime DateUpdated { get; set; } //(datetime, not null)
        public int? CategoryID { get; set; } //(int, null)
        public string Name { get; set; } //(varchar(150), not null)
        public decimal? StandardCost { get; set; } //(decimal(18,2), not null)
        public decimal Price { get; set; } //(decimal(18,2), not null)
        public int? ProviderID { get; set; } //(int, null)
        public bool IsSellable { get; set; } //(bit, not null)
        public DateTime SellStartDate { get; set; } //(datetime, not null)
        public DateTime? SellEndDate { get; set; } //(datetime, null)
        public decimal? Tax { get; set; } //(decimal(4,2), null)
        public DateTime? DiscontinuedDate { get; set; } //(datetime, null)
        public byte Status { get; set; } //(tinyint, not null)
        public int Quantity { get; set; } //(int, not null)
        public short? MaxCnt { get; set; } //(short, null)
        public short? MinCnt { get; set; } //(short, null)
        public string StatusDescription { get; set; }
        public string ProductNumber { get; set; } //(nvarchar(25), null)

        public string customProduct {
            get {
                var _customPro = $@"{Name} | ${string.Format("{0:#,##0.##}", Price)}";
                return _customPro;
            }
        }
    }
}
