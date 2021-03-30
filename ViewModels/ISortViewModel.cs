namespace TableTennis.ViewModels
{
    public interface ISortViewModel
    {
        string Name { get; }
        bool IsDescending { get; set; }
    }
}