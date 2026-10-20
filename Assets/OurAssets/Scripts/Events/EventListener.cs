
public interface IEventListener
{
    public abstract void OnEventReceived(string eventName, params object[] parameters);
}