
using APICatalogo.Models;

namespace APICatalogo.Repositorio
{
  public class UnitOfwork : IUnitOfwork
  {
    private IProdutoRepository _produtoRepository;

    private ICategoriaRepository _categoriaRepository;
    public CatalogoDbEfPowerContext _context;

    public UnitOfwork(CatalogoDbEfPowerContext context)
    {
      _context = context;
    }

    public IProdutoRepository ProdutoRepository
    {
      get 
      {

        return _produtoRepository = _produtoRepository ?? new ProdutoRepository(_context);

        //if(_produtoRepository == null)
        //{
        //  new ProdutoRepositoryGenerico(_context);
        //}
        //return _produtoRepository;
      }
    }

    public ICategoriaRepository CategoriaRepository
    {
      get
      {
        return _categoriaRepository = _categoriaRepository ?? new CategoriaRepository(_context);
      }
    }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
    {
      _context.Dispose();
    }
  }
}
