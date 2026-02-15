using GridRunner.Enums;
using GridRunner.Interfaces;

namespace GridRunner.Blocks
{
    public record class MoveBlock : Block
    {
        public MoveActionEnum Direction { get; set; }

        public MoveBlock(MoveActionEnum direction) : base()
        {
            Description = direction.ToString();
            Direction = direction;
            FillColor = "#d1d5db";
        }

        public override void RunBlock(string state)
        {
            Console.WriteLine("Moving " + Direction.ToString());
        }
    }
}
