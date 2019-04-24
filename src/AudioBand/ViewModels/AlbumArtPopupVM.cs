﻿using System;
using System.Diagnostics;
using System.Drawing;
using AudioBand.Models;
using AudioBand.Resources;
using AudioBand.Settings;
using AutoMapper;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace AudioBand.ViewModels
{
    /// <summary>
    /// View model for the album art popup.
    /// </summary>
    public class AlbumArtPopupVM : ViewModelBase<AlbumArtPopup>
    {
        private readonly IAppSettings _appSettings;
        private readonly Track _track;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlbumArtPopupVM"/> class.
        /// </summary>
        /// <param name="appSettings">The app settings.</param>
        /// <param name="track">The track mode.</param>
        public AlbumArtPopupVM(IAppSettings appSettings, Track track)
            : base(appSettings.AlbumArtPopup)
        {
            _appSettings = appSettings;
            _track = track;
            SetupModelBindings(_track);
            appSettings.ProfileChanged += AppSettingsOnProfileChanged;
        }

        [PropertyChangeBinding(nameof(AlbumArtPopup.IsVisible))]
        public bool IsVisible
        {
            get => Model.IsVisible;
            set => SetProperty(nameof(Model.IsVisible), value);
        }

        [PropertyChangeBinding(nameof(AlbumArtPopup.Width))]
        [AlsoNotify(nameof(Size))]
        public int Width
        {
            get => Model.Width;
            set => SetProperty(nameof(Model.Width), value);
        }

        [PropertyChangeBinding(nameof(AlbumArtPopup.Height))]
        [AlsoNotify(nameof(Size))]
        public int Height
        {
            get => Model.Height;
            set => SetProperty(nameof(Model.Height), value);
        }

        [PropertyChangeBinding(nameof(AlbumArtPopup.XPosition))]
        public int XPosition
        {
            get => Model.XPosition;
            set => SetProperty(nameof(Model.XPosition), value);
        }

        [PropertyChangeBinding(nameof(AlbumArtPopup.Margin))]
        public int Margin
        {
            get => Model.Margin;
            set => SetProperty(nameof(Model.Margin), value);
        }

        [PropertyChangeBinding(nameof(Track.AlbumArt))]
        public IImage AlbumArt => _track.AlbumArt;

        /// <summary>
        /// Gets the size of the popup.
        /// </summary>
        /// <remarks>This property exists so the designer can bind to it.</remarks>
        public Size Size => new Size(Width, Height);

        private void AppSettingsOnProfileChanged(object sender, EventArgs e)
        {
            Debug.Assert(IsEditing == false, "Should not be editing");
            ReplaceModel(_appSettings.AlbumArtPopup);
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member