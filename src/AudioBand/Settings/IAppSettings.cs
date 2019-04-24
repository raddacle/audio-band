﻿using System;
using System.Collections.Generic;
using AudioBand.Models;

namespace AudioBand.Settings
{
    /// <summary>
    /// Manages application settings.
    /// </summary>
    public interface IAppSettings
    {
        event EventHandler ProfileChanged;

        /// <summary>
        ///  Gets or sets the saved audio source.
        /// </summary>
        string AudioSource { get; set; }

        /// <summary>
        /// Gets the saved album art popup model.
        /// </summary>
        AlbumArtPopup AlbumArtPopup { get; }

        /// <summary>
        /// Gets the saved album art model.
        /// </summary>
        AlbumArt AlbumArt { get; }

        /// <summary>
        /// Gets the saved audio band model.
        /// </summary>
        AudioBand.Models.AudioBand AudioBand { get; }

        /// <summary>
        /// Gets the saved labels.
        /// </summary>
        List<CustomLabel> CustomLabels { get; }

        /// <summary>
        /// Gets the saved button model.
        /// </summary>
        NextButton NextButton { get; }

        /// <summary>
        /// Gets the saved previous button model.
        /// </summary>
        PreviousButton PreviousButton { get; }

        /// <summary>
        /// Gets the saved play pause button model.
        /// </summary>
        PlayPauseButton PlayPauseButton { get; }

        /// <summary>
        /// Gets the saved progress bar model.
        /// </summary>
        ProgressBar ProgressBar { get; }

        /// <summary>
        /// Gets the saved audio source settings.
        /// </summary>
        List<AudioSourceSettings> AudioSourceSettings { get; }

        /// <summary>
        /// Gets or sets the current profile.
        /// </summary>
        string CurrentProfile { get; set; }

        /// <summary>
        /// Gets the list of profiles
        /// </summary>
        List<string> Profiles { get; }

        /// <summary>
        /// Creates a new profile.
        /// </summary>
        /// <param name="profileName">The name of the new profile.</param>
        void CreateProfile(string profileName);

        /// <summary>
        /// Deletes a profile.
        /// </summary>
        /// <param name="profileName">The name of the profile to delete.</param>
        void DeleteProfile(string profileName);

        void RenameCurrentProfile(string newProfileName);

        /// <summary>
        /// Save the settings.
        /// </summary>
        void Save();
    }
}