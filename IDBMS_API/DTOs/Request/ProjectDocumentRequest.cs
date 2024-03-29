﻿using BusinessObject.Enums;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDBMS_API.DTOs.Request
{
    public class ProjectDocumentRequest
    {
        [Required]
        public string Name { get; set; } = default!;

        public string? Description { get; set; }

        public IFormFile? file { get; set; } = default!;

        [Required]
        public ProjectDocumentCategory Category { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        public int? ProjectDocumentTemplateId { get; set; }

        [Required]
        public bool IsPublicAdvertisement { get; set; }
    }
}
