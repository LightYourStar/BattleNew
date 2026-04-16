namespace Game.Battle.Runtime.Commands
{
    public enum CommandSource
    {
        LocalInput = 0,
        RemoteSync = 1,
        Replay = 2,
    }

    public interface IFrameCommand
    {
        int Frame { get; }

        CommandSource Source { get; }
    }
}
