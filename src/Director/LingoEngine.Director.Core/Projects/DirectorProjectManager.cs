using LingoEngine.Core;
using LingoEngine.IO;
using LingoEngine.Movies;
using LingoEngine.Projects;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.Core.Windowing;
using LingoEngine.Director.Core.Stages;
using AbstUI.Commands;
using LingoEngine.Director.Core.Projects.Commands;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using AbstUI.Windowing;

namespace LingoEngine.Director.Core.Projects;

/// <summary>
/// Handles project level operations such as saving and loading movies.
/// </summary>
public class DirectorProjectManager : IAbstCommandHandler<SaveDirProjectSettingsCommand>
{
    private readonly LingoProjectSettings _settings;
    private readonly LingoPlayer _player;
    private readonly JsonStateRepository _repo = new();
    private readonly IAbstWindowManager _windowManager;
    private readonly DirectorProjectSettings _dirSettings;
    private readonly DirectorStageGuides _guides;
    private readonly DirectorProjectSettingsRepository _settingsRepo;
    private readonly LingoProjectSettingsRepository _projectSettingsRepo;

    public DirectorProjectManager(
        LingoProjectSettings settings,
        LingoPlayer player,
        IAbstWindowManager windowManager,
        DirectorProjectSettings dirSettings,
        DirectorStageGuides guides,
        DirectorProjectSettingsRepository settingsRepo,
        LingoProjectSettingsRepository projectSettingsRepo)
    {
        _settings = settings;
        _player = player;
        _windowManager = windowManager;
        _dirSettings = dirSettings;
        _guides = guides;
        _settingsRepo = settingsRepo;
        _projectSettingsRepo = projectSettingsRepo;
    }

    public void SaveMovie()
    {
        if (!_settings.HasValidSettings)
        {
            _windowManager.OpenWindow(DirectorMenuCodes.ProjectSettingsWindow);
            return;
        }
        if (_player.ActiveMovie is not LingoMovie movie)
            return;

        Directory.CreateDirectory(_settings.ProjectFolder);
        var path = _settings.GetMoviePath(_settings.ProjectName);
        _repo.Save(path, movie);

        SaveDirectorSettings();
        SaveProjectSettings();
    }

    public void LoadMovie()
    {
        if (!_settings.HasValidSettings)
        {
            _windowManager.OpenWindow(DirectorMenuCodes.ProjectSettingsWindow);
            return;
        }
        var path = _settings.GetMoviePath(_settings.ProjectName);
        if (!File.Exists(path))
            return;

        var movie = _repo.Load(path, _player);
        _player.SetActiveMovie(movie);

        LoadProjectSettings();
        LoadDirectorSettings();
    }

    private void SaveDirectorSettings()
    {
        // Copy grid and guide settings
        _dirSettings.GridColor = _guides.GridColor;
        _dirSettings.GridVisible = _guides.GridVisible;
        _dirSettings.GridSnap = _guides.GridSnap;
        _dirSettings.GridWidth = _guides.GridWidth;
        _dirSettings.GridHeight = _guides.GridHeight;

        _dirSettings.GuidesColor = _guides.GuidesColor;
        _dirSettings.GuidesVisible = _guides.GuidesVisible;
        _dirSettings.GuidesSnap = _guides.GuidesSnap;
        _dirSettings.GuidesLocked = _guides.GuidesLocked;

        _dirSettings.VerticalGuides = _guides.VerticalGuides.ToList();
        _dirSettings.HorizontalGuides = _guides.HorizontalGuides.ToList();

        var states = new Dictionary<string, DirectorWindowState>();
        if (_windowManager is AbstWindowManager dm)
        {
            foreach (var (code, rect) in dm.GetRects())
                states[code] = new DirectorWindowState { X = (int)rect.Left, Y = (int)rect.Top, Width = (int)rect.Width, Height = (int)rect.Height };
        }

        _dirSettings.WindowStates = states;

        var settingsPath = Path.Combine(_settings.ProjectFolder, _settings.ProjectName + ".director.json");
        _settingsRepo.Save(settingsPath, _dirSettings);
    }

    private void LoadDirectorSettings()
    {
        var settingsPath = Path.Combine(_settings.ProjectFolder, _settings.ProjectName + ".director.json");
        var loaded = _settingsRepo.Load(settingsPath);

        // Copy into runtime settings object
        _dirSettings.GuidesColor = loaded.GuidesColor;
        _dirSettings.GuidesVisible = loaded.GuidesVisible;
        _dirSettings.GuidesSnap = loaded.GuidesSnap;
        _dirSettings.GuidesLocked = loaded.GuidesLocked;
        _dirSettings.VerticalGuides = loaded.VerticalGuides.ToList();
        _dirSettings.HorizontalGuides = loaded.HorizontalGuides.ToList();

        _dirSettings.GridColor = loaded.GridColor;
        _dirSettings.GridVisible = loaded.GridVisible;
        _dirSettings.GridSnap = loaded.GridSnap;
        _dirSettings.GridWidth = loaded.GridWidth;
        _dirSettings.GridHeight = loaded.GridHeight;

        _dirSettings.WindowStates = loaded.WindowStates;

        // Apply to guides object
        _guides.GridColor = _dirSettings.GridColor;
        _guides.GridVisible = _dirSettings.GridVisible;
        _guides.GridSnap = _dirSettings.GridSnap;
        _guides.GridWidth = _dirSettings.GridWidth;
        _guides.GridHeight = _dirSettings.GridHeight;

        _guides.GuidesColor = _dirSettings.GuidesColor;
        _guides.GuidesVisible = _dirSettings.GuidesVisible;
        _guides.GuidesSnap = _dirSettings.GuidesSnap;
        _guides.GuidesLocked = _dirSettings.GuidesLocked;

        _guides.VerticalGuides.Clear();
        foreach (var v in _dirSettings.VerticalGuides)
            _guides.VerticalGuides.Add(v);
        _guides.HorizontalGuides.Clear();
        foreach (var h in _dirSettings.HorizontalGuides)
            _guides.HorizontalGuides.Add(h);
        _guides.Draw();

        // Apply window states
        if (_windowManager is AbstWindowManager dm)
        {
            foreach (var window in dm.GetWindows())
            {
                if (!_dirSettings.WindowStates.TryGetValue(window.WindowCode, out var st))
                    continue;

                window.SetPositionAndSize(st.X, st.Y, st.Width, st.Height);
            }
        }
    }

    private void SaveProjectSettings()
    {
        var settingsPath = Path.Combine(_settings.ProjectFolder, _settings.ProjectName + ".lingo.json");
        _projectSettingsRepo.Save(settingsPath, _settings);
    }

    private void LoadProjectSettings()
    {
        var settingsPath = Path.Combine(_settings.ProjectFolder, _settings.ProjectName + ".lingo.json");
        var loaded = _projectSettingsRepo.Load(settingsPath);
        _settings.CodeFolder = loaded.CodeFolder;
        _settings.MaxSpriteChannelCount = loaded.MaxSpriteChannelCount;
    }

    public bool CanExecute(SaveDirProjectSettingsCommand command) => true;

    public bool Handle(SaveDirProjectSettingsCommand command)
    {
        var dirPath = Path.Combine(command.ProjectSettings.ProjectFolder, command.ProjectSettings.ProjectName + ".director.json");
        var projPath = Path.Combine(command.ProjectSettings.ProjectFolder, command.ProjectSettings.ProjectName + ".lingo.json");

        _settingsRepo.Save(dirPath, command.DirSettings);
        _projectSettingsRepo.Save(projPath, command.ProjectSettings);
        return true;
    }
}
