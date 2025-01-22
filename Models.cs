namespace EF9Bug;

public class OptimisedModel : IOwnedBy
{
    public string? OwnerId { get; set; }
    public string? OwnerType { get; set; }
    public string? Category { get; set; }
    public OptionalOneOptimised? Optional { get; set; }
    public OptionalTwoOptimised? Optional2 { get; set; }

    public IEnumerable<DetailOptimised> Details { get; set; }
}

public interface IOwnedBy
{
    string? OwnerId { get; }
    string? OwnerType { get; }
}

public class OptionalOneOptimised
{
    public string Name { get; set; }
}

public class OptionalTwoOptimised
{
    public string Name { get; set; }
}

public class DetailOptimised
{
    public string Name { get; set; }

    public int Count { get; set; }
}
