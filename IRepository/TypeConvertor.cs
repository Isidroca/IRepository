using System.Data;

namespace EntityRepository {

    public enum DbType : short {

        None = -1,
        BigInt = SqlDbType.BigInt,
        Binary = SqlDbType.Binary,
        Bit = SqlDbType.Bit,
        Char = SqlDbType.Char,
        DateTime = SqlDbType.DateTime,
        Decimal = SqlDbType.Decimal,
        Float = SqlDbType.Float,
        Image = SqlDbType.Image,
        Int = SqlDbType.Int,
        Money = SqlDbType.Decimal,
        NChar = SqlDbType.NChar,
        NText = SqlDbType.NText,
        NVarchar = SqlDbType.NVarChar,
        Real = SqlDbType.Real,
        SmallDateTime = SqlDbType.SmallDateTime,
        SmallInt = SqlDbType.SmallInt,
        SmallMoney = SqlDbType.SmallMoney,
        Text = SqlDbType.Text,
        TinyInt = SqlDbType.TinyInt,
        UniqueIdentifier = SqlDbType.UniqueIdentifier,
        VarBinary = SqlDbType.VarBinary,
        Variant = SqlDbType.Variant
    }
}
