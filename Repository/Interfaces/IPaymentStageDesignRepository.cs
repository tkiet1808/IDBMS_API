﻿using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IPaymentStageDesignRepository : ICrudBaseRepository<BusinessObject.Models.PaymentStageDesign, int>
    {
        IEnumerable<PaymentStageDesign> GetByDecorProjectDesignId(int designId);
    }
}
