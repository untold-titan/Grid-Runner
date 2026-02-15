namespace GridRunner.Interfaces
{
    public record class ConsoleBlock : Block
    {
        public override void RunBlock(string state)
        {
            Console.WriteLine("Im a block! with ID = " + Id);
        }
    }
}
