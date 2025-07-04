using Datas.Services.Interfaces;
using Datas.Services.MapperProjection;
using Datas.Tools;
using Entities.Dtos;
using Entities.Forms;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Datas.Services;

public class CategorieService(MyDbContext context) :  ICategorieService
{
    public async Task<CategorieDto> GetById(int id)
    {
        var categorie = await context.Categories.Where(d => d.Id == id).Select(ProjectionDto.CategorieAsDto).FirstOrDefaultAsync();
        if (categorie == null) return null;
        return categorie;
    }

    public async Task<IEnumerable<CategorieDto>> GetCategories()
    {
        var categories = await context.Categories.Select(ProjectionDto.CategorieAsDto).ToListAsync();
        ///if (categories == null) return null;
        return categories;
    }

    public async Task<Result> Add(CategorieForm entity)
    {
        context.Categories.Add(new Categorie{Name = entity.Name});
        var result = await context.SaveChangesAsync();
        return result > 0 ? Result.Success : Result.Failure;
    }

    public async Task<Result> Update(int id, CategorieForm entity)
    {
        var categorie = await context.TransactionsVariables.FindAsync(id);
        if (categorie == null) return Result.NotFound; //return not found
        
        var result = await context.Categories
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(f => f.SetProperty(a => a.Name, entity.Name));
        return result > 0 ? Result.Success : Result.NotFound; 
    }

    public async Task<bool> Delete(int id)
    {
        var result = await context.Categories.Where(d => d.Id == id).ExecuteDeleteAsync();
        return result > 0;
    }
}