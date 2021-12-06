using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls.Primitives;
using ReactiveUI;
using TableTennis.ViewModels;

namespace TableTennis
{
    public static class BindingEx
    {
        public static IDisposable BindSorting<TView, TViewModel>(
            this TView view,
            TViewModel viewModel,
            Func<TView, ToggleButton> toggleButton,
            Func<TViewModel, ISortViewModel> sortViewModel
        ) where TView : IViewFor<TViewModel> where TViewModel : class, IHasSorting
        {
            return new CompositeDisposable(
                viewModel.WhenAnyValue(x => x.SelectedSortViewModel)
                    .Subscribe(selectedSortViewModel =>
                    {
                        toggleButton.Invoke(view).IsChecked = selectedSortViewModel == sortViewModel.Invoke(viewModel)
                            ? selectedSortViewModel.IsDescending
                            : null;
                    }),
                sortViewModel.Invoke(viewModel).WhenAnyValue(x => x.IsDescending)
                    .Subscribe(isDescending =>
                    {
                        if (toggleButton.Invoke(view).IsChecked != null)
                            toggleButton.Invoke(view).IsChecked = isDescending;
                    }),
                toggleButton.Invoke(view).Events().Checked
                    .Subscribe(_ =>
                    {
                        viewModel.SelectedSortViewModel = sortViewModel.Invoke(viewModel);
                        sortViewModel.Invoke(viewModel).IsDescending = true;
                    }),
                toggleButton.Invoke(view).Events().Unchecked
                    .Subscribe(_ =>
                    {
                        viewModel.SelectedSortViewModel = sortViewModel.Invoke(viewModel);
                        sortViewModel.Invoke(viewModel).IsDescending = false;
                    })
                );
        }
    }
}