using System;
using System.Collections.Generic;
using System.Text;

namespace EntityRepository.Test {
    
    public class Person {

        public string Name { get; set; }

        [PrimaryKey(true)]
        public int Id { get; set; }
        [Required]
        public string lastName { get; set; }
    }
}
