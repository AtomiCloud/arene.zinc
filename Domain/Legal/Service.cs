using CarboxylicLithium;

namespace Domain.Legal;

public class LegalDocumentService(
  ILegalDocumentRepository repo,
  ITransactionManager tm
) : ILegalDocumentService
{
  public Task<Result<IEnumerable<LegalDocumentPrincipal>>> Search(LegalDocumentSearch search)
  {
    return repo.Search(search);
  }

  public Task<Result<LegalDocument?>> Get(Guid id)
  {
    return repo.Get(id);
  }

  public Task<Result<LegalDocument?>> GetByType(LegalDocumentType type)
  {
    return repo.GetByType(type);
  }

  public Task<Result<LegalDocument?>> GetByTypeAndVersion(LegalDocumentType type, uint version)
  {
    return repo.GetByTypeAndVersion(type, version);
  }

  public Task<Result<LegalDocumentPrincipal>> Create(CreateLegalDocumentRequest request)
  {
    return tm.Start(() => repo.Create(request));
  }

  public Task<Result<LegalDocumentPrincipal?>> Update(Guid id, UpdateLegalDocumentRequest request)
  {
    return tm.Start(() => repo.Update(id, request));
  }

  public Task<Result<LegalDocumentPrincipal?>> SetStatus(Guid id, LegalDocumentStatus status)
  {
    return tm.Start(() => repo.SetStatus(id, status));
  }

  public Task<Result<Unit?>> Delete(Guid id)
  {
    return repo.Delete(id);
  }

  public Task<Result<IEnumerable<LegalDocumentVersion>>> GetVersionHistory(Guid id)
  {
    return repo.GetVersionHistory(id);
  }
}
