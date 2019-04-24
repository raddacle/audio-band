﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using AudioBand.Logging;
using AudioBand.Models;
using AudioBand.Settings;
using AudioBand.Settings.Migrations;
using AudioBand.Settings.Models.v3;
using AutoMapper;
using Nett;
using NLog;

namespace AudioBandModel.Settings
{
    /// <summary>
    /// Manages application settings.
    /// </summary>
    public class AppSettings : IAppSettings
    {
        private static readonly Dictionary<string, Type> SettingsTable = new Dictionary<string, Type>()
        {
            { "0.1", typeof(AudioBand.Settings.Models.V1.AudioBandSettings) },
            { "2", typeof(AudioBand.Settings.Models.V2.Settings) },
            { "3", typeof(SettingsV3) }
        };

        private static readonly string CurrentVersion = "3";
        private static readonly string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AudioBand");
        private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "audioband.settings");
        private static readonly ILogger Logger = AudioBandLogManager.GetLogger<AppSettings>();
        private readonly TomlSettings _tomlSettings;
        private SettingsV3 _settings;
        private ProfileV3 _currentProfile;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettings"/> class.
        /// </summary>
        public AppSettings()
        {
            _tomlSettings = TomlSettings.Create(cfg =>
            {
                cfg.ConfigureType<Color>(type => type.WithConversionFor<TomlString>(convert => convert
                    .ToToml(ColorTranslator.ToHtml)
                    .FromToml(tomlString => ColorTranslator.FromHtml(tomlString.Value))));
                cfg.ConfigureType<CustomLabel.TextAlignment>(type => type.WithConversionFor<TomlString>(convert => convert
                    .ToToml(SerializationConversions.EnumToString)
                    .FromToml(str => SerializationConversions.StringToEnum<CustomLabel.TextAlignment>(str.Value))));
            });

            if (!Directory.Exists(SettingsDirectory))
            {
                Directory.CreateDirectory(SettingsDirectory);
            }

            if (!File.Exists(SettingsFilePath))
            {
                CreateDefault();
                Toml.WriteFile(_settings, SettingsFilePath, _tomlSettings);
            }
            else
            {
                LoadSettings();
            }

            AudioSourceSettings = _settings.AudioSourceSettings ?? new List<AudioSourceSettings>();
            SelectProfile(_settings.CurrentProfileName);
        }

        public event EventHandler ProfileChanged;

        /// <summary>
        /// Gets or sets the name of the current audio source.
        /// </summary>
        public string AudioSource
        {
            get => _settings.AudioSource;
            set => _settings.AudioSource = value;
        }

        /// <summary>
        /// Gets the saved album art popup model.
        /// </summary>
        public AlbumArtPopup AlbumArtPopup { get; private set; } = new AlbumArtPopup();

        /// <summary>
        /// Gets the saved album art model.
        /// </summary>
        public AlbumArt AlbumArt { get; private set; } = new AlbumArt();

        /// <summary>
        /// Gets the saved audio band model.
        /// </summary>
        public AudioBand.Models.AudioBand AudioBand { get; private set; } = new AudioBand.Models.AudioBand();

        /// <summary>
        /// Gets the saved labels.
        /// </summary>
        public List<CustomLabel> CustomLabels { get; private set; } = new List<CustomLabel>();

        /// <summary>
        /// Gets the saved button model.
        /// </summary>
        public NextButton NextButton { get; private set; } = new NextButton();

        /// <summary>
        /// Gets the saved previous button model.
        /// </summary>
        public PreviousButton PreviousButton { get; private set; } = new PreviousButton();

        /// <summary>
        /// Gets the saved play pause button model.
        /// </summary>
        public PlayPauseButton PlayPauseButton { get; private set; } = new PlayPauseButton();

        /// <summary>
        /// Gets the saved progress bar model.
        /// </summary>
        public ProgressBar ProgressBar { get; private set; } = new ProgressBar();

        /// <summary>
        /// Gets the saved audio source settings.
        /// </summary>
        public List<AudioSourceSettings> AudioSourceSettings { get; } = new List<AudioSourceSettings>();

