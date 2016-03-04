namespace Pook.SlackAPI
{
	public interface IEventHandler<T>
	{
		void Handle(ISlackSocket socket, T message);
	}
}