using Microsoft.EntityFrameworkCore;
using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Interfaces;
using QuranListeningApp.Infrastructure.Data;

namespace QuranListeningApp.Infrastructure.Repositories;

public class SurahReferenceRepository : ISurahReferenceRepository
{
    private readonly IDbContextFactory<QuranAppDbContext> _contextFactory;

    public SurahReferenceRepository(IDbContextFactory<QuranAppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<SurahReference?> GetByNumberAsync(int surahNumber)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.SurahReferences
            .FirstOrDefaultAsync(s => s.SurahNumber == surahNumber);
    }

    public async Task<IEnumerable<SurahReference>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.SurahReferences
            .OrderBy(s => s.SurahNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<SurahReference>> GetMakkiSurahsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.SurahReferences
            .Where(s => s.IsMakki)
            .OrderBy(s => s.SurahNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<SurahReference>> GetMadaniSurahsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.SurahReferences
            .Where(s => !s.IsMakki)
            .OrderBy(s => s.SurahNumber)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int surahNumber)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.SurahReferences.AnyAsync(s => s.SurahNumber == surahNumber);
    }

    public async Task<int> GetTotalCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.SurahReferences.CountAsync();
    }
}
