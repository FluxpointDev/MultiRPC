using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MultiRPC.Core.Rpc
{
    //TODO: Make it react things
    //TODO: Somehow get application Name from ID
    public class RichPresence : INotifyPropertyChanged
    {
        public RichPresence(string applicationName, long applicationId)
        {
            ApplicationName = applicationName;
            ApplicationId = applicationId;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string memName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memName));

        public RichPresence Clone() => new RichPresence(ApplicationName, ApplicationId)
        {
            State = State,
            Details = Details,
            Timestamp = Timestamp,
            Assets = Assets?.SmallImage == null && Assets?.LargeImage == null ? null :
            new Assets
            {
                LargeImage = Assets?.LargeImage,
                SmallImage = Assets?.SmallImage
            },
            Party = Party,
            Secret = Secret
        };

        public string ApplicationName { get; }
        public long ApplicationId { get; }

        private string state;
        public string State 
        {
            get => state;
            set
            {
                state = value;
                OnPropertyChanged();
            }
        }

        private string details;
        public string Details
        {
            get => details;
            set
            {
                details = value;
                OnPropertyChanged();
            }
        }

        private Timestamp timestamp = new Timestamp();
        public Timestamp Timestamp
        {
            get => timestamp;
            set
            {
                timestamp = value;
                OnPropertyChanged();
            }
        }

        private Assets assets = new Assets();
        public Assets Assets 
        {
            get => assets;
            set
            {
                assets = value;
                OnPropertyChanged();
            }
        }

        private Party party = new Party();
        public Party Party 
        {
            get => party;
            set
            {
                party = value;
                OnPropertyChanged();
            }
        }

        private Secret secret = new Secret();
        public Secret Secret 
        {
            get => secret;
            set
            {
                secret = value;
                OnPropertyChanged();
            }
        }
    }

    public class User
    {
        public long ID { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public string Avatar { get; set; }
        public bool Bot { get; }
    }

    public class Party
    {
        public string ID { get; set; }
        public int Size { get; set; }
        public int MaxSize { get; set; }

        //TODO: Add
        public bool IsValid() => false;
    }

    public class Assets
    {
        public Image SmallImage { get; set; } = new Image();
        public Image LargeImage { get; set; } = new Image();
    }

    public class Secret
    {
        public string Match { get; set; }
        public string Join { get; set; }
        public string Spectate { get; set; }

        //TODO: Add
        public bool IsValid() => false;
    }

    public class Image
    {
        public Image()
        {
        }

        public string Key { get; set; }
        public string Text { get; set; }

        /// <summary>
        /// Where the image is stored, it's the <see cref="IRpcClient"/> job to set this
        /// </summary>
        public Uri Uri { get; set; }
    }

    public class Timestamp
    {
        public DateTime? Start { get; set; } = null;
        public DateTime? End { get; set; } = null;

        /// <summary>
        /// If we should set start when we start an rpc connection
        /// </summary>
        public bool SetStartOnRPCConnection { get; set; }

        //TODO: Add
        public bool IsValid() => true;
    }

    public class Invite
    {
        public ActionType Type { get; }

        public User User { get; }

        public RichPresence RichPresence { get; }
    }

    public enum JoinRequestReply
    {
        No = 0,
        Yes = 1,
        Ignore = 2
    }

    public enum ActionType
    {
        Join = 1,
        Spectate = 2
    }

    public enum Result
    {
        Success,
        Failed
    }
}