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
}
