using System;
using System.Collections.Generic;

namespace VideoService.Infrastructure.Persistence.Entities;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Video> Videos { get; set; } = new List<Video>();
}
