using System.Collections.Concurrent;
using Pook.SlackAPI.APIMessages;
using System;

namespace Pook.SlackAPI
{
    public class TeamMember
    {
        public TeamMember(User user)
        {
            Id = user.id;
            Name = user.name;
            Deleted = user.deleted;
            Color = user.color;
            Profile = new TeamMemberProfile(user.profile);
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
        public TeamMemberProfile Profile { get; }
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

        public override string ToString()
        {
            return Name;
        }
    }
    public class TeamMemberProfile
    {
        public TeamMemberProfile(UserProfile profile)
        {
            FirstName = profile.first_name;
            LastName = profile.last_name;
            RealName = profile.real_name;
            Email = profile.email;
            Skype = profile.skype;
            Phone = profile.phone;
            Image24 = profile.image_24;
            Image32 = profile.image_32;
            Image48 = profile.image_48;
            Image72 = profile.image_72;
            Image192 = profile.image_192;
        }

        public string FirstName { get; }
        public string LastName { get; }
        public string RealName { get; }
        public string Email { get; }
        public string Skype { get; }
        public string Phone { get; }
        public string Image24 { get; }
        public string Image32 { get; }
        public string Image48 { get; }
        public string Image72 { get; }
        public string Image192 { get; }

        public override string ToString()
        {
            return RealName;
        }
    }
}