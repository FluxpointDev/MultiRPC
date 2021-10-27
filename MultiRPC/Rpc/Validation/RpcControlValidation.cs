using System;
using Avalonia.Data;
using TinyUpdate.Core.Logging;

namespace MultiRPC.Rpc.Validation
{
    public class RpcControlValidation
    {
        private readonly ILogging _logging = LoggingCreator.CreateLogger(nameof(RpcControlValidation));
        
        private readonly Func<string, CheckResult>? _validation;
        public RpcControlValidation(Func<string, CheckResult>? validation, string? initialValue)
        {
            _validation = validation;
            _result = initialValue ?? string.Empty;
        }

        public Language Lang { get; init; } = null!;

        private string _result;
        public string Result
        {
            get => _result;
            set
            {
                var check = _validation?.Invoke(value);
                if (check?.Valid ?? true)
                {
                    _logging.Debug("Validation was successful");
                    _result = value;
                    ResultChanged?.Invoke(this, _result);
                    return;

                }

                var error = check?.ReasonWhy ?? "Unknown validation reason";
                _logging.Error(error);
                throw new DataValidationException(error);
            }
        }

        public event EventHandler<string>? ResultChanged;
    }
    
    public record CheckResult(bool Valid, string? ReasonWhy = null);
}