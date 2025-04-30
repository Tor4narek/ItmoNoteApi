namespace Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("username")] 
    public string Username { get; set; }
    [Column("firstName")] 
    public string FirstName { get; set; }
    [Column("lastName")] 
    public string LastName { get; set; }
    [Column("hash")] 
    public string Hash { get; set; }
    
}