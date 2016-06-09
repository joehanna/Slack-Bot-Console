using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using Pook.SlackAPI.APIMessages;
using System;

namespace Pook.SlackAPI
{
    public class SlackState
    {
        public SlackState(LoginResponse login)
        {
            Url = login.url;
            Self = login.self;
            Team = login.team;

            foreach (var u in login.users)
                AddUpdate(u);
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

        public IEnumerable<SlackUser> Users => users.Values;
        private readonly ConcurrentDictionary<string, SlackUser> users = new ConcurrentDictionary<string, SlackUser>();
        public void AddUpdate(User user)
        {
            users[user.id] = new SlackUser(user);
        }
        public SlackUser GetUser(string id)
        {
            SlackUser user;
            if (users.TryGetValue(id, out user))
                return user;

            return null;
        }

        public IEnumerable<Channel> Channels => channels.Values;
        private readonly ConcurrentDictionary<string, Channel> channels = new ConcurrentDictionary<string, Channel>();
        public void AddUpdate(Channel channel)
        {
            channels[channel.id] = channel;
        }
        public Channel GetChannel(string id)
        {
            Channel channel;
            if (channels.TryGetValue(id, out channel))
                return channel;

            return null;
        }

        public IEnumerable<Channel> Groups => groups.Values;
        private readonly ConcurrentDictionary<string, Channel> groups = new ConcurrentDictionary<string, Channel>();
        public void AddUpdateGroup(Channel group)
        {
            groups[group.id] = group;
        }
        public Channel GetGroup(string id)
        {
            Channel channel;
            if (groups.TryGetValue(id, out channel))
                return channel;

            return null;
        }

        /// <summary>
        /// list of IM objects, one for every direct message channel visible to the authenticated user.
        /// </summary>
        public IEnumerable<DirectMessageChannel> IMS => ims.Values;
        private readonly ConcurrentDictionary<string, DirectMessageChannel> ims = new ConcurrentDictionary<string, DirectMessageChannel>();
        public void AddUpdate(DirectMessageChannel channel)
        {
            ims[channel.id] = channel;
        }
        public DirectMessageChannel GetDMChannel(string id)
        {
            DirectMessageChannel channel;
            if (ims.TryGetValue(id, out channel))
                return channel;

            return null;
        }

        public ICollection<Bot> Bots => bots.Values;
        private readonly ConcurrentDictionary<string, Bot> bots = new ConcurrentDictionary<string, Bot>();
        public void AddUpdate(Bot bot)
        {
            bots[bot.id] = bot;
        }
        public Bot GetBot(string id)
        {
            Bot bot;
            if (bots.TryGetValue(id, out bot))
                return bot;

            return null;
        }
    }
}