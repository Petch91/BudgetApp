using Application.Tools;
using FluentResults;

namespace Application.Interfaces;

public interface IReadRepository<TDto> where TDto : class
{
    Task<Result<TDto>> GetById(int id);
}
public interface IWriteRepository<T,TForm> where TForm : class
{
    Task<Result<T>> Add(TForm form);
    Task<Result> Update(int id, TForm entity);
    Task<Result> Delete(int id);
}