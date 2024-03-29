﻿using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IRoomRepository : ICrudBaseRepository<Room, Guid>
    {
        IEnumerable<Room> GetByProjectId(Guid id);
        IEnumerable<Room> GetByFloorId(Guid id);
    }
}
