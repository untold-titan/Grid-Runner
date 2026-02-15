namespace GridRunner.Interfaces
{
    public abstract record class Block
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string FillColor { get; set; }
        public bool Locked { get; set; }
        public bool isFixed { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public int? VehicleIndex { get; set; } // Associates block with a specific vehicle
        public abstract void RunBlock(string state); // State is placeholder until an actual state is created
    }
}
