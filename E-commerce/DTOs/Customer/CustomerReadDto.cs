namespace E_commerce.DTOs.Customer
{
    public class CustomerReadDto
    {
        public int CustomerId { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Phone { get; set; }

        public string Address { get; set; } = null!;

        public string City { get; set; } = null!;

        public int Points { get; set; }

        public string? UserId { get; set; }
    }
}
