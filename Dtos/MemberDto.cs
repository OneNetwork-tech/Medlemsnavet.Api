namespace Medlemsnavet.Dtos;

public class MemberDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PersonalIdentityNumber { get; set; }
    public string PhoneNumber { get; set; }
    public string PostalAddress { get; set; }
    public DateTime EntryDate { get; set; }
}