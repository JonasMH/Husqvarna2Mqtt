using System.ComponentModel.DataAnnotations;

namespace Husqvarna2Mqtt;

public class HusqvarnaAutomoverClientOptions
{
    [Required]
    public string ClientId { get; set; } = "";
    [Required]
    public string ClientSecret { get; set; } = "";
}
