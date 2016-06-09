using System;
using Pook.SlackAPI.APIMessages;

namespace Pook.SlackAPI
{
    public class SlackUserProfile
    {
        public SlackUserProfile(UserProfile profile)
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