using GridRunner.Enums;
using GridRunner.Interfaces;

namespace GridRunner.Blocks
{
    public record class MoveBlock : Block
    {
        public MoveActionEnum Direction { get; set; }

        public MoveBlock(MoveActionEnum direction) : base()
        {
            Description = "Move " + direction.ToString();
            Direction = direction;
            FillColor = "#ff0000";
        }

        public override void RunBlock(string state)
        {
            Console.WriteLine("Moving " + Direction.ToString());
        }
    }
}
