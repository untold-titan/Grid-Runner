using GridRunner.Enums;
using GridRunner.Interfaces;

namespace GridRunner.Blocks
{
    public record class RotateBlock : Block
    {
        public RotateActionEnum Direction { get; set; }

        public RotateBlock(RotateActionEnum direction) : base()
        {
            Description = "Rotate " + direction.ToString();
            Direction = direction;
            FillColor = "#00ff00";
        }

        public override void RunBlock(string state)
        {
            Console.WriteLine("Moving " + Direction.ToString());
        }
    }
}
