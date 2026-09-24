using System;
using UniRx;

namespace Aim.Clock
{
    public sealed class ClockController : IDisposable
    {
        private readonly ClockModel _model;
        private readonly ClockView _view;
        private readonly ClockConfig _config;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ClockController(ClockModel model, ClockView view, ClockConfig config)
        {
            _model = model;
            _view = view;
            _config = config;
        }

        public void Initialize()
        {
            _model.CurrentTime
                .Subscribe(time => _view.RenderTime(time, _config.DigitalTimeFormat))
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
