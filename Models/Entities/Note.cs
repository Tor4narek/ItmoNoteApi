using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Models.Entities;
[Table("notes")]
public class Note
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("title")] 
    public string Title { get; set; }
    [Column("description")]
    public string Description { get; set; }
    [Column("file")]
    public string File { get; set; }
    [Column("category")]
    public string Category { get; set; }
    [Column("date")]
    public DateTime Date { get; set; }
    [Column("username")]
    public string Username { get; set; }

    [Column("is_public")]
    public bool IsPublic
    {
        get; set;
        
    }
    
}