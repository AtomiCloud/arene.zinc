namespace Domain.Projects;

// Project
public record ProjectSearch
{
  public Guid? Id { get; init; }
  public string? Name { get; init; }
  public int Limit { get; init; }
  public int Skip { get; init; }
}

public record Project
{
  public required ProjectPrincipal Principal { get; init; }

  public required uint SubscriberCount { get; init; }
}

public record ProjectPrincipal
{
  public required Guid Id { get; init; }
  public required ProjectRecord Record { get; init; }
}

public record ProjectRecord
{
  public required string Name { get; init; }
  public required bool Open { get; init; }
}
