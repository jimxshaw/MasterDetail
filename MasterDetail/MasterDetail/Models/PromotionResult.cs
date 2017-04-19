using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetail.Models
{
    public class PromotionResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public PromotionResult()
        {
            Success = false;
        }
    }
}
