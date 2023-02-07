using System;
using System.ComponentModel.DataAnnotations;

namespace App.Models.Contact;
public class ContactModel
{
    [Key]
    public int Id {get;set;}
    [Required]
    [StringLength(50)]
    public string Name {get;set;}
    [EmailAddress]
    public string Email {get;set;}
    [DataType(DataType.DateTime)]
    public DateTime DateSend {get;set;}
    [DataType(DataType.Text)]
    public string Message {get;set;}
}