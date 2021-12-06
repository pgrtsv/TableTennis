using ReactiveUI.Fody.Helpers;

namespace TableTennis.ViewModels
{
    public class SettingsViewModel : ChildViewModelBase
    {
        [Reactive] public bool IsRubetsEnabled { get; set; } = false;
        
        public override string Name { get; } = "Настройки";
    }
}