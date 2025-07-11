namespace Medlemsnavet.Dtos;

public class EventDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Location { get; set; }
    public string CreatorName { get; set; }
    public int AttendeeCount { get; set; }
}