namespace RoktoLink.Models.ViewModels
{
    /// <summary>
    /// Represents an official, verified institutional blood center or transfusion bank in Dhaka.
    /// </summary>
    public class BloodCenterViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string EmergencyHotline { get; set; } = string.Empty;
        public string Landline { get; set; } = string.Empty;
        public string OperatingHours { get; set; } = "24/7 Emergency Service";
        public string AvailableServices { get; set; } = "Whole Blood, Platelets, Screening";
        public string GoogleMapsQuery => Uri.EscapeDataString($"{Name}, {Address}, Dhaka");
    }
}
