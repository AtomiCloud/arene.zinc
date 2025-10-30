using CarboxylicLithium;

namespace Domain.Legal;

public interface ILegalDocumentService
{
  Task<Result<IEnumerable<LegalDocumentPrincipal>>> Search(LegalDocumentSearch search);
  Task<Result<LegalDocument?>> Get(Guid id);
  Task<Result<LegalDocument?>> GetByType(LegalDocumentType type);
  Task<Result<LegalDocument?>> GetByTypeAndVersion(LegalDocumentType type, uint version);
  Task<Result<LegalDocumentPrincipal>> Create(CreateLegalDocumentRequest request);
  Task<Result<LegalDocumentPrincipal?>> Update(Guid id, UpdateLegalDocumentRequest request);
  Task<Result<LegalDocumentPrincipal?>> SetStatus(Guid id, LegalDocumentStatus status);
  Task<Result<Unit?>> Delete(Guid id);
  Task<Result<IEnumerable<LegalDocumentVersion>>> GetVersionHistory(Guid id);
}
