using LingoEngine.Commands;
using LingoEngine.Movies.Commands;

namespace LingoEngine.Movies
{
    public interface ILingoFrameLabelManager
    {
        event Action? LabelsChanged;
        IReadOnlyDictionary<string, int> GetScoreLabels();

        void SetScoreLabel(int frameNumber, string? name);
        int GetNextLabelFrame(int frame);
        int GetPreviousLabelFrame(int frame);
    }

    /// <summary>
    /// Handles frame related data such as score labels and frame specific behaviours.
    /// </summary>
    internal class LingoFrameLabelManager : ILingoFrameLabelManager,
        ICommandHandler<DeleteFrameLabelCommand>,
        ICommandHandler<SetFrameLabelCommand>,
        ICommandHandler<AddFrameLabelCommand>,
        ICommandHandler<UpdateFrameLabelCommand>
    {
        private readonly Dictionary<string, int> _scoreLabels = new();

        public event Action? LabelsChanged;
        public LingoFrameLabelManager()
        {
        }

        internal IReadOnlyDictionary<int, string> MarkerList =>
            _scoreLabels.ToDictionary(kv => kv.Value, kv => kv.Key);

        public IReadOnlyDictionary<string, int> ScoreLabels => _scoreLabels;


        public void SetScoreLabel(int frameNumber, string? name)
        {
            string? existingLabel = null;
            foreach (var item in _scoreLabels)
            {
                if (item.Value == frameNumber)
                {
                    existingLabel = item.Key;
                    break;
                }
            }
            if (existingLabel != null)
                _scoreLabels.Remove(existingLabel);
            if (!string.IsNullOrEmpty(name))
                _scoreLabels[name] = frameNumber;
            LabelsChanged?.Invoke();
        }

        public bool DeleteLabel(int frameNumber)
        {
            string? existingLabel = null;
            foreach (var item in _scoreLabels)
            {
                if (item.Value == frameNumber)
                {
                    existingLabel = item.Key;
                    break;
                }
            }
            if (existingLabel == null) return false;
            _scoreLabels.Remove(existingLabel);
            return true;
        }


        public IReadOnlyDictionary<string, int> GetScoreLabels() => _scoreLabels;

        public int GetNextLabelFrame(int frame)
        {
            var next = _scoreLabels.Values
                .Where(v => v > frame)
                .DefaultIfEmpty(int.MaxValue)
                .Min();
            if (next == int.MaxValue)
                return frame + 10;
            return next;
        }
        public int GetPreviousLabelFrame(int frame)
        {
            var previous = _scoreLabels.Values
                .Where(v => v < frame)
                .DefaultIfEmpty(int.MaxValue)
                .Max();
            if (previous == int.MaxValue)
                return frame - 10;
            return previous;
        }



        public int GetNextMarker(int frame)
        {
            if (_scoreLabels.Count == 0)
                return 1;
            var next = _scoreLabels.Values.Where(v => v > frame).DefaultIfEmpty(_scoreLabels.Values.Max()).Min();
            return next;
        }

        public int GetPreviousMarker(int frame)
        {
            if (_scoreLabels.Count == 0)
                return 1;
            var markers = _scoreLabels.Values.OrderBy(v => v).ToList();
            bool currentIsMarker = markers.Contains(frame);
            int target;
            if (currentIsMarker)
            {
                int idx = markers.IndexOf(frame);
                target = idx > 0 ? markers[idx - 1] : frame;
            }
            else
            {
                int prev = markers.Where(v => v < frame).DefaultIfEmpty(0).Max();
                if (prev == 0)
                {
                    int right = markers.Where(v => v > frame).DefaultIfEmpty(1).Min();
                    target = right;
                }
                else
                {
                    int idx = markers.IndexOf(prev);
                    target = idx > 0 ? markers[idx - 1] : prev;
                }
            }
            return target;
        }

        internal int GetLoopMarker(int frame)
        {
            if (_scoreLabels.Count == 0)
                return 1;
            var markers = _scoreLabels.Values.OrderBy(v => v).ToList();
            int prev = markers.Where(v => v < frame).DefaultIfEmpty(0).Max();
            if (prev > 0)
            {
                return prev;
            }
            else
            {
                if (markers.Contains(frame))
                    return frame;
                else
                {
                    int right = markers.Where(v => v > frame).DefaultIfEmpty(1).Min();
                    return right;
                }
            }
        }


        public bool CanExecute(SetFrameLabelCommand command) => true;

        public bool Handle(SetFrameLabelCommand command)
        {
            SetScoreLabel(command.FrameNumber, command.Name);
            return true;
        }

        public bool CanExecute(AddFrameLabelCommand command) => true;

        public bool Handle(AddFrameLabelCommand command)
        {
            SetScoreLabel(command.FrameNumber, command.Name);
            return true;
        }

        public bool CanExecute(UpdateFrameLabelCommand command) => true;

        public bool Handle(UpdateFrameLabelCommand command)
        {
            SetScoreLabel(command.PreviousFrame, null);
            SetScoreLabel(command.NewFrame, command.Name);
            return true;
        }
        public bool CanExecute(DeleteFrameLabelCommand command) => true;

        public bool Handle(DeleteFrameLabelCommand command)
        {
            return DeleteLabel(command.Frame);
        }


    }
}
