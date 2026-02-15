namespace GridRunner.Models
{
    public record class Vehicle
    {
        public char Type { get; set; }
        public char Color { get; set; }
        public string CssClass { get; set; } = "";
        public List<string> Cells { get; set; } = new();
    }
}
