using KnowledgeMining.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMining.Application.Common.Interfaces
{
    public interface IChatService
    {
        Task<ChatAnswer> AskQuestionAboutDocument(string question, string content, string documentId = "", CancellationToken ct = default);
    }
}
