namespace APICatalogo.Repositorio
{
  public interface IUnitOfwork
  {
    IProdutoRepository ProdutoRepository { get; }
    ICategoriaRepository CategoriaRepository { get; }
        Task CommitAsync();
    }
}
