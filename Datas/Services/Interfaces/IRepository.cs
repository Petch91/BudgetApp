using Datas.Tools;

namespace Datas.Services.Interfaces;

public interface IReadRepository<TDto> where TDto : class
{
    Task<TDto> GetById(int id);
}
public interface IWriteRepository<TForm> where TForm : class
{
    Task<Result> Add(TForm entity);
    Task<Result> Update(int id, TForm entity);
    Task<bool> Delete(int id);
}