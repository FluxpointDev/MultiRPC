using Avalonia.Data;
using TinyUpdate.Core.Logging;

namespace MultiRPC;

public class ControlValidation
{
    private readonly ILogging _logging = LoggingCreator.CreateLogger(nameof(ControlValidation));
        
    private readonly Func<string, CheckResult>? _validation;
    public ControlValidation(Func<string, CheckResult>? validation, string? initialValue)
    {
        _validation = validation;
        _result = initialValue ?? string.Empty;
    }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public Language? Lang { get; init; }

    public event EventHandler<bool>? ResultStatusChanged;

    private bool _lastResultStatus = true;
    public bool LastResultStatus
    {
        get => _lastResultStatus;
        private set
        {
            if (_lastResultStatus != value)
            {
                _lastResultStatus = value;
                ResultStatusChanged?.Invoke(this, value);
            }
        }
    }

    private string _result;
    public string Result
    {
        get => _result;
        set
        {
            var check = _validation?.Invoke(value);
            LastResultStatus = check?.Valid ?? true;
            if (LastResultStatus)
            {
                _logging.Debug("Validation was successful");
                _result = value;
                ResultChanged?.Invoke(this, _result);
                return;
            }

            var error = check?.ReasonWhy ?? Language.GetText(LanguageText.UnknownValidationReason);
            _logging.Error(error);
            throw new DataValidationException(error);
        }
    }

    public event EventHandler<string>? ResultChanged;
}

public record CheckResult(bool Valid, string? ReasonWhy = null);