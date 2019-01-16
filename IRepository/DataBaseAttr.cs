using System;

namespace EntityRepository {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableNameAttribute : Attribute {
        public string Name { get; set; }

        /// <summary>
        /// Table name
        /// </summary>
        /// <param name="Name"></param>
        public TableNameAttribute(string Name) {
            this.Name = Name;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PrimaryKeyAttribute : Attribute {
        public bool PrimaryKey { get; set; }
        public bool AutoIncrease { get; set; }
        public PrimaryKeyAttribute(bool AutoIncrease = false) {
            this.PrimaryKey = true;
            this.AutoIncrease = AutoIncrease;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ForeignKeyAttribute : Attribute {
        public string Name { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name">ForeignKey Column</param>
        public ForeignKeyAttribute(string Name) {
            this.Name = Name;
        }
    }

    public class ColumnNameAttribute : Attribute {
        public string Name { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name">Column name</param>
        public ColumnNameAttribute(string Name) {
            this.Name = Name;
        }
    }

    public class RequiredAttribute : Attribute {
        public bool IsRequired { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name">Column Is Required</param>
        public RequiredAttribute() {
            IsRequired = true;
        }
    }

    public class NotMappedAttribute : Attribute {

        public bool NoInsert { get; private set; }
        public bool NoUpdate { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name">Column no mapper for Crud</param>
        public NotMappedAttribute() { }
    }

    public class NoInsertAttribute : Attribute {
        public bool NoInsert { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name">Column no mapper for insert and update</param>
        public NoInsertAttribute() {
            NoInsert = true;
        }
    }

    public class NoUpdateAttribute : Attribute {
        public bool NoUpdate { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name">Column no mapper for update</param>
        public NoUpdateAttribute() {
            NoUpdate = true;
        }
    }

    public class ComputedAttribute : Attribute {
        public bool Computed { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name">Computed, no crud</param>
        public ComputedAttribute() {
            Computed = true;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class UpperCaseAttribute : Attribute {
        public bool UpperCase { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name">UpperCase to string values</param>
        public UpperCaseAttribute() {
            UpperCase = true;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class LowerCaseAttribute : Attribute {
        public bool LowerCase { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name">LowerCase to string values</param>
        public LowerCaseAttribute() {
            LowerCase = true;
        }
    }
}
