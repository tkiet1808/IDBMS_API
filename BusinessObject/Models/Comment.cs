﻿using System.ComponentModel.DataAnnotations;

namespace BusinessObject.Models;

public class Comment
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public CommentType Type { get; set; }

    public string? Content { get; set; } = default!;

    public string? FileUrl { get; set; }

    public Guid? ItemId { get; set; }

    public Guid? ConstructionTaskId { get; set; }
    public ConstructionTask? ConstructionTask { get; set; }

    public Guid? DecorProgressReportId { get; set; }
    public DecorProgressReport? DecorProgressReport { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } = new();

    [Required]
    public DateTime CreatedTime { get; set; }

    public DateTime? LastModifiedTime { get; set; }

    [Required]
    public bool IsDeleted { get; set; }
}
