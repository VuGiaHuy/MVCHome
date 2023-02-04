using System.ComponentModel.DataAnnotations;

namespace App.Models.Contact;
public class ContactModel
{
    [Key]
    public int Id {get;set;}
    [Required]
    [StringLength(50)]
    public string Name {get;set;}
    public string Email {get;set;}
    public DateTime DateSend {get;set;}
    public string Message {get;set;}
}