﻿namespace Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
[Table("categories")]
public class Category
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("name")] 
    public string Name { get; set; }
    
}