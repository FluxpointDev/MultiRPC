using System;
using Avalonia.Data;
using MultiRPC.UI.Pages.Rpc;

namespace MultiRPC.Rpc
{
    public class RpcControlDataContext
    {
        private readonly Func<string, CheckResult>? _validation;
        public RpcControlDataContext(Func<string, CheckResult>? validation)
        {
            _validation = validation;
        }

        public Language Lang { get; init; } = null!;

        private string _result = string.Empty;
        public string Result
        {
            get => _result;
            set
            {
                var check = _validation?.Invoke(value);
                if (check?.Valid ?? true)
                {
                    _result = value;
                    ResultChanged?.Invoke(this, _result);
                    return;
                }

                throw new DataValidationException(check?.ReasonWhy ?? "Unknown validation reason");
            }
        }

        public event EventHandler<string>? ResultChanged;
    }
    
    public record CheckResult(bool Valid, string? ReasonWhy = null);
}