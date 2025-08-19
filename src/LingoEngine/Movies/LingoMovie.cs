using LingoEngine.Casts;
using LingoEngine.Core;
using AbstUI.Core;
using LingoEngine.Events;
using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Sounds;
using LingoEngine.Sprites;
using LingoEngine.Stages;
using LingoEngine.Projects;
using LingoEngine.Transitions;
using LingoEngine.Tempos;
using LingoEngine.ColorPalettes;
using LingoEngine.Scripts;

namespace LingoEngine.Movies
{


    public class LingoMovie : ILingoMovie, ILingoClockListener, IDisposable
    {
        private readonly ILingoMemberFactory _memberFactory;
        private ILingoFrameworkMovie _FrameworkMovie;
        private readonly LingoMovieEnvironment _environment;
        private readonly Action<LingoMovie> _onRemoveMe;
        private readonly LingoStageMouse _lingoMouse;
        private readonly LingoClock _lingoClock;
        private int _currentFrame = 0;
        private int _NextFrame = -1;
        private int _lastFrame = 0;
        private bool _isPlaying = false;

        private bool _needToRaiseStartMovie = false;
        private LingoCastLibsContainer _castLibContainer;
        private readonly LingoFrameLabelManager _frameLabelManager;
        private readonly LingoSprite2DManager _sprite2DManager;
        private bool _IsManualUpdateStage;
        public event Action<int>? Sprite2DListChanged { add => _sprite2DManager.SpriteListChanged += value; remove => _sprite2DManager.SpriteListChanged -= value; }

        private readonly LingoSpriteAudioManager _audioManager;
        private readonly LingoSpriteTransitionManager _transitionManager;
        private readonly LingoTempoSpriteManager _tempoManager;
        private readonly LingoSpriteColorPaletteSpriteManager _paletteManager;
        private readonly LingoFrameScriptSpriteManager _frameScriptManager;

        // Movie Script subscriptions
        private readonly ActorList _actorList = new ActorList();
        private readonly LingoMovieScriptContainer _MovieScripts;
        private readonly List<LingoSpriteManager> _spriteManagers = new();


        #region Properties

        public ILingoFrameworkMovie FrameworkObj => _FrameworkMovie;
        public T Framework<T>() where T : class, ILingoFrameworkMovie => (T)_FrameworkMovie;

        public ILingoSpriteAudioManager Audio => _audioManager;
        public ILingoSpriteTransitionManager Transitions => _transitionManager;
        public ILingoTempoSpriteManager Tempos => _tempoManager;
        public ILingoSpriteColorPaletteSpriteManager ColorPalettes => _paletteManager;
        public ILingoFrameScriptSpriteManager FrameScripts => _frameScriptManager;
        public LingoSprite2DManager Sprite2DManager => _sprite2DManager;
        public ILingoFrameLabelManager FrameLabels => _frameLabelManager;

        public string Name { get; set; }

        public int Number { get; private set; }


