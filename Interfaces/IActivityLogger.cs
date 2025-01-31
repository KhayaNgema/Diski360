namespace MyField.Interfaces
{
    public interface IActivityLogger
    {
        Task Log(string activity, string userId);
    }
}
