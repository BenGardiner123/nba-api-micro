using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DTOs
{
    public class PaginationDTO
    {
        public int Page { get; set; }
        private int resordsPerPage = 20;
        private readonly int maxAmount = 50;

        public int RecordsPerPage
        {
            get
            {
                return RecordsPerPage;
            }
            set
            {
                //so if the value of records is over the max amount it will default ot max amount otherwise what
                //whatever they input will be fine.
                RecordsPerPage = (value > maxAmount) ? maxAmount : value;
            }
        }

    }
}
