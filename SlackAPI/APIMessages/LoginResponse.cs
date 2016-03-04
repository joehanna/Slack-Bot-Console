using System;
using System.Collections.Generic;

using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace Pook.SlackAPI.APIMessages
{
	public class SlackSocketMessage
	{
		public int id;
		public int reply_to;
		public string type;
		public string subtype;
		public bool ok = true;
		public Error error;
	}

	public class LoginResponse : Response
	{
		public string url;
		public Self self;
		public Team team;
		public User[] users;
		public Channel[] channels;
		public Channel[] groups;
		public DirectMessageChannel[] ims;
		public MultipartyIM[] mpims;
		public Bot[] bots;
	}

	public class Bot
	{
		public string emoji;
		public string image_24;
		public string image_32;
		public string image_48;
		public string image_72;
		public string image_192;

		public bool deleted;
		public UserProfile icons;
		public string id;
		public string name;
	}
	public class Channel
	{
		public string id;
		public string name;
		public bool is_channel;
		[JsonConverter(typeof(UnixDateTimeConverter))]
		public DateTime created;
		public string creator;

		public bool is_archived;
		public bool is_general;

		public string[] members;

		public ChannelDetails topic;
		public ChannelDetails purpose;

		public bool is_member;

		public string latest_read;
		public Message latest;

		public int unread_count;
		public int unread_count_display;
	}

	public class Error
	{
		public int code;
		public string msg;
	}
	public class ChannelDetails
	{
		public string value;
		public string creator;
		public string last_set;
	}
	public class DirectMessageChannel
	{
		public string id;
		public string user;
		[JsonConverter(typeof(UnixDateTimeConverter))]
		public DateTime created;
		public bool is_user_deleted;
		public bool is_open;
		public bool is_starred;
		public string last_read;
		public Message latest;
		public int unread_count;
	}
	public class Team
	{
		[JsonConverter(typeof(JavascriptBotsToArray))]
		public Bot[] bots;
		public string domain;
		/// <summary>
		/// Supported domains emails can be registered from.
		/// </summary>
		/// TODO: Is this obsolete?
		public string email_domain;
		/// <summary>
		/// Supported domains emails can be registered from.
		/// </summary>
		public string[] email_domains;

		public string id;

		public int msg_edit_window_mins;
		public string name;
		public bool over_storage_limit, sso;
		public TeamPreferences prefs;
		public string sso_required;
		public string sso_type;
		public string url;
		public SSOProvider[] sso_provider;
	}

	public class MultipartyIM
	{
		public string id;
		public string name;
		public bool is_mpim;
		public bool is_group;
		[JsonConverter(typeof(UnixDateTimeConverter))]
		public DateTime created;

		public string[] users;
		public string last_read;
		public Message lastest;
		public int unread_count;
		public int unread_count_display;
	}

	public class JavascriptBotsToArray : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return true;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var bots = new List<Bot>();
			int d = reader.Depth;

			while (reader.Read() && reader.Depth > d)
			{
				var current = new Bot();
				int depth = reader.Depth;

				current.name = reader.Value.ToString();

				reader.Read();
				while (reader.Read() && reader.Depth > depth)
				{
					if (reader.Value == null) break;
					switch (reader.Value.ToString())
					{
						case "image_48":
							reader.Read();
							current.image_48 = reader.Value.ToString();
							break;

						case "image_64":
							reader.Read();
							current.image_48 = reader.Value.ToString();
							break;

						case "emoji":
							reader.Read();
							current.emoji = reader.Value.ToString();
							break;
					}
				}

				bots.Add(current);
			}

			return bots.ToArray();
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			//Not sure if this is correct :D
			throw new NotSupportedException("Too hackish for this shi.");
		}
	}

	public class TeamPreferences
	{
		public AuthMode auth_mode;
		public string[] default_channels;
		public bool display_real_names;
		public int gateway_allow_irc_plain;
		public int gateway_allow_irc_ssl;
		public int gateway_allow_xmpp_ssl;
		public bool hide_referers;
		public int msg_edit_window_mins;
		public bool srvices_only_admins;
		public bool stats_only_admins;

		public enum AuthMode
		{
			normal,
			saml
		}
	}
	public class SSOProvider
	{
		public string name;
		public string type;
	}
	public class User
	{
		public string id;
		public bool IsSlackBot => id.Equals("USLACKBOT", StringComparison.CurrentCultureIgnoreCase);
		public string name;
		public bool deleted;
		public string color;
		public UserProfile profile;
		public bool is_admin;
		public bool is_owner;
		public bool has_files;
		public string presence;
	}
	public class UserProfile
	{
		public string first_name;
		public string last_name;
		public string real_name;
		public string email;
		public string skype;
		public string phone;
		public string image_24;
		public string image_32;
		public string image_48;
		public string image_72;
		public string image_192;

		public override string ToString()
		{
			return real_name;
		}
	}
	public class Self
	{
		[JsonConverter(typeof(UnixDateTimeConverter))]
		public DateTime created;
		public string id;
		public string manual_presence;
		public string name;
		public Preferences prefs;
	}
	public class Preferences
	{
		public bool email_misc;
		public bool push_everything;
		public bool seen_notification_prefs_banner;
		public bool seen_welcome_2;
		public bool seen_user_menu_tip_card;
		public bool seen_message_input_tip_card;
		public bool seen_channels_tip_card;
		public bool seen_team_menu_tip_card;
		public bool seen_flexpane_tip_card;
		public bool seen_search_input_tip_card;
		public bool has_uploaded;
		public bool search_only_my_channels;
		public bool seen_channel_menu_tip_card;
		public bool has_invited;
		public bool has_created_channel;
		public bool color_names_in_list;
		public bool growls_enabled;
		public bool push_dm_alert;
		public bool push_mention_alert;
		public bool welcome_message_hidden;
		public bool all_channels_loud;
		public bool show_member_presence;
		public bool expand_inline_imgs;
		public bool expand_internal_inline_imgs;
		public bool seen_ssb_prompt;
		public bool webapp_spellcheck;
		public bool no_joined_overlays;
		public bool no_created_overlays;
		public bool dropbox_enabled;
		public bool mute_sounds;
		public bool arrow_history;
		public bool tab_ui_return_selects;
		public bool obey_inline_img_limit;
		public bool collapsible;
		public bool collapsible_by_click;
		public bool require_at;
		public bool mac_ssb_bullet;
		public bool expand_non_media_attachments;
		public bool show_typing;
		public bool pagekeys_handled;
		public bool time24;
		public bool enter_is_special_in_tbt;
		public bool graphic_emoticons;
		public bool convert_emoticons;
		public bool autoplay_chat_sounds;
		public bool ss_emojis;
		public bool mark_msgs_read_immediately;
		public string tz;
		public string emoji_mode;
		public string hightlight_words;
		//public string newxp_slackbot_step; //I don't even...
		public SearchSort search_sort;
		public string push_loud_channels;
		public string push_mention_channels;
		public string push_loud_channels_set;
		public string user_colors;
		public int push_idle_wait;
		public string push_sound;
		public string email_alerts;
		public int email_alerts_sleep_until;
		public string loud_channels;
		public string never_channels;
		public string loud_channels_set;
		public string search_excluse_channels;
		public string messages_theme;
		public string new_msg_snd;
		public string mac_ssb_bounce;
		public string last_snippet_type;
		public int display_real_names_override;
	}
	public enum SearchSort
	{
		score,
		timestamp
	}
}
