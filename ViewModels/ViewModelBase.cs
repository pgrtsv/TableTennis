using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;

namespace TableTennis.ViewModels
{
    public class ViewModelBase : ReactiveObject, IActivatableViewModel
    {
        public ViewModelActivator Activator { get; } = new();
    }
}
