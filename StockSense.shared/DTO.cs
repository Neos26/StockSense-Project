namespace StockSense.shared
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
        public bool IsBlocked { get; set; }
    }

    public class CreateEmployeeDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee"; // Admin or Employee
    }


    public class CreatePreBuildDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CompatibleBrand { get; set; } = string.Empty;
        public string CompatibleModel { get; set; } = string.Empty;
        public string TargetCC { get; set; } = string.Empty;

        public int EstimatedAddedCC { get; set; }

        // Notice we only send the IDs of the products!
        public List<int> ProductIds { get; set; } = new();
    }


    public class UpdateServiceProductsDto
    {
        public int ServiceId { get; set; }
        public List<int> ProductIds { get; set; } = new();
    }
}
