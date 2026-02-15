using GridRunner.Enums;
using GridRunner.Interfaces;

namespace GridRunner.Blocks
{
    public record class RotateBlock : Block
    {
        public RotateActionEnum Direction { get; set; }

        public RotateBlock(RotateActionEnum direction) : base()
        {
            Description = direction.ToString() == "Clockwise" ? "CW" : "CCW";
            Direction = direction;
            FillColor = "#d1d5db";
        }

        public override void RunBlock(string state)
        {
            Console.WriteLine("Moving " + Direction.ToString());
        }
    }
}
