using Flights.Domain.Interfaces;
using Flights.Domain.Models;
using MediatR;

namespace Flights.Application.Features.Documents.CreateDocument;

public class CreateDocumentHandler : IRequestHandler<CreateDocumentCommand, CreateDocumentDto>
{
    private readonly IEncryptionService _encryptionService;
    private readonly IDocumentRepository _documentRepo;
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateDocumentHandler(
        IEncryptionService encryptionService, 
        IDocumentRepository documentRepo,
        IUnitOfWork unitOfWork)
    {
        _encryptionService = encryptionService;
        _documentRepo = documentRepo;
        _unitOfWork = unitOfWork;
    }
    public async Task<CreateDocumentDto> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = Document.Create(
            request.Type, 
            request.FirstName,
            request.MiddleName,
            request.LastName,
            request.Number,
            request.Series,
            request.Gender,
            request.DateOfBirth,
            request.ValidityPeriod,
            request.PassengerId,
            request.UserId);
        
        var encryptedSeries = document.Series is not null ?
            _encryptionService.Encrypt(document.Series) : null;
        
        var encryptedNumber = _encryptionService.Encrypt(document.Number);
        document.SetEncryptedData(encryptedNumber, encryptedSeries);
        
        await _documentRepo.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
        //TODO: переделать всё нахуй строки енамы вперед!!!!!!!
        return CreateDocumentDto.Map(document);
    }
}