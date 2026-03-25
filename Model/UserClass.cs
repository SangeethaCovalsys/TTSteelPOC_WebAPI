

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTSteelWebAPI.Model
{
    public class UserClass
    {
        [Table("OUSR")]
        public class OUSR
        {
            [Column("USER_CODE")]
            public string? UserCode { get; set; }
            [Column("U_NAME")]
            public string? UserName { get; set; }
            [Key]
            [Column("USERID")]
            public short UserId { get; set; }
            //public short? Department { get; set; }
            //public short? Branch { get; set; }
        }
    }
}
