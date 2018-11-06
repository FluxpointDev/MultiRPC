using Newtonsoft.Json;
using System;
using DiscordRPC.Helper;
using System.Text;
using DiscordRPC.Exceptions;

namespace DiscordRPC
{
	/// <summary>
	/// The Rich Presence structure that will be sent and received by Discord. Use this class to build your presence and update it appropriately.
	/// </summary>
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	[Serializable]
	public class RichPresence
	{
		/// <summary>
		/// The user's current <see cref="Party"/> status. For example, "Playing Solo" or "With Friends".
		/// <para>Max 128 bytes</para>
		/// </summary>
		[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
		public string State
		{
			get { return _state; }
			set
			{
				if (!ValidateString(value, out _state, 128, Encoding.UTF8))
					throw new StringOutOfRangeException("State", 128);
			}
		}
		private string _state;

		/// <summary>
		/// What the user is currently doing. For example, "Competitive - Total Mayhem".
		/// <para>Max 128 bytes</para>
		/// </summary>
		[JsonProperty("details", NullValueHandling = NullValueHandling.Ignore)]
		public string Details
		{
			get { return _details; }
			set 
			{
				if (!ValidateString(value, out _details, 128, Encoding.UTF8))
					throw new StringOutOfRangeException("Details", 128);
			}
		}
		private string _details;
		
		/// <summary>
		/// The time elapsed / remaining time data.
		/// </summary>
		[JsonProperty("timestamps", NullValueHandling = NullValueHandling.Ignore)]
		public Timestamps Timestamps { get; set; }

		/// <summary>
		/// The names of the images to use and the tooltips to give those images.
		/// </summary>
		[JsonProperty("assets", NullValueHandling = NullValueHandling.Ignore)]
		public Assets Assets { get; set; }
		
		/// <summary>
		/// Clones the presence into a new instance. Used for thread safe writing and reading. This function will ignore properties if they are in a invalid state.
		/// </summary>
		/// <returns></returns>
		public RichPresence Clone()
		{
			return new RichPresence
			{
				State = this._state != null ? _state.Clone() as string : null,
				Details = this._details != null ? _details.Clone() as string : null,

				Timestamps = !HasTimestamps() ? null : new Timestamps
				{
					Start = this.Timestamps.Start,
					End = this.Timestamps.End
				},

				Assets = !HasAssets() ? null : new Assets
				{
					LargeImageKey = this.Assets.LargeImageKey != null ? this.Assets.LargeImageKey.Clone() as string  : null,
					LargeImageText = this.Assets.LargeImageText != null ? this.Assets.LargeImageText.Clone() as string : null,
					SmallImageKey = this.Assets.SmallImageKey != null ? this.Assets.SmallImageKey.Clone() as string : null,
					SmallImageText = this.Assets.SmallImageText != null ? this.Assets.SmallImageText.Clone() as string : null
				},
			};
		}

		/// <summary>
		/// Merges the passed presence with this one, taking into account the image key to image id annoyance.
		/// </summary>
		/// <param name="presence"></param>
		internal void Merge(RichPresence presence)
		{
			this._state = presence._state;
			this._details = presence._details;
			this.Timestamps = presence.Timestamps;

			//If they have assets, we should merge them
			if (presence.HasAssets())
			{
				//Make sure we actually have assets too
				if (!this.HasAssets())
				{
					//We dont, so we will just use theirs
					this.Assets = presence.Assets;
				}
				else
				{
					//We do, so we better merge them!
					this.Assets.Merge(presence.Assets);
				}
			}
			else
			{
				//They dont have assets, so we will just set ours to null
				this.Assets = null;
			}	
		}

		/// <summary>
		/// Updates this presence with any values from the previous one
		/// </summary>
		/// <param name="presence"></param>
		internal void Update(RichPresence presence)
		{
			if (presence == null) return;

			this._state		= presence._state	?? this._state;
			this._details	= presence._details ?? this._details;
		}

		/// <summary>
		/// Does the Rich Presence have valid timestamps?
		/// </summary>
		/// <returns></returns>
		public bool HasTimestamps()
		{
			return this.Timestamps != null && (Timestamps.Start != null || Timestamps.End != null);
		}

		/// <summary>
		/// Does the Rich Presence have valid assets?
		/// </summary>
		/// <returns></returns>
		public bool HasAssets()
		{
			return this.Assets != null;
		}
		
		/// <summary>
		/// Attempts to call <see cref="StringTools.NullEmpty(string)"/> on the string and return the result, if its within a valid length.
		/// </summary>
		/// <param name="str">The string to check</param>
		/// <param name="result">The formatted string result</param>
		/// <param name="bytes">The maximum number of bytes the string can take up</param>
		/// <param name="encoding">The encoding to count the bytes with</param>
		/// <returns>True if the string fits within the number of bytes</returns>
		internal static bool ValidateString(string str, out string result, int bytes, Encoding encoding)
		{
			result = str;
			if (str == null)
				return true;

            //Trim the string, for the best chance of fitting
            string s = str.Trim();

			//Make sure it fits
			if (!s.WithinLength(bytes, encoding))
				return false;

			//Make sure its not empty
			result = s.NullEmpty();
			return true;
		}

		/// <summary>
		/// Operator that converts a presence into a boolean for null checks.
		/// </summary>
		/// <param name="presesnce"></param>
		public static implicit operator bool(RichPresence presesnce)
		{
			return presesnce != null;
		}
	}

