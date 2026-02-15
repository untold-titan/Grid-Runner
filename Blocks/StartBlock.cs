namespace GridRunner.Interfaces
{
    public record class StartBlock : Block
    {
        public StartBlock()
        {
            isFixed = true;
        }
        public override void RunBlock(string state)
        {
            Console.WriteLine("Ran Start Block ID: " + Id);
        }
    }
}
