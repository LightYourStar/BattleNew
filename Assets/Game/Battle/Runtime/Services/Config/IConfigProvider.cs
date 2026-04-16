namespace Game.Battle.Runtime.Services.Config
{
    public interface IConfigProvider
    {
        T Get<T>(string key);
    }
}