	/// <summary>
	/// Information about the pictures used in the Rich Presence.
	/// </summary>
	[Serializable]
	public class Assets
	{
		/// <summary>
		/// Name of the uploaded image for the large profile artwork.
		/// <para>Max 32 Bytes.</para>
		/// </summary>
		[JsonProperty("large_image", NullValueHandling = NullValueHandling.Ignore)]
		public string LargeImageKey
		{
			get { return _largeimagekey; }
			set
			{
				if (!RichPresence.ValidateString(value, out _largeimagekey, 32, Encoding.UTF8))
					throw new StringOutOfRangeException("LargeImageKey", 32);

				//Reset the large image ID
				_largeimageID = null;
			}
		}
		private string _largeimagekey;

		/// <summary>
		/// The tooltip for the large square image. For example, "Summoners Rift" or "Horizon Lunar Colony".
		/// <para>Max 128 Bytes.</para>
		/// </summary>
		[JsonProperty("large_text", NullValueHandling = NullValueHandling.Ignore)]
		public string LargeImageText
		{
			get { return _largeimagetext; }
			set
			{
				if (!RichPresence.ValidateString(value, out _largeimagetext, 128, Encoding.UTF8))
					throw new StringOutOfRangeException("LargeImageText", 128);
			}
		}
		private string _largeimagetext;


		/// <summary>
		/// Name of the uploaded image for the small profile artwork.
		/// <para>Max 32 Bytes.</para>
		/// </summary>
		[JsonProperty("small_image", NullValueHandling = NullValueHandling.Ignore)]
		public string SmallImageKey
		{
			get { return _smallimagekey; }
			set
			{
				if (!RichPresence.ValidateString(value, out _smallimagekey, 32, Encoding.UTF8))
					throw new StringOutOfRangeException("SmallImageKey", 32);

				//Reset the small image id
				_smallimageID = null;
			}
		}
		private string _smallimagekey;

		/// <summary>
		/// The tooltip for the small circle image. For example, "LvL 6" or "Ultimate 85%".
		/// <para>Max 128 Bytes.</para>
		/// </summary>
		[JsonProperty("small_text", NullValueHandling = NullValueHandling.Ignore)]
		public string SmallImageText
		{
			get { return _smallimagetext; }
			set
			{
				if (!RichPresence.ValidateString(value, out _smallimagetext, 128, Encoding.UTF8))
					throw new StringOutOfRangeException("SmallImageText", 128);
			}
		}
		private string _smallimagetext;

		/// <summary>
		/// The ID of the large image. This is only set after Update Presence and will automatically become null when <see cref="LargeImageKey"/> is changed.
		/// </summary>
		[JsonIgnore]
		public ulong? LargeImageID { get { return _largeimageID; } }
		private ulong? _largeimageID;

		/// <summary>
		/// The ID of the small image. This is only set after Update Presence and will automatically become null when <see cref="SmallImageKey"/> is changed.
		/// </summary>
		[JsonIgnore]
		public ulong? SmallImageID { get { return _smallimageID; } }
		private ulong? _smallimageID;

		/// <summary>
		/// Merges this asset with the other, taking into account for ID's instead of keys.
		/// </summary>
		/// <param name="other"></param>
		internal void Merge(Assets other)
		{
			//Copy over the names
			_smallimagetext = other._smallimagetext;
			_largeimagetext = other._largeimagetext;

            //Convert large ID
            if (ulong.TryParse(other._largeimagekey, out ulong largeID))
            {
                _largeimageID = largeID;
            }
            else
            {
                _largeimagekey = other._largeimagekey;
                _largeimageID = null;
            }

            //Convert the small ID
            if (ulong.TryParse(other._smallimagekey, out ulong smallID))
            {
                _smallimageID = smallID;
            }
            else
            {
                _smallimagekey = other._smallimagekey;
                _smallimageID = null;
            }
        }
	}

