namespace Reactor.Entities
{
    public interface IReaction
    {
        string Name { get; set; }
        void Execute();
    }

    public abstract class Reaction : IReaction
    {
        public string Name { get; set; }

        public abstract void Execute();
    }
}
