using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMining.Application.Common.Interfaces
{
    public interface IChatService
    {
        Task<string> AskQuestionAboutDocument(string question, string document, CancellationToken ct = default);
    }
}
