namespace Datas.Services.Interfaces;

public interface IReadRepository<TDto> where TDto : class
{
    Task<TDto> GetById(int id);
}
public interface IWriteRepository<TForm> where TForm : class
{
    Task<bool> Add(TForm entity);
    Task<bool> Update(int id, TForm entity);
    Task<bool> Delete(int id);
}