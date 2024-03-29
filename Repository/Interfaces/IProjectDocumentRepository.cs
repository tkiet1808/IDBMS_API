﻿using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IProjectDocumentRepository : ICrudBaseRepository<ProjectDocument, Guid>
    {
        IEnumerable<ProjectDocument> GetByFilter(Guid? projectId, int? documentTemplateId);
        IEnumerable<ProjectDocument> GetByProjectId(Guid id);
        public ProjectDocument? GetContractById(Guid id);
    }
}
