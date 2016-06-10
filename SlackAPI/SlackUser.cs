using System.Collections.Concurrent;
using Pook.SlackAPI.RTMMessages;
using System;
using System.Threading.Tasks;

namespace Pook.SlackAPI
{
	public class SlackUser
	{
		public SlackUser(APIMessages.User user)
		{
			Id = user.id;
			Name = user.name;
			Deleted = user.deleted;
			Color = user.color;
			Profile = new SlackUserProfile(user.profile);
			IsAdmin = user.is_admin;
			IsOwner = user.is_owner;
			HasFiles = user.has_files;
			Presence = user.presence;
		}

		private readonly ConcurrentDictionary<object, object> state = new ConcurrentDictionary<object, object>();

		public string Id { get; }
		public bool IsSlackBot => Id.Equals("USLACKBOT", StringComparison.OrdinalIgnoreCase);
		public string Name { get; }
		public bool Deleted { get; }
		public string Color { get; }
		public SlackUserProfile Profile { get; }
		public bool IsAdmin { get; }
		public bool IsOwner { get; }
		public bool HasFiles { get; }
		public string Presence { get; set; }

		public void SetState<T>(T item)
		{
			state[typeof(T)] = item;
		}
		public void SetState(string name, object item)
		{
			state[name] = item;
		}

		public T GetState<T>()
		{
			object result;
			state.TryGetValue(typeof(T), out result);
			return (T)result;
		}
		public T GetState<T>(string name)
		{
			object result;
			state.TryGetValue(name, out result);
			return (T)result;
		}

		public Func<ISlackSocket, Message, SlackUser, Task<bool>> NextStep { get; set; }

		public override string ToString()
		{
			return $"{Name}<{Id}>";
		}
	}
}