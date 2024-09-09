
using APICatalogo.Models;
using APICatalogo.Pagination;
using X.PagedList;


namespace APICatalogo.Repositorio
{
  public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
  {
    public CategoriaRepository(CatalogoDbEfPowerContext context) : base(context)
    {
    }

        public async Task< IPagedList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriasFiltroNome categoriasParams)
        {
            var categorias = await  GetAllAsync();
            var resultado = categorias.AsQueryable();


            if (!string.IsNullOrEmpty(categoriasParams.Nome))
            {
                categorias = categorias.Where(c => c.Nome.Contains(categoriasParams.Nome));
            }

            var categoriasFiltradas = await categorias.ToPagedListAsync(categoriasParams.PageNumber, categoriasParams.PageSize); 
               

           return categoriasFiltradas;
        }



    
  }
}
