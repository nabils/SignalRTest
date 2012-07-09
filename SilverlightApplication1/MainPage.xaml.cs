using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ImageTools;
using ImageTools.IO.Png;
using Microsoft.Practices.Prism.Commands;
using SignalR.Client.Hubs;

namespace SilverlightApplication1
{
    public class UserConnection
    {
        public string ConnectionId { get; set; }
        public string UserName { get; set; }
        public DateTime ConnectedSinceUtcDate { get; set; }
        public string ConnectedSinceRelativeDate { get { return ToRelativeDate(ConnectedSinceUtcDate.ToLocalTime()); } }

        public static string ToRelativeDate(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            // span is less than or equal to 60 seconds, measure in seconds.
            if (timeSpan <= TimeSpan.FromSeconds(60))
            {
                if (timeSpan.Seconds == 0) return "just now";

                return timeSpan.Seconds + " seconds ago";
            }
            // span is less than or equal to 60 minutes, measure in minutes.
            if (timeSpan <= TimeSpan.FromMinutes(60))
            {
                return timeSpan.Minutes > 1
                           ? "about " + timeSpan.Minutes + " minutes ago"
                           : "about a minute ago";
            }
            // span is less than or equal to 24 hours, measure in hours.
            if (timeSpan <= TimeSpan.FromHours(24))
            {
                return timeSpan.Hours > 1
                           ? "about " + timeSpan.Hours + " hours ago"
                           : "about an hour ago";
            }
            // span is less than or equal to 30 days (1 month), measure in days.
            if (timeSpan <= TimeSpan.FromDays(30))
            {
                return timeSpan.Days > 1
                           ? "about " + timeSpan.Days + " days ago"
                           : "about a day ago";
            }
            // span is less than or equal to 365 days (1 year), measure in months.
            if (timeSpan <= TimeSpan.FromDays(365))
            {
                return timeSpan.Days > 30
                           ? "about " + timeSpan.Days/30 + " months ago"
                           : "about a month ago";
            }

            // span is greater than 365 days (1 year), measure in years.
            return timeSpan.Days > 365
                       ? "about " + timeSpan.Days/365 + " years ago"
                       : "about a year ago";
        }
    }

    public class UserConnectionInfo
    {
        public int NoOfConnectedUsers { get; set; }
        public UserConnection[] UserConnections { get; set; }
    }

    public partial class MainPage : UserControl, INotifyPropertyChanged
    {
        private IHubProxy _hub;
        private int? _noOfUsers;
        private string _userName;
        private ObservableCollection<UserConnection> _userConnections;

        public MainPage()
        {
            InitializeComponent();
            _userConnections = new ObservableCollection<UserConnection>();
            TakeScreenshotCommand = new DelegateCommand<string>(connectionId => _hub.Invoke<string>("RequestScreenshot", connectionId));
            ConnectCommand = new DelegateCommand(() =>
                {
                    var conn = new HubConnection("http://localhost:63531/");
                    _hub = conn.CreateProxy("Chat");

                    _hub.On<UserConnectionInfo>("NoOfUsersUpdated", userConnectionInfo => Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        NoOfUsers = userConnectionInfo.NoOfConnectedUsers;
                        _userConnections = new ObservableCollection<UserConnection>(userConnectionInfo.UserConnections);
                        OnPropertyChanged("CurrentUsers");
                    }));

                    _hub.On("TakeScreenshot", () => Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            var screenshot = new WriteableBitmap(LayoutRoot, null);
                            using (var writestream = new MemoryStream())
                            {
                                var encoder = new PngEncoder();
                                encoder.Encode(screenshot.ToImage(), writestream);
                                _hub.Invoke<byte[]>("ScreenshotTaken", writestream.ToArray());
                            }
                        }));

                    conn.Start().ContinueWith(t =>
                    {
                        _hub.Invoke<string>("UserJoined", UserName);
                    });
                });
            this.DataContext = this;    
        }

        public ICommand TakeScreenshotCommand { get; set; }
        public ICommand ConnectCommand { get; set; }

        private CollectionViewSource _currentUsers;
        public ICollectionView CurrentUsers
        {
            get
            {
                if (_currentUsers == null)
                {
                    _currentUsers = new CollectionViewSource();
                    _currentUsers.SortDescriptions.Add(new SortDescription("ConnectedSinceUtcDate", ListSortDirection.Descending));
                }

                _currentUsers.Source = _userConnections;
                return _currentUsers.View;
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged("UserName");
            }
        }

        public int? NoOfUsers
        {
            get { return _noOfUsers; }
            set
            {
                _noOfUsers = value;
                OnPropertyChanged("NoOfUsers");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