        public string About { get; set; } = string.Empty;
        public string Copyright { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        private readonly LingoEventMediator _EventMediator;

        public int Frame => _currentFrame;
        public int CurrentFrame => _currentFrame;
        public int FrameCount => 620;
        public int Timer { get; private set; }
        public int SpriteTotalCount => _sprite2DManager.SpriteTotalCount;
        public int SpriteMaxNumber => _sprite2DManager.SpriteMaxNumber;
        public int LastChannel => _sprite2DManager.MaxSpriteChannelCount;
        public int LastFrame => FrameCount;
        public IReadOnlyDictionary<int, string> MarkerList =>
            _frameLabelManager.MarkerList;
        // Tempo (Frame Rate)
        public int Tempo
        {
            get => _tempoManager.Tempo;
            set => _tempoManager.ChangeTempo(value);
        }
        public int MaxSpriteChannelCount
        {
            get => _sprite2DManager.MaxSpriteChannelCount;
            set => _sprite2DManager.MaxSpriteChannelCount = value;
        }
        public bool IsPlaying => _isPlaying;

        public event Action<bool>? PlayStateChanged;
        public event Action<int>? CurrentFrameChanged;

        public ActorList ActorList => _actorList;
        public LingoTimeOutList TimeOutList { get; private set; } = new LingoTimeOutList();

        #endregion


#pragma warning disable CS8618
        protected internal LingoMovie(LingoMovieEnvironment environment, LingoStage movieStage, LingoCastLibsContainer castLibContainer, ILingoMemberFactory memberFactory, string name, int number, LingoEventMediator mediator, Action<LingoMovie> onRemoveMe, LingoProjectSettings projectSettings, ILingoFrameLabelManager lingoFrameLabelManager)
#pragma warning restore CS8618
        {
            _castLibContainer = castLibContainer;
            _environment = environment;
            _memberFactory = memberFactory;
            _environment = environment;
            _onRemoveMe = onRemoveMe;
            Name = name;
            Number = number;
            _EventMediator = mediator;
            _MovieScripts = new(environment, mediator);
            _lingoMouse = (LingoStageMouse)environment.Mouse;
            _lingoClock = (LingoClock)environment.Clock;

            _sprite2DManager = new LingoSprite2DManager(this, environment);
            MaxSpriteChannelCount = projectSettings.MaxSpriteChannelCount;
            _frameLabelManager = (LingoFrameLabelManager)lingoFrameLabelManager;
            _audioManager = new LingoSpriteAudioManager(this, environment);
            _transitionManager = new LingoSpriteTransitionManager(this, environment);
            _tempoManager = new LingoTempoSpriteManager(this, environment);
            _paletteManager = new LingoSpriteColorPaletteSpriteManager(this, environment);
            _frameScriptManager = new LingoFrameScriptSpriteManager(this, environment);

            _spriteManagers.Add(_tempoManager);
            _spriteManagers.Add(_paletteManager);
            _spriteManagers.Add(_transitionManager);
            _spriteManagers.Add(_audioManager);
            _spriteManagers.Add(_frameScriptManager);
        }
        public void Init(ILingoFrameworkMovie frameworkMovie)
        {
            _FrameworkMovie = frameworkMovie;
        }
        public void Dispose()
        {
            RemoveMe();
        }
        public void RemoveMe()
        {
            Hide();
            _onRemoveMe(this);
        }

        internal void Show()
        {
            _lingoClock.Subscribe(this);
        }
        internal void Hide()
        {
            _lingoClock.Unsubscribe(this);
        }






        #region Sprites
        public ILingoSpriteChannel Channel(int channelNumber) => _sprite2DManager.Channel(channelNumber);
        public void PuppetSprite(int number, bool isPuppetSprite) => Channel(number).Puppet = isPuppetSprite;
        public ILingoSpriteChannel GetActiveSprite(int number) => _sprite2DManager.GetActiveSprite(number);
        public LingoSprite2D AddSprite(string name, Action<LingoSprite2D>? configure = null) => _sprite2DManager.AddSprite(name, configure);
        public LingoSprite2D AddSprite(int num, Action<LingoSprite2D>? configure = null) => _sprite2DManager.AddSprite(num, configure);
        public LingoFrameScriptSprite AddFrameBehavior<TBehaviour>(int frameNumber, Action<TBehaviour>? configureBehaviour = null, Action<LingoFrameScriptSprite>? configure = null) where TBehaviour : LingoSpriteBehavior
            => _frameScriptManager.Add(frameNumber, configureBehaviour, configure);
        public LingoSprite2D AddSprite(int num, string name, Action<LingoSprite2D>? configure = null) => _sprite2DManager.AddSprite(num, name, configure);
        public LingoSprite? AddSpriteByChannelNum(int spriteNumWithChannel, int begin, int end, ILingoMember? member)
        {
            if (spriteNumWithChannel < _spriteManagers.Count)
            {
                var sprite = _spriteManagers[spriteNumWithChannel].Add(spriteNumWithChannel, begin, end, member);
                return sprite;
            }
            var sprite2D = _sprite2DManager.Add(spriteNumWithChannel, begin, end, member);
            return sprite2D;
        }
        public LingoSprite2D AddSprite(int num, int begin, int end, float x, float y, Action<LingoSprite2D>? configure = null)
            => _sprite2DManager.AddSprite(num, begin, end, x, y, configure);
        public bool RemoveSprite(string name) => _sprite2DManager.RemoveSprite(name);
        public bool RemoveSprite(LingoSprite2D sprite) => _sprite2DManager.RemoveSprite(sprite);
        public bool TryGetAllTimeSprite(string name, out LingoSprite2D? sprite) => _sprite2DManager.TryGetAllTimeSprite(name, out sprite);
        public bool TryGetAllTimeSprite(int number, out LingoSprite2D? sprite) => _sprite2DManager.TryGetAllTimeSprite(number, out sprite);
        public void SetSpriteMember(int number, string memberName) => _sprite2DManager.SetSpriteMember(number, memberName);
        public void SetSpriteMember(int number, int memberNumber) => _sprite2DManager.SetSpriteMember(number, memberNumber);
        public void SendSprite<T>(int spriteNumber, Action<T> actionOnSpriteBehaviour) where T : LingoSpriteBehavior => _sprite2DManager.SendSprite(spriteNumber, actionOnSpriteBehaviour);
        public TResult? SendSprite<T, TResult>(int spriteNumber, Func<T, TResult> actionOnSpriteBehaviour) where T : LingoSpriteBehavior => _sprite2DManager.SendSprite<T, TResult>(spriteNumber, actionOnSpriteBehaviour);
        public void SendSprite(string name, Action<ILingoSpriteChannel> actionOnSprite) => _sprite2DManager.SendSprite(name, actionOnSprite);
        public void SendSprite(int spriteNumber, Action<ILingoSpriteChannel> actionOnSprite) => _sprite2DManager.SendSprite(spriteNumber, actionOnSprite);
        public void SendAllSprites(Action<ILingoSpriteChannel> actionOnSprite) => _sprite2DManager.SendAllSprites(actionOnSprite);
        public void SendAllSprites<T>(Action<T> actionOnSprite) where T : LingoSpriteBehavior => _sprite2DManager.SendAllSprites(actionOnSprite);
        public IEnumerable<TResult?> SendAllSprites<T, TResult>(Func<T, TResult> actionOnSprite) where T : LingoSpriteBehavior => _sprite2DManager.SendAllSprites<T, TResult>(actionOnSprite);
        public bool RollOver(int spriteNumber) => _sprite2DManager.RollOver(spriteNumber);
        public int RollOver() => _sprite2DManager.RollOver();
        public int ConstrainH(int spriteNumber, int pos) => _sprite2DManager.ConstrainH(spriteNumber, pos);
        public int ConstrainV(int spriteNumber, int pos) => _sprite2DManager.ConstrainV(spriteNumber, pos);
        public LingoSprite2D? GetSpriteUnderMouse(bool skipLockedSprites = false) => _sprite2DManager.GetSpriteUnderMouse(skipLockedSprites);
        public IEnumerable<LingoSprite2D> GetSpritesAtPoint(float x, float y, bool skipLockedSprites = false) => _sprite2DManager.GetSpritesAtPoint(x, y, skipLockedSprites);
        public LingoSprite2D? GetSpriteAtPoint(float x, float y, bool skipLockedSprites = false) => _sprite2DManager.GetSpriteAtPoint(x, y, skipLockedSprites);
        public void ChangeSpriteChannel(LingoSprite sprite, int newChannel)
        {
            if (sprite is LingoSprite2D sprite2D)
                _sprite2DManager.ChangeSpriteChannel(sprite2D, newChannel);
        }

        #endregion



        #region Playhead

        public void GoTo(string label) => Go(label);
        public void Go(string label)
        {
            if (_frameLabelManager.ScoreLabels.TryGetValue(label, out var scoreLabel))
                _NextFrame = scoreLabel;
        }

        public void GoTo(int frame) => Go(frame);

        public void Go(int frame)
        {
            if (frame <= 0)
                throw new ArgumentOutOfRangeException(nameof(frame));
            _NextFrame = frame;
        }

        public void OnTick()
        {
            if (_isPlaying)
            {
                if (_waitingForInput || _waitingForCuePoint)
                    return;

                if (_delayTicks > 0)
                {
                    _delayTicks--;
                    return;
                }
                if (_IsManualUpdateStage)
                    OnUpdateStage();
                else
                    AdvanceFrame();
            }
        }
        private bool _isAdvancing;
        public void AdvanceFrame()
        {
            if (_isAdvancing) return;
            _isAdvancing = true;

            try
            {
                var frameChanged = false;
                if (_NextFrame < 0)
                {
                    frameChanged = true;
                    _currentFrame++;
                }
                else
                {
                    frameChanged = _currentFrame != _NextFrame;
                    _currentFrame = _NextFrame;
                    _NextFrame = -1;
                }

                if (frameChanged)
                {
                    // update the list with all ended, and all the new started sprites.
                    _sprite2DManager.UpdateActiveSprites(_currentFrame, _lastFrame);
                    _spriteManagers.ForEach(x => x.UpdateActiveSprites(_currentFrame, _lastFrame));

                    // End the sprites first, the frame has change, start by ending all sprites, that are not on this frame anymore.
                    _sprite2DManager.EndSprites();
                    _spriteManagers.ForEach(x => x.EndSprites());

                    // Begin the new sprites
                    _sprite2DManager.BeginSprites();
                    _spriteManagers.ForEach(x => x.BeginSprites());
                }
                _lastFrame = _currentFrame;

                if (_needToRaiseStartMovie)
                    _EventMediator.RaiseStartMovie();

                _lingoMouse.UpdateMouseState();
                _sprite2DManager.PreStepFrame();
                _EventMediator.RaiseStepFrame();
                _EventMediator.RaisePrepareFrame();
                _EventMediator.RaiseEnterFrame();

                OnUpdateStage();
                if (frameChanged)
                    CurrentFrameChanged?.Invoke(_currentFrame);

                _EventMediator.RaiseExitFrame();
            }
            finally
            {
                //_sprite2DManager.EndSprites();
                //_spriteManagers.ForEach(x => x.EndSprites());
                _isAdvancing = false;
            }

        }



        // Play the movie
        public void Play()
        {
            _EventMediator.RaisePrepareMovie();
            _needToRaiseStartMovie = true;
            // prepareMovie
            // PrepareFrame
            // BeginSprite
            // StartMovie
            _isPlaying = true;
            PlayStateChanged?.Invoke(true);
            //OnTick();
            _needToRaiseStartMovie = false;

        }

        private void OnStop()
        {
            _isPlaying = false;
            PlayStateChanged?.Invoke(false);
            _environment.Sound.StopAll();
            //_spriteManager.EndSprites();
            _EventMediator.RaiseStopMovie();
            // EndSprite
            // StopMovie
        }
        // Halt the movie
        public void Halt()
        {
            OnStop();
        }
        public void NextFrame()
        {
            if (_isPlaying)
            {
                if (Frame < FrameCount)
                    Go(Frame + 1);
            }
        }

        public void PrevFrame()
        {
            if (_isPlaying)
            {
                if (Frame > 1)
                    Go(Frame - 1);
            }
        }

        private int _delayTicks;
        private bool _waitingForInput;
        private bool _waitingForCuePoint;
        private int _waitCueChannel;
        private int _waitCuePoint;
        public void Delay(int ticks)
        {
            if (ticks <= 0) return;
            _delayTicks += ticks;
        }

        public void WaitForInput()
        {
            _waitingForInput = true;
        }

        public void ContinueAfterInput()
        {
            _waitingForInput = false;
        }

        public void WaitForCuePoint(int channel, int point)
        {
            _waitingForCuePoint = true;
            _waitCueChannel = channel;
            _waitCuePoint = point;
        }

        public void CuePointReached(int channel, int point)
        {
            if (_waitingForCuePoint && channel == _waitCueChannel && point == _waitCuePoint)
                _waitingForCuePoint = false;
        }

        public void GoNext()
            => Go(_frameLabelManager.GetNextMarker(Frame));

        public void GoPrevious()
            => Go(_frameLabelManager.GetPreviousMarker(Frame));

        public void GoLoop()
            => Go(_frameLabelManager.GetLoopMarker(Frame));

        public void InsertFrame()
        {
            // TODO: Implement score recording frame duplication
        }

        public void DeleteFrame()
        {
            // TODO: Implement frame deletion during score recording
        }

        public void UpdateFrame()
        {
            // TODO: Finalize changes for current frame during recording
        }
        // Go to a specific frame and stop
        public void GoToAndStop(int frame)
        {
            if (frame >= 1 && frame <= FrameCount)
            {
                // Jump directly to the requested frame while ensuring sprite
                // lifecycle events are fired. The existing AdvanceFrame logic
                // already handles begin/end sprite events when the playhead
                // moves to a new frame, so reuse it by setting the next frame
                // and manually advancing once.
                _NextFrame = frame;
                AdvanceFrame();
                _isPlaying = false;
                PlayStateChanged?.Invoke(false);
                _environment.Sound.StopAll();
            }
        }


        public void UpdateStage()
        {
            // a manual update stage needs to run on same framerate, it means that the head player will not advance
            _IsManualUpdateStage = true;
        }
        private void OnUpdateStage()
        {

            Timer++;
            _actorList.Invoke();
            _FrameworkMovie.UpdateStage();
            _IsManualUpdateStage = false;
        }
        #endregion



        // PuppetTransition (for special effects/animations, implementation is up to you)
        public void PuppetTransition(int effectNumber)
        {
            // Implement specific logic for puppet transition effects (if any)
        }


        #region CastLibs
        public ILingoCastLibsContainer CastLib => _castLibContainer;
        public ILingoMembersContainer Member => _castLibContainer.Member;
        public T? GetMember<T>(int number) where T : class, ILingoMember => _castLibContainer.GetMember<T>(number);
        public T? GetMember<T>(string name) where T : class, ILingoMember => _castLibContainer.GetMember<T>(name);
        #endregion


        #region MovieScripts
        public ILingoMovie AddMovieScript<T>()
            where T : LingoMovieScript
        {
            _MovieScripts.Add<T>();
            return this;
        }
        public void CallMovieScript<T>(Action<T> action) where T : LingoMovieScript
            => _MovieScripts.Call(action);
        public TResult? CallMovieScript<T, TResult>(Func<T, TResult> action) where T : LingoMovieScript
            => _MovieScripts.Call(action);

        private void CallOnAllMovieScripts(Action<LingoMovieScript> actionOnAll)
            => _MovieScripts.CallAll(actionOnAll);

        #endregion



        public LingoMovieEnvironment GetEnvironment() => _environment;
        public IAbstServiceProvider GetServiceProvider() => _environment.GetServiceProvider();

        public void StartTimer() => Timer = 0;

        public void SetScoreLabel(int frameNumber, string? name)
            => _frameLabelManager.SetScoreLabel(frameNumber, name);

        public int GetNextSpriteStart(int channel, int frame)
            => _sprite2DManager.GetNextSpriteStart(channel, frame);

        public int GetPrevSpriteEnd(int channel, int frame)
            => _sprite2DManager.GetPrevSpriteEnd(channel, frame);


        public int GetMaxLocZ() => _sprite2DManager.GetMaxLocZ();

        public ILingoMemberFactory New => _memberFactory;

        public LingoMember? MouseMemberUnderMouse() // todo : implement
            => null;


    }
}
