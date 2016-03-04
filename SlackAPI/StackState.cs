using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using Pook.SlackAPI.APIMessages;

namespace Pook.SlackAPI
{
	public class StackState
	{
		public StackState(LoginResponse login)
		{
			Url = login.url;
			Self = login.self;
			Team = login.team;

			foreach (var u in login.users)
				users[u.id] = u;
			foreach (var c in login.channels)
				channels[c.id] = c;
			foreach (var g in login.groups)
				groups[g.id] = g;
			foreach (var i in login.ims)
				ims[i.id] = i;
			foreach (var b in login.bots)
				bots[b.id] = b;
		}

		public bool HelloReceived { get; set; }

		public string Url { get; }
		public Self Self { get; }
		public Team Team { get; }

		public IEnumerable<User> Users => users.Values;
		private readonly ConcurrentDictionary<string, User> users = new ConcurrentDictionary<string, User>();
		public void AdUpdate(User user)
		{
			users[user.id] = user;
		}
		public User GetUser(string id)
		{
			User user;
			if(users.TryGetValue(id, out user))
				return user;

			return null;
		}

		public IEnumerable<Channel> Channels => channels.Values;
		private readonly ConcurrentDictionary<string, Channel> channels = new ConcurrentDictionary<string, Channel>();
		public void AdUpdate(Channel channel)
		{
			channels[channel.id] = channel;
		}

		public IEnumerable<Channel> Groups => groups.Values;
		private readonly ConcurrentDictionary<string, Channel> groups = new ConcurrentDictionary<string, Channel>();
		public void AdUpdateGroup(Channel group)
		{
			groups[group.id] = group;
		}

		/// <summary>
		/// list of IM objects, one for every direct message channel visible to the authenticated user.
		/// </summary>
		public IEnumerable<DirectMessageChannel> IMS => ims.Values;
		private readonly ConcurrentDictionary<string, DirectMessageChannel> ims = new ConcurrentDictionary<string, DirectMessageChannel>();
		public void AdUpdate(DirectMessageChannel channel)
		{
			ims[channel.id] = channel;
		}

		public ICollection<Bot> Bots => bots.Values;
		private readonly ConcurrentDictionary<string, Bot> bots = new ConcurrentDictionary<string, Bot>();

		public void AdUpdate(Bot bot)
		{
			bots[bot.id] = bot;
		}
	}
}