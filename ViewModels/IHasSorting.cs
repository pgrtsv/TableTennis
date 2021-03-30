using System.Collections.Generic;

namespace TableTennis.ViewModels
{
    public interface IHasSorting
    {
        IEnumerable<ISortViewModel> SortViewModels { get; }
        
        ISortViewModel SelectedSortViewModel { get; set; }
    }
}