        public string CurrentProfile
        {
            get => _settings.CurrentProfileName;
            set
            {
                if (value == _settings.CurrentProfileName)
                {
                    return;
                }

                _settings.CurrentProfileName = value;
                SelectProfile(value);
            }
        }

        public List<string> Profiles => _settings.Profiles.Keys.ToList();

        public void CreateProfile(string profileName)
        {
            if (profileName == null)
            {
                throw new ArgumentNullException(nameof(profileName));
            }

            if (_settings.Profiles.ContainsKey(profileName))
            {
                throw new ArgumentException("Profile name already exists", nameof(profileName));
            }

            _settings.Profiles.Add(profileName, CreateProfileModel());
        }

        public void DeleteProfile(string profileName)
        {
            if (profileName == null)
            {
                throw new ArgumentNullException(nameof(profileName));
            }

            if (_settings.Profiles.Count == 1)
            {
                throw new InvalidOperationException("Must have at least one profile");
            }

            if (!_settings.Profiles.ContainsKey(profileName))
            {
                throw new ArgumentException($"Profile {profileName} does not exist", nameof(profileName));
            }

            _settings.Profiles.Remove(profileName);
        }

        public void RenameCurrentProfile(string newProfileName)
        {
            if (newProfileName == null)
            {
                throw new ArgumentNullException(nameof(newProfileName));
            }

            if (_currentProfile == null)
            {
                throw new InvalidOperationException("No profile selected. Current profile is null");
            }

            if (_settings.Profiles.ContainsKey(newProfileName))
            {
                throw new ArgumentException("Profile already exists", nameof(newProfileName));
            }

            _settings.Profiles.Remove(_settings.CurrentProfileName);
            _settings.CurrentProfileName = newProfileName;
            _settings.Profiles.Add(newProfileName, _currentProfile);
        }

        /// <summary>
        /// Save the settings.
        /// </summary>
        public void Save()
        {
            try
            {
                Toml.WriteFile(_settings, SettingsFilePath, _tomlSettings);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void LoadSettings()
        {
            try
            {
                var file = Toml.ReadFile(SettingsFilePath, _tomlSettings);
                var version = file["Version"].Get<string>();
                if (version != CurrentVersion)
                {
                    Toml.WriteFile(file, Path.Combine(SettingsDirectory, $"audioband.settings.{version}"), _tomlSettings);
                }

                _settings = Migration.MigrateSettings<SettingsV3>(file.Get(SettingsTable[version]), version, CurrentVersion);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error loading settings");
                throw;
            }
        }

        private void CreateDefault()
        {
            _settings = new SettingsV3()
            {
                AudioSource = null,
                AudioSourceSettings = new List<AudioSourceSettings>(),
                Profiles = new Dictionary<string, ProfileV3> { { SettingsV3.DefaultProfileName, CreateProfileModel() } }
            };
        }

        private ProfileV3 CreateProfileModel()
        {
            return new ProfileV3
            {
                AudioBandSettings = new AudioBand.Models.AudioBand(),
                AlbumArtSettings = new AlbumArt(),
                AlbumArtPopupSettings = new AlbumArtPopup(),
                PlayPauseButtonSettings = new PlayPauseButton(),
                NextButtonSettings = new NextButton(),
                PreviousButtonSettings = new PreviousButton(),
                ProgressBarSettings = new ProgressBar(),
                CustomLabelSettings = new List<CustomLabel> { new CustomLabel() }
            };
        }

        private void SelectProfile(string profileName)
        {
            Debug.Assert(_settings.Profiles.ContainsKey(profileName), $"Selecting non existent profile {profileName}");

            // Get new profile. Map to the app settings properties to trigger notification changes
            _currentProfile = _settings.Profiles[profileName];

            AlbumArtPopup = _currentProfile.AlbumArtPopupSettings;
            AlbumArt = _currentProfile.AlbumArtSettings;
            AudioBand = _currentProfile.AudioBandSettings;
            CustomLabels = _currentProfile.CustomLabelSettings;
            NextButton = _currentProfile.NextButtonSettings;
            PreviousButton = _currentProfile.PreviousButtonSettings;
            PlayPauseButton = _currentProfile.PlayPauseButtonSettings;
            ProgressBar = _currentProfile.ProgressBarSettings;

            ProfileChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}