	/// <summary>
	/// Structure representing the start and endtimes of a match.
	/// </summary>
	[Serializable]
	public class Timestamps
    {
        /// <summary> Creates a new timestamp for now. </summary>
        public static Timestamps Now { get { return new Timestamps(DateTime.UtcNow); } }

        /// <summary>
        /// Creates a new timestamp starting at UtcNow and ending in the supplied timespan
        /// </summary>
        /// <param name="seconds">How long the Timestamp will last for in seconds.</param>
        /// <returns>Returns a new timestamp with given duration.</returns>
        public static Timestamps FromTimeSpan(double seconds) { return FromTimeSpan(TimeSpan.FromSeconds(seconds)); }

        /// <summary>
        /// Creates a new timestamp starting at UtcNow and ending in the supplied timespan
        /// </summary>
        /// <param name="timespan">How long the Timestamp will last for.</param>
        /// <returns>Returns a new timestamp with given duration.</returns>
        public static Timestamps FromTimeSpan(TimeSpan timespan)
        {
            return new Timestamps()
            {
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow + timespan
            };
        }

        /// <summary>
        /// The time that match started. When included (not-null), the time in the rich presence will be shown as "00:01 elapsed".
        /// </summary>
        [JsonIgnore]
		public DateTime? Start { get; set; }

		/// <summary>
		/// The time the match will end. When included (not-null), the time in the rich presence will be shown as "00:01 remaining". This will override the "elapsed" to "remaining".
		/// </summary>
		[JsonIgnore]
		public DateTime? End { get; set; }

        /// <summary>
        /// Creates a empty timestamp object
        /// </summary>
        public Timestamps()
        {
            Start = null;
            End = null;
        }

        /// <summary>
        /// Creates a timestamp with the set start or end time.
        /// </summary>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        public Timestamps(DateTime start, DateTime? end = null)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Converts between DateTime and Milliseconds to give the Unix Epoch Time for the <see cref="Timestamps.Start"/>.
        /// </summary>
        [JsonProperty("start", NullValueHandling = NullValueHandling.Ignore)]
		public ulong? StartUnixMilliseconds
		{
			get
			{
				return Start.HasValue ? ToUnixMilliseconds(Start.Value) : (ulong?)null;
			}

			set
			{
				Start = value.HasValue ? FromUnixMilliseconds(value.Value) : (DateTime?)null;
			}
		}


        /// <summary>
        /// Converts between DateTime and Milliseconds to give the Unix Epoch Time  for the <see cref="Timestamps.End"/>.
        /// <seealso cref="End"/>
        /// </summary>
		[JsonProperty("end", NullValueHandling = NullValueHandling.Ignore)]
		public ulong? EndUnixMilliseconds
		{
			get
			{
				return End.HasValue ? ToUnixMilliseconds(End.Value) : (ulong?)null;
			}

			set
			{
				End = value.HasValue ? FromUnixMilliseconds(value.Value) : (DateTime?)null;
			}
		}
		
		/// <summary>
		/// Converts a Unix Epoch time into a <see cref="DateTime"/>.
		/// </summary>
		/// <param name="unixTime">The time in milliseconds since 1970 / 01 / 01</param>
		/// <returns></returns>
		public static DateTime FromUnixMilliseconds(ulong unixTime)
		{
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return epoch.AddMilliseconds(Convert.ToDouble(unixTime));
		}

		/// <summary>
		/// Converts a <see cref="DateTime"/> into a Unix Epoch time (in milliseconds).
		/// </summary>
		/// <param name="date">The datetime to convert</param>
		/// <returns></returns>
		public static ulong ToUnixMilliseconds(DateTime date)
		{
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return Convert.ToUInt64((date - epoch).TotalMilliseconds);
		}

	}

	
	/// <summary>
	/// A rich presence that has been parsed from the pipe as a response.
	/// </summary>
	internal class RichPresenceResponse : RichPresence
	{
        /// <summary>
        /// ID of the client
        /// </summary>
        [JsonProperty("application_id")]
        public string ClientID { get; private set; }

        /// <summary>
        /// Name of the bot
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

	}
}
