using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Application.Entities
{
    public class SearchRequestToResult
    {
        [Key]
        public Guid Id { get; set; }
        public Guid SearchRequestId { get; set; }
        public Guid SearchResultId { get; set; }

        public SearchRequest SearchRequest { get; set; } = null!;
        public SearchResult SearchResult { get; set; } = null!;
    }
}
