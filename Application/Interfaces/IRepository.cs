using Application.Tools;
using FluentResults;

namespace Application.Interfaces;

public interface IReadRepository<TDto> where TDto : class
{
    Task<Result<TDto>> GetById(int id, int userId);
}
public interface IWriteRepository<T,TForm> where TForm : class
{
    Task<Result<T>> Add(TForm form, int userId);
    Task<Result> Update(int id, TForm entity, int userId);
    Task<Result> Delete(int id, int userId);
}
