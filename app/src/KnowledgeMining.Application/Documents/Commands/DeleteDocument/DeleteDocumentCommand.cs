using FluentValidation;
using KnowledgeMining.Application.Common.Interfaces;
using MediatR;

namespace KnowledgeMining.Application.Documents.Commands.DeleteDocument
{
    public readonly record struct DeleteDocumentCommand(string DocumentName) : IRequest<Unit>;

    public class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
    {
        public DeleteDocumentCommandValidator()
        {
            RuleFor(x => x.DocumentName).NotEmpty().WithMessage("Document name must be provided.");
        }
    }

    public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Unit>
    {
        private readonly IStorageService _storageService;
        private readonly ISearchService _searchService;

        public DeleteDocumentCommandHandler(IStorageService storageService, ISearchService searchService)
        {
            _storageService = storageService;
            _searchService = searchService;
        }

        public async Task<Unit> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
        {
            await _storageService.DownloadDocument(request.DocumentName, cancellationToken);
            await _searchService.QueueIndexerJob(cancellationToken);

            return Unit.Value;
        }
    }
}
