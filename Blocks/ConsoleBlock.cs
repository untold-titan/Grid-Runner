using GridRunner.Interfaces;

namespace GridRunner.Blocks
{
    public record class ConsoleBlock : Block
    {
        public ConsoleBlock() : base()
        {
            Description = "Console Block (Testing ONLY)";
            FillColor = "#00ffff";
        }

        public override void RunBlock(string state)
        {
            Console.WriteLine("Im a block! with ID = " + Id);
        }
    }
}
