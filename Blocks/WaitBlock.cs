using GridRunner.Interfaces;

namespace GridRunner.Blocks
{
    public record class WaitBlock : Block
    {
        public WaitBlock()
        {
            FillColor = "#d1d5db";
        }
        public override void RunBlock(string state)
        {
            Console.WriteLine("Waiting... (State: " + state + ")");
        }
    }
}
