namespace ArlaNatureConnect.Domain.Entities;

public class CreateNatureCheck
{
    public Guid NatureCheckId { get; set; }
    public Guid FarmId { get; set; }
    public Guid PersonId { get; set; }
    public string FarmName { get; set; }
    public int FarmCVR { get; set; }
    public string FarmAddress { get; set; }
    public string ConsultantFirstName { get; set; }
    public string ConsultantLastName { get; set; }
    public DateTime DateTime { get; set; }

}