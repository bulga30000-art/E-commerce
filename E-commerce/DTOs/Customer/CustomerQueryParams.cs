namespace E_commerce.DTOs.Customer
{
    // Query parameters for paginated search and sorting of customer records
    public class CustomerQueryParams
    {
        private const int MaxPageSize = 50;

        private int _pageSize = 10;
        private int _pageNumber = 1;

        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 1 : Math.Min(value, MaxPageSize);
        }

        // Search term covering FirstName, LastName, Phone, City, and Address
        public string? SearchTerm { get; set; }

        public bool SortDescending { get; set; } = false;
    }
}