namespace KeyDropGiveawayBot.Models;

public class Prize
{
    public int? Id { get; set; }
    public string Color { get; set; }
    public string ItemImg { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public double? Price { get; set; }
    public string Condition { get; set; }
    public string WeaponType { get; set; }
}