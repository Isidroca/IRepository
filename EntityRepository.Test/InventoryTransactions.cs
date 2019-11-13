using System;
using System.Collections.Generic;
using System.Text;

namespace EntityRepository.Test
{
    public class InventoryTransactions
    {
        [PrimaryKey(true)]
        public int Id { get; set; } //(int, not null)
        public int CompanyId { get; set; } //(int, not null)
        public short InventoryTransactionTypeID { get; set; } //(short, not null)
        public string Observations { get; set; } //(varchar(500), null)
        public DateTime WDate { get; set; } //(datetime, not null)
        public byte StatusID { get; set; } //(tinyint, not null)
        public string UserID { get; set; } //(varchar(256), not null)S
    }
}
