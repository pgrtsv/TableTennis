using System.Diagnostics.CodeAnalysis;

namespace TableTennis.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public abstract class ChildViewModelBase : ViewModelBase
    {
        public abstract string Name { get; }
    }
}