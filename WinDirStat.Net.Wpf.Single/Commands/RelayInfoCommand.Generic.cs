using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using WinDirStat.Net.Wpf.Input;

namespace WinDirStat.Net.Wpf.Commands {
    public class RelayInfoCommand<T> : IRelayInfoCommand, IRelayCommand<T> {

        #region Fields

        private readonly RelayCommand<T> command;

        /// <summary>The UI specific info for the command.</summary>
        private RelayInfo info;

        #endregion

        #region Constructors

        public RelayInfoCommand(Action<T> execute) {
            command = new(execute);
        }

        public RelayInfoCommand(Action<T> execute, Predicate<T> canExecute) {
            command = new(execute, canExecute);
        }

        #endregion

        #region Properties

        /// <summary>Gets or sets the UI specific info for the command.</summary>
        public RelayInfo Info {
            get => info;
            set {
                if (info != value) {
                    if (info != null)
                        info.PropertyChanged -= OnInfoPropertyChanged;
                    info = value;
                    if (info != null)
                        info.PropertyChanged += OnInfoPropertyChanged;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>Gets the display text for the command.</summary>
        public string Text => info?.Text;
        /// <summary>Gets the display icon for the command.</summary>
        public ImageSource Icon => info?.Icon;
        /// <summary>Gets the input gesture for the command.</summary>
        public AnyKeyGesture InputGesture => info?.InputGesture;

        #endregion

        #region Events

        /// <summary>Called when a property has changed.</summary>
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CanExecuteChanged {
            add => command.CanExecuteChanged += value;
            remove => command.CanExecuteChanged -= value;
        }

        #endregion

        #region Execute

        public void NotifyCanExecuteChanged() => command.NotifyCanExecuteChanged();

        public bool CanExecute(object parameter) => command.CanExecute(parameter);

        public void Execute(object parameter = null) => command.Execute(parameter);

        public bool CanExecute(T? parameter) => command.CanExecute(parameter);

        public void Execute(T? parameter) => command.Execute(parameter);

        #endregion

        #region Private PropertyChanged

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Event Handlers

        private void OnInfoPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(RelayInfo.Text):
                case nameof(RelayInfo.Icon):
                case nameof(RelayInfo.InputGesture):
                    OnPropertyChanged(e.PropertyName);
                    break;
            }
        }

        #endregion
    }
}