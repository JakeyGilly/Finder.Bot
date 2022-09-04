using Finder.Bot.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace Finder.Bot.Repositories;

public class Repository<T> where T : class {
    protected readonly ApplicationContext Context;
    protected Repository(ApplicationContext context) {
        Context = context;
    }
    
    public async Task<T?> Get(ulong id) {
        return await Context.Set<T>().FindAsync(id);
    }

    public async Task<T?> GetASync(ulong id, ulong id2) {
        return await Context.Set<T>().FindAsync(id, id2);
    }

    public async Task<T?> GetAsync(ulong id, ulong id2, ulong id3) {
        return await Context.Set<T>().FindAsync(id, id2, id3);
    }

    public async Task<IEnumerable<T>> GetAllAsync() {
        return await Context.Set<T>().ToListAsync();
    }

    public async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate) {
        return await Context.Set<T>().Where(predicate).ToListAsync();
    }

    public async Task AddAsync(T entity) {
        await Context.Set<T>().AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities) {
        await Context.Set<T>().AddRangeAsync(entities);
    }

    public void Remove(T entity) {
        Context.Set<T>().Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities) {
        Context.Set<T>().RemoveRange(entities);
    }

    public void Dispose() {
        Context.Dispose();
    }

    public int Save() {
        return Context.SaveChanges();
    }

    public async Task<int> SaveAsync() {
        return await Context.SaveChangesAsync();
    }
}