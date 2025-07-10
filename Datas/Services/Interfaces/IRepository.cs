using Datas.Tools;
using FluentResults;

namespace Datas.Services.Interfaces;

public interface IReadRepository<TDto> where TDto : class
{
    Task<TDto> GetById(int id);
}
public interface IWriteRepository<T,TForm> where TForm : class
{
    Task<Result<T>> Add(TForm entity);
    Task<ResultEnum> Update(int id, TForm entity);
    Task<bool> Delete(int id);
}