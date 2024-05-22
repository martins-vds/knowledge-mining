using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMining.Application.Common.Models
{
    public class EmbeddedReport
    {
        public Guid Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
