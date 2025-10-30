using CarboxylicLithium;

namespace Domain;

public interface ITransactionManager
{
  Task<Result<T>> Start<T>(Func<Task<Result<T>>> func);
}
