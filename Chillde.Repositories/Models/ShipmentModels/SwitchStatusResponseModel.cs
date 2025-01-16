namespace Chillde.Repositories.Models.ShipmentModels;

public class SwitchStatusResponseModel
{
    public string? OrderCode { get; set; } 
    public bool Result { get; set; }       
    public string? Message { get; set; }  
}