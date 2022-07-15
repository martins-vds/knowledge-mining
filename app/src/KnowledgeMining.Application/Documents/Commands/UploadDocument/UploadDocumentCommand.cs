using KnowledgeMining.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMining.Application.Documents.Commands.UploadDocument
{
    public readonly record struct Document(string Name, string ContentType, IDictionary<string, string>? Tags, Stream Content, bool LeaveOpen = false);
    public readonly record struct UploadDocumentCommand(IEnumerable<Document> Documents) : IRequest<Unit>;

    public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Unit>
    {
        private readonly IStorageService _storageService;

        public UploadDocumentCommandHandler(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<Unit> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
        {
            await _storageService.UploadDocuments(request.Documents, cancellationToken);

            return Unit.Value;
        }
    }
}
