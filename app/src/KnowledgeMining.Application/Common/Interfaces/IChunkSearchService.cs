using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMining.Application.Common.Interfaces
{
    public interface IChunkSearchService
    {
        Task<IEnumerable<string>> QueryDocumentChuncksAsync(float[] embedding, string document, CancellationToken cancellationToken = default);
    }
}
